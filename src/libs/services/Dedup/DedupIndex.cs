namespace TNO.Services.Dedup;

/// <summary>
/// DedupIndex class, an in-memory index of recently ingested stories used to find duplicates.
/// A story is a duplicate when an equal or higher priority story published within the window has
/// the same or a similar headline, or nearly identical text.
/// </summary>
public class DedupIndex
{
    #region Variables
    private readonly object _lock = new();
    private readonly Dictionary<string, DedupEntry> _entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<DedupEntry>> _byToken = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DateTime> _skipped = new(StringComparer.Ordinal);
    #endregion

    #region Properties
    /// <summary>
    /// get - The version of the configuration the index was loaded for.
    /// </summary>
    public long? Version { get; private set; }

    /// <summary>
    /// get - When the index was last loaded (UTC).
    /// </summary>
    public DateTime? LoadedOn { get; private set; }

    /// <summary>
    /// get - Number of stories in the index.
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock) return _entries.Count;
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Replace the stories in the index.
    /// The record of skipped duplicates is kept.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="version">The version of the configuration the entries were loaded for.</param>
    /// <param name="loadedOn"></param>
    public void Load(IEnumerable<DedupEntry> entries, long? version, DateTime loadedOn)
    {
        lock (_lock)
        {
            _entries.Clear();
            _byToken.Clear();
            foreach (var entry in entries) AddEntry(entry);
            this.Version = version;
            this.LoadedOn = loadedOn;
        }
    }

    /// <summary>
    /// Add a story to the index.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns>False if the story is already in the index.</returns>
    public bool Add(DedupEntry entry)
    {
        lock (_lock) return AddEntry(entry);
    }

    /// <summary>
    /// Find the story the candidate duplicates.
    /// Only stories from an equal or higher priority feed (lower or equal Priority) are considered,
    /// so a higher priority copy that arrives late is never skipped in favour of a lower priority one.
    /// </summary>
    /// <param name="candidate"></param>
    /// <param name="options"></param>
    /// <param name="match">The best match, the highest score then the highest priority.</param>
    /// <returns>True if the candidate is a duplicate.</returns>
    public bool TryFindMatch(DedupEntry candidate, DedupOptions options, out DedupMatch? match)
    {
        var window = TimeSpan.FromDays(Math.Max(options.WindowDays, 0));
        var compareHeadlines = candidate.HeadlineTokens.Count >= options.MinHeadlineTokens;
        DedupMatch? best = null;

        bool IsEligible(DedupEntry entry) =>
            !entry.IsSame(candidate.Source, candidate.Uid)
            && entry.Priority <= candidate.Priority
            && (entry.PublishedOn - candidate.PublishedOn).Duration() <= window;

        void Consider(DedupEntry entry, DedupMatchReason reason, double score)
        {
            if (best == null
                || score > best.Score
                || (score == best.Score && entry.Priority < best.Primary.Priority)
                || (score == best.Score && entry.Priority == best.Primary.Priority && entry.PublishedOn < best.Primary.PublishedOn))
                best = new DedupMatch(entry, reason, score);
        }

        lock (_lock)
        {
            if (compareHeadlines)
            {
                // Only stories that share at least one headline word can have similar headlines.
                var candidates = new HashSet<DedupEntry>(ReferenceEqualityComparer.Instance);
                foreach (var token in candidate.HeadlineTokens)
                {
                    if (_byToken.TryGetValue(token, out var entries)) candidates.UnionWith(entries);
                }

                foreach (var entry in candidates.Where(IsEligible))
                {
                    if (entry.Headline == candidate.Headline)
                    {
                        Consider(entry, DedupMatchReason.ExactHeadline, 1);
                        continue;
                    }

                    var similarity = TextNormalizer.Jaccard(candidate.HeadlineTokens, entry.HeadlineTokens);
                    if (similarity >= options.HeadlineThreshold) Consider(entry, DedupMatchReason.FuzzyHeadline, similarity);
                }
            }

            if (candidate.BodyHash.HasValue)
            {
                // Fingerprints are compared directly, a headline can be rewritten while the story is not.
                foreach (var entry in _entries.Values.Where(e => e.BodyHash.HasValue && IsEligible(e)))
                {
                    var similarity = SimHash.Similarity(candidate.BodyHash.Value, entry.BodyHash!.Value);
                    if (similarity >= options.BodyThreshold) Consider(entry, DedupMatchReason.Body, similarity);
                }
            }
        }

        match = best;
        return best != null;
    }

    /// <summary>
    /// Remove stories (and skipped duplicates) published before the cutoff.
    /// </summary>
    /// <param name="cutoff"></param>
    /// <returns>Number of stories removed.</returns>
    public int Prune(DateTime cutoff)
    {
        var utcCutoff = cutoff.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(cutoff, DateTimeKind.Utc) : cutoff.ToUniversalTime();
        lock (_lock)
        {
            var expired = _entries.Where(e => e.Value.PublishedOn < utcCutoff).Select(e => e.Key).ToArray();
            foreach (var key in expired)
            {
                var entry = _entries[key];
                _entries.Remove(key);
                foreach (var token in entry.HeadlineTokens)
                {
                    if (!_byToken.TryGetValue(token, out var entries)) continue;
                    entries.Remove(entry);
                    if (entries.Count == 0) _byToken.Remove(token);
                }
            }

            foreach (var key in _skipped.Where(s => s.Value < utcCutoff).Select(s => s.Key).ToArray())
            {
                _skipped.Remove(key);
            }

            return expired.Length;
        }
    }

    /// <summary>
    /// Record that a duplicate was skipped.
    /// </summary>
    /// <param name="entry"></param>
    /// <returns>True the first time the story is skipped, so callers can log it once.</returns>
    public bool MarkSkipped(DedupEntry entry)
    {
        lock (_lock) return _skipped.TryAdd(GetKey(entry.Source, entry.Uid), entry.PublishedOn);
    }

    private bool AddEntry(DedupEntry entry)
    {
        if (!_entries.TryAdd(GetKey(entry.Source, entry.Uid), entry)) return false;
        foreach (var token in entry.HeadlineTokens)
        {
            if (!_byToken.TryGetValue(token, out var entries))
            {
                entries = new List<DedupEntry>();
                _byToken.Add(token, entries);
            }
            entries.Add(entry);
        }
        return true;
    }

    private static string GetKey(string source, string uid) => $"{source}:{uid}";
    #endregion
}
