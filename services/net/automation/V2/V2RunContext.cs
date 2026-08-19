using System.Text.Json;

namespace TNO.Services.Automation.V2;

/// <summary>
/// V2ContentEntry class, one item in the run context: a reference plus a projected digest and the
/// deltas actions have accumulated. Entries are shared - the same content item appearing in two
/// collections is one entry, so a change made in one step is visible everywhere. Full content
/// models are never held here; they are fetched transiently at flush.
/// </summary>
public class V2ContentEntry
{
    /// <summary>get/set - 'existing' (has a database id) or 'draft' (created during the run).</summary>
    public string Kind { get; set; } = "existing";

    /// <summary>get/set - The database id (0 for an unsaved draft).</summary>
    public long Id { get; set; }

    /// <summary>get/set - The temporary key of a draft, unique within the run.</summary>
    public string? TempKey { get; set; }

    /// <summary>get - Projected field values (headline, byline, body, publishedOn, ...).</summary>
    public Dictionary<string, string?> Digest { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - The changes actions have accumulated for this item.</summary>
    public V2Deltas Deltas { get; } = new();

    /// <summary>get - A stable key for dictionaries and self-comparison.</summary>
    public string Key => Kind == "draft" ? $"draft:{TempKey}" : $"id:{Id}";

    /// <summary>
    /// Resolve a field from the working copy: deltas override the digest, and the delta-only
    /// virtual fields (tags, sentiment, status) merge with what the digest carried.
    /// </summary>
    public string? GetField(string field)
    {
        lock (Deltas)
        {
            if (Deltas.Fields.TryGetValue(field, out var overridden)) return overridden;
            if (field.Equals("tags", StringComparison.OrdinalIgnoreCase) && Deltas.Tags.Count > 0)
            {
                var baseTags = Digest.TryGetValue("tags", out var t) ? t : null;
                var all = (baseTags ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Concat(Deltas.Tags.Select(tag => tag.Code)).Distinct(StringComparer.OrdinalIgnoreCase);
                return string.Join(",", all);
            }
            if (field.Equals("sentiment", StringComparison.OrdinalIgnoreCase) && Deltas.Sentiment.HasValue)
                return Deltas.Sentiment.Value.ToString();
            if (field.Equals("status", StringComparison.OrdinalIgnoreCase) && Deltas.Status != null)
                return Deltas.Status;
        }
        return Digest.TryGetValue(field, out var value) ? value : null;
    }

    /// <summary>
    /// Serialize the working copy (digest with deltas folded in) for prompts, so later analyses
    /// see what earlier steps changed whether or not it has been written to the database.
    /// </summary>
    public string ToWorkingJson(JsonSerializerOptions options)
    {
        var view = new Dictionary<string, string?>(Digest, StringComparer.OrdinalIgnoreCase);
        lock (Deltas)
        {
            foreach (var (field, value) in Deltas.Fields) view[field] = value;
            if (Deltas.Tags.Count > 0) view["tags"] = GetField("tags");
            if (Deltas.Sentiment.HasValue) view["sentiment"] = Deltas.Sentiment.Value.ToString();
            if (Deltas.Status != null) view["status"] = Deltas.Status;
            if (Deltas.ContributorName != null) view["contributor"] = Deltas.ContributorName;
        }
        view["id"] = Kind == "draft" ? TempKey : Id.ToString();
        return JsonSerializer.Serialize(view, options);
    }
}

/// <summary>
/// V2Deltas class, the changes accumulated for one item. A handful of strings rather than a full
/// content model - holding full models for every dirty item until end-of-run is exactly the
/// memory problem the v2 engine avoids. Lock the instance when mutating (items run in parallel
/// but an entry can be shared).
/// </summary>
public class V2Deltas
{
    public Dictionary<string, string> Fields { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<(int Id, string Code, string Name)> Tags { get; } = new();
    public int? Sentiment { get; set; }
    public int? ContributorId { get; set; }
    public string? ContributorName { get; set; }
    public List<int> ContentActionIds { get; } = new();
    /// <summary>'publish' or 'unpublish'; null when no status change is pending.</summary>
    public string? Status { get; set; }
    public bool Dirty => Fields.Count > 0 || Tags.Count > 0 || Sentiment.HasValue || ContributorId.HasValue || ContentActionIds.Count > 0 || Status != null;
}

/// <summary>
/// V2RunContext class, the whole state of a run: named collections, scores, exclusions, and the
/// shared entry registry. Replaces the v1 engine's per-purpose dictionaries. Mutations of the
/// run-scoped members are serialized through <see cref="Sync"/>; per-iteration state lives in
/// <see cref="V2ItemScope"/> and needs no locking.
/// </summary>
public class V2RunContext
{
    /// <summary>get - The lock guarding collections, scores, and exclusions.</summary>
    public object Sync { get; } = new();

    /// <summary>get - Named collections ($run.*). Ordered lists of shared entries.</summary>
    public Dictionary<string, List<V2ContentEntry>> Collections { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - Shared entry registry: one entry per existing content item.</summary>
    public Dictionary<long, V2ContentEntry> EntriesById { get; } = new();

    /// <summary>get - All drafts created during the run (persisted at flush).</summary>
    public List<V2ContentEntry> Drafts { get; } = new();

    /// <summary>get - Scores: objective -> (entry key -> score). Written by 'score', read by 'select-top'.</summary>
    public Dictionary<string, Dictionary<string, int>> Scores { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - Excluded entry keys with their reasons. Excluded items skip all later steps
    /// but keep their accumulated deltas - exclusion never discards changes.</summary>
    public Dictionary<string, string> Excluded { get; } = new();

    /// <summary>get - Draft temp key -> database id, recorded at flush for the run summary.</summary>
    public Dictionary<string, long> DraftIds { get; } = new();

    /// <summary>
    /// Get (or register) the shared entry for an existing content item.
    /// </summary>
    public V2ContentEntry GetOrAddEntry(long id, Func<V2ContentEntry> factory)
    {
        lock (Sync)
        {
            if (EntriesById.TryGetValue(id, out var existing)) return existing;
            var entry = factory();
            EntriesById[id] = entry;
            return entry;
        }
    }

    /// <summary>
    /// Get a collection, creating it when absent.
    /// </summary>
    public List<V2ContentEntry> GetCollection(string name)
    {
        lock (Sync)
        {
            if (!Collections.TryGetValue(name, out var list))
            {
                list = new List<V2ContentEntry>();
                Collections[name] = list;
            }
            return list;
        }
    }

    /// <summary>
    /// Every dirty entry (existing items with pending deltas) plus every unsaved draft.
    /// </summary>
    public List<V2ContentEntry> GetFlushables()
    {
        lock (Sync)
        {
            return EntriesById.Values.Where(e => e.Deltas.Dirty)
                .Concat(Drafts.Where(d => d.Kind == "draft" || d.Deltas.Dirty))
                .Distinct()
                .ToList();
        }
    }
}

/// <summary>
/// V2ItemScope class, per-iteration state: the subject, analysis results, chained conversations,
/// and drafts created this iteration. Isolated per parallel item - no locking needed.
/// </summary>
public class V2ItemScope
{
    public V2ItemScope(V2ContentEntry? subject)
    {
        Subject = subject;
    }

    /// <summary>get - The item this iteration processes (null in init/complete steps).</summary>
    public V2ContentEntry? Subject { get; }

    /// <summary>get - Parsed structured analysis results by analysis name.</summary>
    public Dictionary<string, JsonDocument> Structured { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - Raw analysis responses by analysis name.</summary>
    public Dictionary<string, string> Raw { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - Conversations for chained analyses, keyed by the chain root's name.</summary>
    public Dictionary<string, List<(string Role, string Content)>> Conversations { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get - Drafts created this iteration by name ($item.*).</summary>
    public Dictionary<string, V2ContentEntry> Drafts { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>get/set - Set when 'abort' stops the remaining actions of this step instance.</summary>
    public bool Aborted { get; set; }

    /// <summary>get/set - Set when 'exclude' removes the subject from later steps.</summary>
    public bool Excluded { get; set; }

    /// <summary>
    /// Resolve an action's target: the named draft, or the subject when no target is given.
    /// </summary>
    public V2ContentEntry? ResolveTarget(string? target)
    {
        if (string.IsNullOrWhiteSpace(target)) return Subject;
        return Drafts.TryGetValue(target, out var draft) ? draft : null;
    }
}
