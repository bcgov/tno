using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Entities;

namespace TNO.DAL.Analysis;

/// <summary>
/// AnalysisPopulator class, applies accepted analysis to empty editorial fields. It fills a field
/// only when it is empty and not cleared by a person, refreshes or removes only values analysis
/// owns, and never changes a value a person or automation set. It never creates tags or
/// contributors; it creates topics only when topic population allows it.
/// </summary>
public partial class AnalysisPopulator
{
    #region Variables
    [GeneratedRegex(@"[^\p{L}\p{N}]+")]
    private static partial Regex NonWordRegex();

    private readonly TNOContext _context;
    private readonly ContentAnalysisSettings _settings;
    private readonly Content _content;
    private readonly ContentAnalysis _analysis;
    private readonly List<ContentFieldOwnership> _ownership;
    #endregion

    #region Properties
    /// <summary>
    /// get - The fields populated or refreshed.
    /// </summary>
    public List<string> PopulatedFields { get; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisPopulator.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="settings"></param>
    /// <param name="content">The tracked content, with its tags, topics, and quotes.</param>
    /// <param name="analysis">The saved analysis.</param>
    public AnalysisPopulator(TNOContext context, ContentAnalysisSettings settings, Content content, ContentAnalysis analysis)
    {
        _context = context;
        _settings = settings;
        _content = content;
        _analysis = analysis;
        _ownership = context.ContentFieldOwnerships.Where(o => o.ContentId == content.Id).ToList();
    }
    #endregion

    #region Methods
    /// <summary>
    /// A deterministic key for matching names: lower case, words only, single spaces.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Normalize(string? value) => NonWordRegex().Replace((value ?? "").ToLowerInvariant(), " ").Trim();

    /// <summary>
    /// Apply the analysis to the fields of the processes the worker ran.
    /// </summary>
    /// <param name="result"></param>
    public void Populate(AnalysisResultModel result)
    {
        if (result.Processes.HasFlag(AnalysisProcess.Summary)) PopulateSummary(result.Summary);
        if (result.Processes.HasFlag(AnalysisProcess.Contributor)) PopulateContributor(result.SuggestedContributor);
        if (result.Processes.HasFlag(AnalysisProcess.Tags)) PopulateTags(result.SuggestedTags);
        if (result.Processes.HasFlag(AnalysisProcess.Quotes)) PopulateQuotes(result.Quotes);
        if (result.Processes.HasFlag(AnalysisProcess.Topics)) PopulateTopic(_analysis.AnalysisTopic, result.PrimaryTopic);
    }

    private ContentFieldOwnership? Find(string field, string key = "")
        => _ownership.FirstOrDefault(o => o.Field == field && o.ValueKey == key);

    private bool IsCleared(string field, string key = "") => Find(field, key)?.IsCleared == true;

    private bool IsAnalysisOwned(string field, string key = "")
    {
        var record = Find(field, key);
        return record != null && record.Owner == FieldOwner.Analysis && !record.IsCleared;
    }

    private void Own(string field, string key)
    {
        var record = Find(field, key);
        if (record == null)
        {
            record = new ContentFieldOwnership(_content.Id, field, key, FieldOwner.Analysis) { AnalysisId = _analysis.Id };
            _context.ContentFieldOwnerships.Add(record);
            _ownership.Add(record);
        }
        else
        {
            record.Owner = FieldOwner.Analysis;
            record.IsCleared = false;
            record.AnalysisId = _analysis.Id;
        }
    }

    private void Disown(string field, string key)
    {
        var record = Find(field, key);
        if (record == null) return;
        _context.ContentFieldOwnerships.Remove(record);
        _ownership.Remove(record);
    }

    /// <summary>
    /// Fill an empty summary; refresh one analysis wrote.
    /// </summary>
    private void PopulateSummary(string summary)
    {
        const string field = ContentFieldOwnership.SummaryField;
        if (String.IsNullOrWhiteSpace(summary)) return;
        var isEmpty = String.IsNullOrWhiteSpace(_content.Summary);
        if ((isEmpty && !IsCleared(field)) || IsAnalysisOwned(field))
        {
            if (_content.Summary == summary) return;
            _content.Summary = summary;
            Own(field, "");
            PopulatedFields.Add("summary");
        }
    }

    /// <summary>
    /// Set an empty contributor when the byline or columnist matches an existing contributor or alias.
    /// </summary>
    private void PopulateContributor(string? suggested)
    {
        const string field = ContentFieldOwnership.ContributorField;
        var isEmpty = !_content.ContributorId.HasValue;
        if (!((isEmpty && !IsCleared(field)) || IsAnalysisOwned(field))) return;

        var match = MatchContributor(suggested);
        if (match == _content.ContributorId) return;
        if (match == null && isEmpty) return;

        _content.ContributorId = match;
        if (match.HasValue) Own(field, "");
        else Disown(field, "");
        PopulatedFields.Add("contributor");
    }

    private int? MatchContributor(string? name)
    {
        var key = Normalize(name);
        if (key.Length == 0) return null;
        var contributors = _context.Contributors.AsNoTracking()
            .Where(c => c.IsEnabled)
            .Select(c => new { c.Id, c.Name, c.Aliases })
            .ToArray();
        return contributors.FirstOrDefault(c => Normalize(c.Name) == key
                || (c.Aliases ?? "").Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries).Any(a => Normalize(a) == key))
            ?.Id;
    }

    /// <summary>
    /// Add matched existing tags when the content has none a person or automation set.
    /// </summary>
    private void PopulateTags(IEnumerable<string> suggested)
    {
        const string field = ContentFieldOwnership.TagField;
        var keys = suggested.Select(Normalize).Where(k => k.Length > 0).ToHashSet();
        var tags = _context.Tags.AsNoTracking().Where(t => t.IsEnabled).Select(t => new { t.Id, t.Code, t.Name }).ToArray();
        var matches = tags.Where(t => keys.Contains(Normalize(t.Code)) || keys.Contains(Normalize(t.Name))).Select(t => t.Id).ToHashSet();

        var current = _content.TagsManyToMany.ToArray();
        var ownedByOthers = current.Where(t => !IsAnalysisOwned(field, t.TagId.ToString())).ToArray();
        var changed = false;

        // Remove tags analysis added that the new analysis no longer supports.
        foreach (var tag in current.Where(t => IsAnalysisOwned(field, t.TagId.ToString()) && !matches.Contains(t.TagId)))
        {
            _content.TagsManyToMany.Remove(tag);
            _context.ContentTags.Remove(tag);
            Disown(field, tag.TagId.ToString());
            changed = true;
        }

        // Only an empty field (no tags a person or automation set) is filled.
        if (ownedByOthers.Length == 0 && !IsCleared(field))
        {
            foreach (var tagId in matches.Where(id => current.All(t => t.TagId != id) && !IsCleared(field, id.ToString())))
            {
                _content.TagsManyToMany.Add(new ContentTag(_content.Id, tagId));
                Own(field, tagId.ToString());
                changed = true;
            }
        }
        if (changed) PopulatedFields.Add("tags");
    }

    /// <summary>
    /// Add validated verbatim quotes that are not already on the content; remove quotes analysis
    /// added that the new analysis no longer finds. Existing quotes keep their IDs and relevance.
    /// </summary>
    private void PopulateQuotes(IEnumerable<AnalysisQuoteModel> quotes)
    {
        const string field = ContentFieldOwnership.QuoteField;
        var incoming = quotes
            .Where(q => !String.IsNullOrWhiteSpace(q.Statement))
            .GroupBy(q => AnalysisInput.QuoteKey(q.Statement))
            .ToDictionary(g => g.Key, g => g.First());
        var existing = _content.Quotes.ToArray();
        var existingKeys = existing.Select(q => AnalysisInput.QuoteKey(q.Statement)).ToHashSet();
        var changed = false;

        foreach (var quote in existing.Where(q => q.Owner == FieldOwner.Analysis && !incoming.ContainsKey(AnalysisInput.QuoteKey(q.Statement))))
        {
            _content.Quotes.Remove(quote);
            _context.Quotes.Remove(quote);
            Disown(field, AnalysisInput.QuoteKey(quote.Statement));
            changed = true;
        }

        foreach (var (key, quote) in incoming.Where(q => !existingKeys.Contains(q.Key) && !IsCleared(field, q.Key)))
        {
            _content.Quotes.Add(new Quote(_content.Id, quote.Statement.Trim(), quote.Speaker.Trim(), true)
            {
                Owner = FieldOwner.Analysis,
                AnalysisId = _analysis.Id,
                SourceStart = quote.Span?.Start,
                SourceLength = quote.Span?.Length,
            });
            Own(field, key);
            changed = true;
        }
        if (changed) PopulatedFields.Add("quotes");
    }

    /// <summary>
    /// Assign the primary topic when the content has no topic (other than the system placeholder).
    /// Scores come from the topic score rules, like any calculated score.
    /// </summary>
    private void PopulateTopic(AnalysisTopic? registry, string? label)
    {
        const string field = ContentFieldOwnership.TopicField;
        if (String.IsNullOrWhiteSpace(label) || IsCleared(field)) return;

        var systemTopicIds = _context.Topics.AsNoTracking().Where(t => t.IsSystem).Select(t => t.Id).ToHashSet();
        var current = _content.TopicsManyToMany.ToArray();
        if (current.Any(t => !systemTopicIds.Contains(t.TopicId) && !IsAnalysisOwned(field, t.TopicId.ToString()))) return;

        var topic = ResolveTopic(registry, label);
        if (topic == null) return;
        if (current.Any(t => t.TopicId == topic.Id)) return;

        // Replace the placeholder and any topic analysis assigned before.
        foreach (var existing in current)
        {
            _content.TopicsManyToMany.Remove(existing);
            _context.ContentTopics.Remove(existing);
            Disown(field, existing.TopicId.ToString());
        }
        _content.TopicsManyToMany.Add(new ContentTopic(_content.Id, topic.Id, 0));
        Own(field, topic.Id.ToString());
        PopulatedFields.Add("topics");
    }

    /// <summary>
    /// The staff topic for the label: the registry's match, an active topic of the same name, or,
    /// when topic creation is allowed, a new topic.
    /// </summary>
    private Topic? ResolveTopic(AnalysisTopic? registry, string label)
    {
        if (registry?.TopicId != null)
        {
            var matched = _context.Topics.FirstOrDefault(t => t.Id == registry.TopicId && t.IsEnabled && !t.IsSystem);
            if (matched != null) return matched;
        }
        var key = Normalize(label);
        var topics = _context.Topics.Where(t => !t.IsSystem).ToArray();
        var existing = topics.FirstOrDefault(t => Normalize(t.Name) == key);
        if (existing != null) return existing.IsEnabled ? existing : null; // A disabled topic is never revived.
        if (_settings.TopicPopulationMode != TopicPopulationMode.AllowCreate) return null;

        // Saved now, inside the acceptance transaction, so the content topic and ownership can use its ID.
        var created = new Topic(label.Trim(), TopicType.Issues);
        _context.Topics.Add(created);
        _context.SaveChanges();
        if (registry != null) registry.TopicId = created.Id;
        return created;
    }
    #endregion
}
