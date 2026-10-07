using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Elastic;

namespace TNO.DAL.Services;

/// <summary>
/// ReportEvidenceService class, reads report synthesis evidence from the evidence index. Complete
/// sets are read with a point in time and 'search_after', so a large report is never cut off at a
/// page or result-window limit. Only approved evidence is returned.
/// </summary>
public class ReportEvidenceService : IReportEvidenceService
{
    #region Variables
    private const int PageSize = 500;
    private const int TermsPerQuery = 1000;
    private const string KeepAlive = "1m";
    private readonly ITNOElasticClient _client;
    private readonly ElasticOptions _options;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<ReportEvidenceService> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEvidenceService object, initializes with specified parameters.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ReportEvidenceService(ITNOElasticClient client, IOptions<ElasticOptions> options, IOptions<JsonSerializerOptions> serializerOptions, ILogger<ReportEvidenceService> logger)
    {
        _client = client;
        _options = options.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find the approved evidence for the specified content.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<long, ContentEvidenceModel>();
        var ids = contentIds.Distinct().ToArray();
        if (ids.Length == 0 || String.IsNullOrWhiteSpace(_options.EvidenceIndex)) return result;

        var open = await _client.LowLevel.OpenPointInTimeAsync<StringResponse>(_options.EvidenceIndex, new OpenPointInTimeRequestParameters() { KeepAlive = KeepAlive }, cancellationToken);
        if (!open.Success)
        {
            // An environment without the evidence index synthesizes from story text.
            _logger.LogWarning("Failed to open the evidence index '{index}': {error}", _options.EvidenceIndex, open.Body);
            return result;
        }
        var pitId = JsonDocument.Parse(open.Body).RootElement.GetProperty("id").GetString()!;
        try
        {
            foreach (var chunk in ids.Chunk(TermsPerQuery))
            {
                object[]? searchAfter = null;
                while (true)
                {
                    var body = new Dictionary<string, object?>
                    {
                        ["size"] = PageSize,
                        ["pit"] = new { id = pitId, keep_alive = KeepAlive },
                        ["query"] = new
                        {
                            @bool = new
                            {
                                filter = new object[]
                                {
                                    new { terms = new { contentId = chunk } },
                                    new { term = new { isApproved = true } },
                                },
                            },
                        },
                        ["sort"] = new object[] { new { contentId = "asc" } },
                    };
                    if (searchAfter != null) body["search_after"] = searchAfter;

                    var response = await _client.LowLevel.SearchAsync<StringResponse>(PostData.String(JsonSerializer.Serialize(body)), ctx: cancellationToken);
                    if (!response.Success) throw new InvalidOperationException($"Failed to read evidence: {response.Body}");

                    using var document = JsonDocument.Parse(response.Body);
                    var root = document.RootElement;
                    if (root.TryGetProperty("pit_id", out var nextPit)) pitId = nextPit.GetString() ?? pitId;
                    var hits = root.GetProperty("hits").GetProperty("hits");
                    if (hits.GetArrayLength() == 0) break;
                    foreach (var hit in hits.EnumerateArray())
                    {
                        var evidence = hit.GetProperty("_source").Deserialize<ContentEvidenceModel>(_serializerOptions);
                        if (evidence != null) result[evidence.ContentId] = evidence;
                        searchAfter = hit.GetProperty("sort").EnumerateArray().Select(v => (object)v.Clone()).ToArray();
                    }
                    if (hits.GetArrayLength() < PageSize) break;
                }
            }
        }
        finally
        {
            var close = await _client.LowLevel.ClosePointInTimeAsync<StringResponse>(PostData.String(JsonSerializer.Serialize(new { id = pitId })), ctx: CancellationToken.None);
            if (!close.Success) _logger.LogDebug("Failed to close point in time: {error}", close.Body);
        }
        return result;
    }
    #endregion
}
