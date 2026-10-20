using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elasticsearch.Net;
using TNO.API.Models.Settings;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.TemplateEngine.FilterAI;

namespace TNO.API.Helpers;

/// <summary>
/// FilterAITools class, the read-only tools a filter AI model uses: reference values from the
/// database and searches against one content index. A subscriber's tools hide the sources and
/// media types the subscriber may not search, the same way their own searches do.
/// </summary>
public class FilterAITools : IFilterAITools
{
    #region Variables
    /// <summary>How long an index's described mapping is reused before it is read again.</summary>
    private static readonly TimeSpan MappingLifetime = TimeSpan.FromMinutes(30);

    /// <summary>The read mapping of each index, with when it was read.</summary>
    private static readonly ConcurrentDictionary<string, (DateTime ReadOn, FilterAIIndexSchema Schema)> Mappings = new();

    private readonly ITNOElasticClient _elasticClient;
    private readonly ISourceService _sourceService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISeriesService _seriesService;
    private readonly IContributorService _contributorService;
    private readonly ITagService _tagService;
    private readonly IActionService _actionService;
    private readonly string _index;
    private readonly string? _unpublishedIndex;
    private readonly int[] _excludedSourceIds;
    private readonly int[] _excludedMediaTypeIds;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterAITools object, initializes with specified parameters.
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="sourceService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="seriesService"></param>
    /// <param name="contributorService"></param>
    /// <param name="tagService"></param>
    /// <param name="actionService"></param>
    /// <param name="index">The content index searches run against.</param>
    /// <param name="unpublishedIndex">The index of all content, drafts included, when 'index' holds only published content; otherwise null.</param>
    /// <param name="excludedSourceIds">Sources hidden from lookups and searches.</param>
    /// <param name="excludedMediaTypeIds">Media types hidden from lookups and searches.</param>
    public FilterAITools(
        ITNOElasticClient elasticClient,
        ISourceService sourceService,
        IMediaTypeService mediaTypeService,
        ISeriesService seriesService,
        IContributorService contributorService,
        ITagService tagService,
        IActionService actionService,
        string index,
        string? unpublishedIndex,
        IEnumerable<int> excludedSourceIds,
        IEnumerable<int> excludedMediaTypeIds)
    {
        _elasticClient = elasticClient;
        _sourceService = sourceService;
        _mediaTypeService = mediaTypeService;
        _seriesService = seriesService;
        _contributorService = contributorService;
        _tagService = tagService;
        _actionService = actionService;
        _index = index;
        _unpublishedIndex = unpublishedIndex;
        _excludedSourceIds = excludedSourceIds.ToArray();
        _excludedMediaTypeIds = excludedMediaTypeIds.ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find enabled reference values of the specified kind whose name or code contains the text.
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="text"></param>
    /// <param name="limit"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyList<FilterAIReference>> LookupAsync(string kind, string? text, int limit, CancellationToken cancellationToken)
    {
        var values = All(kind, enabledOnly: true);
        var term = text?.Trim();
        if (!String.IsNullOrEmpty(term))
            values = values.Where(v => v.Name.Contains(term, StringComparison.OrdinalIgnoreCase)
                || (v.Code?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                || (v.Detail?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
        IReadOnlyList<FilterAIReference> result = values
            .OrderBy(v => String.Equals(v.Name, term, StringComparison.OrdinalIgnoreCase) || String.Equals(v.Code, term, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
            .ThenBy(v => v.Name)
            .Take(limit)
            .ToArray();
        return Task.FromResult(result);
    }

    /// <summary>
    /// Read the fields of the index from its live mapping. The result is reused for a while,
    /// since a mapping only changes when new fields are indexed or the index migrates.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FilterAIException"></exception>
    public async Task<FilterAIIndexSchema> GetIndexSchemaAsync(CancellationToken cancellationToken)
    {
        if (Mappings.TryGetValue(_index, out var cached) && DateTime.UtcNow - cached.ReadOn < MappingLifetime) return cached.Schema;

        var response = await _elasticClient.LowLevel.Indices.GetMappingAsync<StringResponse>(_index, ctx: cancellationToken);
        if (!response.Success)
            throw new FilterAIException(ElasticError(response.Body) ?? response.OriginalException?.Message ?? "Elasticsearch did not return the mapping.");
        var schema = FilterAISchema.Read(response.Body);
        Mappings[_index] = (DateTime.UtcNow, schema);
        return schema;
    }

    /// <summary>
    /// Run the search, excluding the sources and media types this user may not search.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="FilterAIException"></exception>
    public async Task<string> SearchAsync(JsonObject body, CancellationToken cancellationToken)
    {
        body["query"] = Restrict(body["query"]);
        var response = await _elasticClient.LowLevel.SearchAsync<StringResponse>(_index, PostData.String(body.ToJsonString()), ctx: cancellationToken);
        if (!response.Success)
            throw new FilterAIException(ElasticError(response.Body) ?? response.OriginalException?.Message ?? "Elasticsearch rejected the search.");
        return response.Body;
    }

    /// <summary>
    /// Whether searches include unpublished content (drafts).
    /// </summary>
    public bool SearchesUnpublished => _unpublishedIndex == null;

    /// <summary>
    /// Count the stories the search matches among all content, drafts included, when searches
    /// only cover published content; otherwise null.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<long?> CountUnpublishedAsync(JsonObject body, CancellationToken cancellationToken)
    {
        if (_unpublishedIndex == null) return null;
        var count = new JsonObject() { ["query"] = Restrict(body["query"]) };
        var response = await _elasticClient.LowLevel.CountAsync<StringResponse>(_unpublishedIndex, PostData.String(count.ToJsonString()), ctx: cancellationToken);
        if (!response.Success) return null;
        using var document = JsonDocument.Parse(response.Body);
        return document.RootElement.TryGetProperty("count", out var c) && c.TryGetInt64(out var n) ? n : null;
    }

    /// <summary>
    /// Validate the query body; returns null when it is valid, otherwise why not.
    /// </summary>
    /// <param name="body"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string?> ValidateAsync(JsonObject body, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(body.ToJsonString());
        var result = await _elasticClient.ValidateAsync(_index, document);
        if (result.Valid) return null;
        return result.Error
            ?? result.Explanations?.Select(e => e.Error).FirstOrDefault(e => !String.IsNullOrWhiteSpace(e))
            ?? "The query is not valid.";
    }

    /// <summary>
    /// Describe every reference in the settings that does not exist or that this user may not search.
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyList<string>> FindUnknownReferencesAsync(FilterSettingsModel settings, CancellationToken cancellationToken)
    {
        var problems = new List<string>();
        void Check(string kind, string label, IEnumerable<int>? ids)
        {
            if (ids == null || !ids.Any()) return;
            var known = All(kind, enabledOnly: false).Where(v => v.Id.HasValue).Select(v => v.Id!.Value).ToHashSet();
            problems.AddRange(ids.Where(id => !known.Contains(id)).Select(id => $"{label} id {id} does not exist (use the lookup tool)"));
        }

        Check(FilterAIReferenceKind.Source, "source", settings.SourceIds);
        Check(FilterAIReferenceKind.MediaType, "media type", settings.MediaTypeIds);
        Check(FilterAIReferenceKind.Series, "series", settings.SeriesIds);
        Check(FilterAIReferenceKind.Contributor, "contributor", settings.ContributorIds);
        Check(FilterAIReferenceKind.Action, "action", settings.Actions?.Select(a => a.Id));
        if (settings.Tags?.Length > 0)
        {
            var codes = All(FilterAIReferenceKind.Tag, enabledOnly: false).Select(v => v.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);
            problems.AddRange(settings.Tags.Where(code => !codes.Contains(code)).Select(code => $"tag code '{code}' does not exist (use the lookup tool)"));
        }
        IReadOnlyList<string> result = problems;
        return Task.FromResult(result);
    }

    /// <summary>
    /// Every reference value of the kind this user may see.
    /// </summary>
    private IEnumerable<FilterAIReference> All(string kind, bool enabledOnly)
    {
        return kind switch
        {
            FilterAIReferenceKind.Source => _sourceService.FindAll()
                .Where(s => (!enabledOnly || s.IsEnabled) && !_excludedSourceIds.Contains(s.Id))
                .Select(s => new FilterAIReference(s.Id, s.Code, s.Name, String.IsNullOrWhiteSpace(s.ShortName) ? null : s.ShortName)),
            FilterAIReferenceKind.MediaType => _mediaTypeService.FindAll()
                .Where(m => (!enabledOnly || m.IsEnabled) && !_excludedMediaTypeIds.Contains(m.Id))
                .Select(m => new FilterAIReference(m.Id, null, m.Name)),
            FilterAIReferenceKind.Series => _seriesService.FindAll()
                .Where(s => !enabledOnly || s.IsEnabled)
                .Select(s => new FilterAIReference(s.Id, null, s.Name)),
            FilterAIReferenceKind.Contributor => _contributorService.FindAll()
                .Where(c => !enabledOnly || c.IsEnabled)
                .Select(c => new FilterAIReference(c.Id, null, c.Name, String.IsNullOrWhiteSpace(c.Aliases) ? null : c.Aliases)),
            FilterAIReferenceKind.Tag => _tagService.FindAll()
                .Where(t => !enabledOnly || t.IsEnabled)
                .Select(t => new FilterAIReference(null, t.Code, t.Name)),
            FilterAIReferenceKind.Action => _actionService.FindAll()
                .Where(a => !enabledOnly || a.IsEnabled)
                .Select(a => new FilterAIReference(a.Id, null, a.Name, $"valueType {a.ValueType}")),
            _ => Array.Empty<FilterAIReference>(),
        };
    }

    /// <summary>
    /// A copy of the query that the excluded sources and media types never match. It is always a
    /// copy, so it can be placed in another body.
    /// </summary>
    private JsonNode Restrict(JsonNode? query)
    {
        if (_excludedSourceIds.Length == 0 && _excludedMediaTypeIds.Length == 0) return query?.DeepClone() ?? new JsonObject() { ["match_all"] = new JsonObject() };

        var mustNot = new JsonArray();
        if (_excludedSourceIds.Length > 0)
            mustNot.Add(new JsonObject() { ["terms"] = new JsonObject() { ["sourceId"] = new JsonArray(_excludedSourceIds.Select(id => (JsonNode?)id).ToArray()) } });
        if (_excludedMediaTypeIds.Length > 0)
            mustNot.Add(new JsonObject() { ["terms"] = new JsonObject() { ["mediaTypeId"] = new JsonArray(_excludedMediaTypeIds.Select(id => (JsonNode?)id).ToArray()) } });
        return new JsonObject()
        {
            ["bool"] = new JsonObject()
            {
                ["must"] = new JsonArray(query?.DeepClone() ?? new JsonObject() { ["match_all"] = new JsonObject() }),
                ["must_not"] = mustNot,
            },
        };
    }

    /// <summary>
    /// The reason in an Elasticsearch error response, when there is one.
    /// </summary>
    private static string? ElasticError(string? body)
    {
        if (String.IsNullOrWhiteSpace(body)) return null;
        try
        {
            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("error", out var error)) return null;
            if (error.ValueKind == JsonValueKind.String) return error.GetString();
            if (error.TryGetProperty("root_cause", out var causes) && causes.ValueKind == JsonValueKind.Array && causes.GetArrayLength() > 0
                && causes[0].TryGetProperty("reason", out var rootReason))
                return rootReason.GetString();
            return error.TryGetProperty("reason", out var reason) ? reason.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
    #endregion
}
