using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Charts;
using TNO.API.Models.Settings;
using TNO.DAL.Models;
using TNO.Elastic;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ReportChartDataService class, provides chart data from Elasticsearch aggregations.
/// The output matches what the 'Custom' chart template produces from the same stories.
/// </summary>
public class ReportChartDataService : BaseService, IReportChartDataService
{
    #region Variables
    /// <summary>The most ids in a single terms query, below index.max_terms_count.</summary>
    public const int MaxIdsPerQuery = 10000;

    private static readonly string[] QueryPropertiesToRemove = ["sort", "from", "_source", "highlight", "collapse", "aggs", "aggregations", "search_after", "size"];

    private readonly ITNOElasticClient _elasticClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly IReportService _reportService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISeriesService _seriesService;
    private readonly IConfiguration? _configuration;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    /// <inheritdoc />
    public bool ByLineAggregationEnabled => _configuration?.GetValue<bool>(ReportChartSupport.ByLineAggregationKey) ?? false;
    #endregion

    #region Constructors
    public ReportChartDataService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IReportService reportService,
        IMediaTypeService mediaTypeService,
        ISeriesService seriesService,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ReportChartDataService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _reportService = reportService;
        _mediaTypeService = mediaTypeService;
        _seriesService = seriesService;
        _configuration = serviceProvider.GetService(typeof(IConfiguration)) as IConfiguration;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <inheritdoc />
    public async Task<ReportChartDataResult> GetChartDataAsync(ReportChartDataRequest request)
    {
        var settings = request.SettingsOverride ?? JsonSerializer.Deserialize<ChartSectionSettingsModel>(request.Chart.Settings.RootElement.GetRawText(), _serializerOptions) ?? new();
        if (!ReportChartSupport.SupportsDimensions(settings, this.ByLineAggregationEnabled))
            throw new InvalidOperationException("byline is not available in aggregate mode");

        var isSentiment = settings.DatasetValue == "sentiment";
        var hasDataset = ChartAggregationQuery.IsAggregated(settings.Dataset);
        var hasGroupBy = ChartAggregationQuery.IsAggregated(settings.GroupBy);
        var levels = new List<(string Dimension, string Order)>();
        if (hasDataset) levels.Add((settings.Dataset, settings.DatasetOrder));
        if (hasGroupBy) levels.Add((settings.GroupBy, settings.GroupByOrder));
        var aggs = ChartAggregationQuery.Build(levels, isSentiment, this.ByLineAggregationEnabled);

        var mediaTypes = levels.Any(l => l.Dimension == "mediaType") ? _mediaTypeService.FindAll().ToDictionary(m => m.Id.ToString(CultureInfo.InvariantCulture), m => m.Name) : [];
        var series = levels.Any(l => l.Dimension == "series") ? _seriesService.FindAll().ToDictionary(s => s.Id.ToString(CultureInfo.InvariantCulture), s => s.Name) : [];
        string ResolveKey(string dimension, string key) => dimension switch
        {
            "mediaType" => mediaTypes.GetValueOrDefault(key) ?? "Other",
            "series" => series.GetValueOrDefault(key) ?? "None",
            "otherSeries" => String.IsNullOrWhiteSpace(key) ? "None" : key,
            "byline" => CleanByline(key),
            _ => key,
        };

        var reader = new ChartAggregationReader(levels.Select(l => l.Dimension).ToArray(), ResolveKey);
        var sections = new List<string>();
        foreach (var (sectionName, index, query) in await BuildSourceQueriesAsync(request, aggs))
        {
            var response = await RetryElasticsearchQueryAsync(() => _elasticClient.SearchRawAsync(index, query));
            if (!sections.Contains(sectionName)) sections.Add(sectionName);
            reader.Read(response.RootElement, keys => Place(settings, keys, sectionName, hasDataset, hasGroupBy));
        }

        var result = new ReportChartDataResult
        {
            Data = Convert(settings, reader, sections, request.SectionLabels),
            DocCount = reader.DocCount,
            Truncated = reader.Truncated,
        };
        if (result.Truncated)
            result.Warnings.Add($"Chart data was truncated at {ChartAggregationQuery.TermsSize} buckets. Report:{request.Report.Id}, Section:{request.Section.Name}, Chart:{request.Chart.ChartTemplateId}");
        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, Elastic.Models.AggregationRootModel>> AggregateAsync(ReportChartDataRequest request, JsonDocument aggs)
    {
        var aggsNode = JsonNode.Parse(aggs.RootElement.GetRawText())?.AsObject() ?? new JsonObject();
        var totals = new Dictionary<string, Elastic.Models.AggregationRootModel>();
        foreach (var (sectionName, index, query) in await BuildSourceQueriesAsync(request, aggsNode))
        {
            var response = await RetryElasticsearchQueryAsync(() => _elasticClient.SearchAsync<object>(index, query));
            foreach (var aggregation in response.Aggregations ?? [])
            {
                var key = aggsNode.Count > 1 ? $"{sectionName}:{aggregation.Key}" : sectionName;
                totals[key] = totals.TryGetValue(key, out var existing) ? MergeBuckets(existing, aggregation.Value) : aggregation.Value;
            }
        }
        return totals;
    }

    /// <inheritdoc />
    public ReportChartDataRequest BuildRequest(
        Report report,
        ReportSection section,
        ReportSectionChartTemplate chart,
        ReportChartSource source,
        long? instanceId,
        IDictionary<string, long[]>? contentBySection,
        int? requestorId)
    {
        var request = new ReportChartDataRequest
        {
            Report = report,
            Section = section,
            Chart = chart,
            Source = source,
            InstanceId = instanceId,
            RequestorId = requestorId,
        };
        switch (source)
        {
            case ReportChartSource.InstanceIds:
                var content = contentBySection ?? (instanceId.HasValue ? GetInstanceContent(instanceId.Value) : new Dictionary<string, long[]>());
                FillSections(request, report.Sections, content);
                break;
            case ReportChartSource.LinkedInstanceIds:
                var linkedId = section.LinkedReportId ?? throw new InvalidOperationException($"Section '{section.Name}' has no linked report.");
                var linkedInstance = _reportService.GetCurrentReportInstance(linkedId, report.OwnerId);
                if (linkedInstance != null)
                {
                    var linkedSections = this.Context.ReportSections.AsNoTracking().Where(s => s.ReportId == linkedId).ToArray();
                    FillSections(request, linkedSections, GetInstanceContent(linkedInstance.Id));
                }
                break;
            case ReportChartSource.Filter:
            case ReportChartSource.Folder:
                request.SectionLabels[section.Name] = GetSectionLabel(section);
                break;
        }
        return request;
    }

    /// <inheritdoc />
    public DateTime? GetStoriesChangedOn(Report report, ReportSection section, long instanceId)
    {
        // The same instance BuildRequest reads the stories from.
        var instances = this.Context.ReportInstances.AsNoTracking();
        switch (ReportChartSupport.GetSource(section, _serializerOptions))
        {
            case ReportChartSource.InstanceIds:
                instances = instances.Where(i => i.Id == instanceId);
                break;
            case ReportChartSource.LinkedInstanceIds:
                var linkedId = section.LinkedReportId!.Value;
                instances = instances.Where(i => i.ReportId == linkedId);
                if (report.OwnerId.HasValue) instances = instances.Where(i => i.OwnerId == report.OwnerId);
                break;
            default:
                // A filter or folder is run when the report or the section is generated, not when it is viewed.
                return null;
        }
        var instance = instances.OrderByDescending(i => i.Id).Select(i => new { i.Id, i.UpdatedOn }).FirstOrDefault();
        if (instance == null) return null;

        // Adding a story to a report only inserts its link and leaves the instance as it was. Removing
        // one saves the instance. Between them, the newer of the two covers every change.
        var contentChangedOn = this.Context.ReportInstanceContents.AsNoTracking()
            .Where(c => c.InstanceId == instance.Id)
            .Max(c => (DateTime?)c.UpdatedOn);
        return contentChangedOn > instance.UpdatedOn ? contentChangedOn : instance.UpdatedOn;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ReportInstanceChart>> ComputeChartsAsync(
        Report report,
        long? instanceId,
        IDictionary<string, long[]>? contentBySection,
        int? requestorId,
        int? sectionId = null,
        Func<ReportSection, ReportSectionChartTemplate, Dictionary<string, Elastic.Models.AggregationRootModel>, Task<JsonDocument>>? renderBuckets = null)
    {
        var rows = new List<ReportInstanceChart>();
        var byLine = this.ByLineAggregationEnabled;
        var sections = report.Sections
            .Where(s => s.SectionType == ReportSectionType.MediaAnalytics && s.IsEnabled && (sectionId == null || s.Id == sectionId))
            .OrderBy(s => s.SortOrder);
        foreach (var section in sections)
        {
            if (ReportChartSupport.GetSource(section, _serializerOptions) is not ReportChartSource source) continue;
            // Without an instance or its stories, a chart over listed sections has nothing to count.
            if (source == ReportChartSource.InstanceIds && !instanceId.HasValue && contentBySection == null) continue;

            foreach (var chart in section.ChartTemplatesManyToMany)
            {
                if (!ReportChartSupport.SupportsAggregation(chart, _serializerOptions, byLine)) continue;
                var template = ReportChartSupport.GetTemplateSettings(chart, _serializerOptions)!;
                // Stamped before the stories are read, so a story added while the chart is computed is newer than the chart.
                var generatedOn = DateTime.UtcNow;
                var request = BuildRequest(report, section, chart, source, instanceId, contentBySection, requestorId);

                JsonDocument data;
                long docCount = 0;
                var truncated = false;
                if (template.Aggregate)
                {
                    var result = await GetChartDataAsync(request);
                    data = JsonSerializer.SerializeToDocument(result.Data);
                    docCount = result.DocCount;
                    truncated = result.Truncated;
                    result.Warnings.ForEach(warning => this.Logger.LogWarning("{warning}", warning));
                }
                else
                {
                    if (renderBuckets == null) continue;
                    var aggregations = await AggregateAsync(request, template.Aggregations!);
                    data = await renderBuckets(section, chart, aggregations);
                    docCount = aggregations.Values.Sum(a => a.DocCount);
                }
                rows.Add(new ReportInstanceChart(instanceId ?? 0, section.Id, chart.ChartTemplateId, section.Name, data)
                {
                    DocCount = docCount,
                    IsTruncated = truncated,
                    GeneratedOn = generatedOn,
                });
            }
        }
        return rows;
    }

    /// <summary>
    /// The stories of each section in the instance.
    /// </summary>
    private Dictionary<string, long[]> GetInstanceContent(long instanceId)
    {
        return this.Context.ReportInstanceContents
            .AsNoTracking()
            .Where(c => c.InstanceId == instanceId)
            .Select(c => new { c.SectionName, c.ContentId })
            .AsEnumerable()
            .GroupBy(c => c.SectionName)
            .ToDictionary(g => g.Key, g => g.Select(c => c.ContentId).ToArray());
    }

    private void FillSections(ReportChartDataRequest request, IEnumerable<ReportSection> sections, IDictionary<string, long[]> content)
    {
        foreach (var section in sections.Where(s => s.SectionType == ReportSectionType.Content && s.IsEnabled).OrderBy(s => s.SortOrder))
        {
            request.SectionLabels[section.Name] = GetSectionLabel(section);
            if (content.TryGetValue(section.Name, out var ids) && ids.Length > 0) request.SectionIds[section.Name] = ids;
        }
    }

    private string? GetSectionLabel(ReportSection section)
    {
        var settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(section.Settings.RootElement.GetRawText(), _serializerOptions);
        return String.IsNullOrEmpty(settings?.Label) ? null : settings.Label;
    }

    /// <summary>
    /// Build the query of every source section. A section with more ids than a single query takes is chunked.
    /// </summary>
    private async Task<List<(string SectionName, string Index, JsonDocument Query)>> BuildSourceQueriesAsync(ReportChartDataRequest request, JsonObject aggs)
    {
        var queries = new List<(string, string, JsonDocument)>();
        switch (request.Source)
        {
            case ReportChartSource.InstanceIds:
            case ReportChartSource.LinkedInstanceIds:
                foreach (var (sectionName, ids) in request.SectionIds)
                    AddIdQueries(queries, sectionName, ids, aggs);
                break;
            case ReportChartSource.Folder:
                var folderId = request.Section.FolderId ?? throw new InvalidOperationException($"Section '{request.Section.Name}' has no folder.");
                var folderIds = await this.Context.FolderContents.AsNoTracking()
                    .Where(fc => fc.FolderId == folderId)
                    .OrderBy(fc => fc.SortOrder)
                    .Select(fc => fc.ContentId)
                    .ToArrayAsync();
                var context = _reportService.BuildSectionQueryContext(request.Report, request.InstanceId, request.RequestorId);
                var settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(request.Section.Settings.RootElement.GetRawText(), _serializerOptions) ?? new();
                var exclude = settings.OverrideExcludeHistorical ? context.ExcludeReportContentIds : context.ExcludeHistoricalContentIds.Concat(context.ExcludeReportContentIds);
                AddIdQueries(queries, request.Section.Name, folderIds.Except(exclude).ToArray(), aggs);
                break;
            case ReportChartSource.Filter:
                var queryContext = _reportService.BuildSectionQueryContext(request.Report, request.InstanceId, request.RequestorId);
                var (index, query) = _reportService.BuildSectionQuery(request.Report, request.Section, queryContext);
                this.Logger.LogDebug("Chart query excludes {count} stories. Report:{reportId}, Section:{section}", queryContext.ExcludeHistoricalContentIds.Length + queryContext.ExcludeReportContentIds.Length, request.Report.Id, request.Section.Name);
                queries.Add((request.Section.Name, index, ToAggregationQuery(JsonNode.Parse(query.RootElement.GetRawText())!.AsObject(), aggs)));
                break;
            default:
                throw new NotImplementedException($"Chart source '{request.Source}' has not been implemented");
        }
        return queries;
    }

    private void AddIdQueries(List<(string, string, JsonDocument)> queries, string sectionName, long[] ids, JsonObject aggs)
    {
        // The unpublished index holds every story, so an unpublished story listed in a report still counts.
        foreach (var chunk in ids.Chunk(MaxIdsPerQuery))
        {
            var query = new JsonObject
            {
                ["query"] = new JsonObject { ["terms"] = new JsonObject { ["id"] = new JsonArray(chunk.Select(id => (JsonNode)JsonValue.Create(id)).ToArray()) } },
            };
            queries.Add((sectionName, _elasticOptions.ContentIndex, ToAggregationQuery(query, aggs)));
        }
    }

    /// <summary>
    /// Stop the query returning hits, however many it matches, and ask for the aggregations instead.
    /// </summary>
    private static JsonDocument ToAggregationQuery(JsonObject query, JsonObject aggs)
    {
        foreach (var property in QueryPropertiesToRemove) query.Remove(property);
        query["size"] = 0;
        query["track_total_hits"] = true;
        if (aggs.Count > 0) query["aggs"] = aggs.DeepClone();
        return JsonDocument.Parse(query.ToJsonString());
    }

    private static (string Dataset, string Label) Place(ChartSectionSettingsModel settings, string[] keys, string sectionName, bool hasDataset, bool hasGroupBy)
    {
        var dataset = hasDataset ? keys[0] : settings.Dataset == "reportSection" ? sectionName : "";
        var label = hasGroupBy ? keys[hasDataset ? 1 : 0] : settings.GroupBy == "reportSection" ? sectionName : "";
        return (dataset, label);
    }

    private static string CleanByline(string key)
    {
        var value = key.Replace("<mark>", "").Replace("</mark>", "");
        return String.IsNullOrWhiteSpace(value) ? "Unknown" : value;
    }

    /// <summary>
    /// Convert the cells into Chart.js datasets.
    /// </summary>
    public static ChartDataModel Convert(ChartSectionSettingsModel settings, ChartAggregationReader reader, IReadOnlyList<string> sections, IDictionary<string, string?> sectionLabels)
    {
        var isSentiment = settings.DatasetValue == "sentiment";
        var cells = reader.Cells;

        var datasetKeys = OrderKeys(cells.Keys.Select(k => k.Dataset).Distinct(), settings.Dataset, settings.DatasetOrder, sections);
        var labelKeys = OrderKeys(cells.Keys.Select(k => k.Label).Distinct(), settings.GroupBy, settings.GroupByOrder, sections);

        if (settings.ExcludeEmptyValues)
        {
            datasetKeys = datasetKeys.Where(k => !IsEmptyKey(settings.Dataset, k)
                && (!isSentiment || cells.Where(c => c.Key.Dataset == k).Sum(c => c.Value.SentimentCount) > 0)).ToArray();
            labelKeys = labelKeys.Where(k => !IsEmptyKey(settings.GroupBy, k)
                && (!isSentiment || cells.Where(c => c.Key.Label == k).Sum(c => c.Value.SentimentCount) > 0)).ToArray();
        }

        var datasets = datasetKeys.Select((datasetKey, index) =>
        {
            var colorIndex = settings.ApplyColorToValue ? -1 : index;
            return new ChartDatasetModel
            {
                Label = GetLabel(settings.Dataset, datasetKey, isSentiment, sectionLabels),
                Data = labelKeys.Select(labelKey =>
                {
                    cells.TryGetValue((datasetKey, labelKey), out var cell);
                    if (!isSentiment) return (double?)(cell?.Count ?? 0);
                    // Through an int, because rounding a small negative gives -0, which a chart would label.
                    return cell?.SentimentCount > 0 ? (int)Math.Round(cell.SentimentSum / cell.SentimentCount, MidpointRounding.AwayFromZero) : null;
                }).ToArray(),
                BackgroundColor = ParseColors(settings.DatasetColors, colorIndex, settings, datasetKey),
                BorderColor = ParseColors(settings.DatasetBorderColors, colorIndex, settings, datasetKey),
                SpanGaps = true,
                BorderWidth = 2,
                MinBarLength = settings.MinBarLength,
            };
        }).ToArray();

        return new ChartDataModel
        {
            Labels = labelKeys.Select(k => GetLabel(settings.GroupBy, k, isSentiment, sectionLabels)).ToArray(),
            Datasets = datasets,
        };
    }

    private static string[] OrderKeys(IEnumerable<string> keys, string dimension, string order, IReadOnlyList<string> sections)
    {
        if (dimension == "reportSection") return keys.OrderBy(k => sections.ToList().IndexOf(k)).ToArray();
        return order == "desc"
            ? keys.OrderByDescending(k => k, StringComparer.InvariantCulture).ToArray()
            : keys.OrderBy(k => k, StringComparer.InvariantCulture).ToArray();
    }

    private static bool IsEmptyKey(string dimension, string key) => dimension switch
    {
        "byline" => key == "Unknown",
        "series" or "sentiment" or "sentimentSimple" => key == "None",
        _ => false,
    };

    private static string GetLabel(string dimension, string key, bool isSentiment, IDictionary<string, string?> sectionLabels)
    {
        return dimension switch
        {
            "" => isSentiment ? "Average Sentiment" : "Story Count",
            "reportSection" => sectionLabels.TryGetValue(key, out var label) && label != null ? label : "Other",
            "dayMonthYear" => DateTime.ParseExact(key, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
            "monthDay" => DateTime.ParseExact(key, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("MMM-dd", CultureInfo.InvariantCulture),
            "monthYear" => DateTime.ParseExact(key, "yyyyMM", CultureInfo.InvariantCulture).ToString("MMM-yyyy", CultureInfo.InvariantCulture),
            _ => key,
        };
    }

    private static JsonElement? ParseColors(string[]? colors, int index, ChartSectionSettingsModel settings, string datasetKey)
    {
        var fragment = ChartColors.GetColors(colors, index, settings.Dataset, datasetKey, settings.DatasetValue);
        if (String.IsNullOrEmpty(fragment)) return null;
        using var document = JsonDocument.Parse(fragment);
        return document.RootElement.Clone();
    }

    private static Elastic.Models.AggregationRootModel MergeBuckets(Elastic.Models.AggregationRootModel left, Elastic.Models.AggregationRootModel right)
    {
        left.DocCount += right.DocCount;
        var merged = left.ChildAggregation?.Buckets.ToDictionary(b => b.Key) ?? new();
        foreach (var bucket in right.ChildAggregation?.Buckets ?? [])
        {
            if (merged.TryGetValue(bucket.Key, out var existing))
            {
                existing.DocCount += bucket.DocCount;
                if (existing.AggregationSum != null && bucket.AggregationSum != null) existing.AggregationSum.Value += bucket.AggregationSum.Value;
            }
            else merged[bucket.Key] = bucket;
        }
        if (left.ChildAggregation != null) left.ChildAggregation.Buckets = merged.Values.ToArray();
        return left;
    }

    private async Task<T> RetryElasticsearchQueryAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            this.Logger.LogWarning(ex, "Elasticsearch chart query failed. Retrying in 200ms. Error: {ErrorMessage}", ex.Message);
            await Task.Delay(200);
            return await operation();
        }
    }
    #endregion
}
