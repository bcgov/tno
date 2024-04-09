
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Elastic.Models;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Converters;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Charts;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.API.Helpers;

/// <summary>
/// ReportHelper class, provides helper methods to generate reports and charts.
/// </summary>
/// TODO: Much of this is duplicated in the ReportingManager and needs to be centralized, however there are complicated dependencies that need to be figured out.
public class ReportHelper : IReportHelper
{
    #region Variables
    private readonly IReportEngine _reportEngine;
    private readonly IReportService _reportService;
    private readonly IAVOverviewTemplateService _overviewTemplateService;
    private readonly IContentService _contentService;
    private readonly ElasticOptions _elasticOptions;
    private readonly StorageOptions _storageOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportEngine"></param>
    /// <param name="reportService"></param>
    /// <param name="overviewTemplateService"></param>
    /// <param name="contentService"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportHelper(
        IReportEngine reportEngine,
        IReportService reportService,
        IAVOverviewTemplateService overviewTemplateService,
        IContentService contentService,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportEngine = reportEngine;
        _reportService = reportService;
        _overviewTemplateService = overviewTemplateService;
        _contentService = contentService;
        _elasticOptions = elasticOptions.Value;
        _storageOptions = storageOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// If a filter is provided make a request to Elasticsearch for content.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ContentModel>> FindContentAsync(JsonDocument filter, string? index)
    {
        var content = new List<ContentModel>();
        var searchResults = await _contentService.FindWithElasticsearchAsync(index ?? _elasticOptions.PublishedIndex, filter);
        content.AddRange(searchResults.Hits.Hits.Select(h => new ContentModel(h.Source)).ToArray());
        return content;
    }

    /// <summary>
    /// Get the content from the current instance of the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    public Task<Dictionary<string, ReportSectionModel>> GetLinkedReportContent(int reportId, int? ownerId = null)
    {
        var report = _reportService.FindById(reportId) ?? throw new NoContentException($"Report does not exist: ${reportId}");
        var instance = _reportService.GetCurrentReportInstance(reportId, ownerId, true);
        var sections = report.Sections.ToDictionary(s => s.Name, s =>
        {
            var content = instance?.ContentManyToMany.Where(c => c.Content != null && c.SectionName == s.Name) ?? Array.Empty<Entities.ReportInstanceContent>();
            var section = new ReportSectionModel(s, content, _serializerOptions);
            return section;
        });
        return Task.FromResult(sections);
    }

    /// <summary>
    /// Makes a request to Elasticsearch if required to fetch content.
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// If the model includes a Filter it will make a request to Elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ChartResultModel> GenerateJsonAsync(
        ChartRequestModel model,
        bool isPreview = false)
    {
        var result = await _reportEngine.GenerateJsonAsync(model, isPreview);
        result.Results = JsonDocument.Parse(JsonSerializer.Serialize(model.ChartTemplate.Content));
        return result;
    }

    /// <summary>
    /// Execute the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64ImageAsync(
        ChartRequestModel model,
        bool isPreview = false)
    {
        // Get the Chart JSON data.
        model.ChartData ??= (await this.GenerateJsonAsync(model, isPreview)).Json;
        return await _reportEngine.GenerateBase64ImageAsync(model);
    }

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Uses the content already in the report instance.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        bool viewOnWebOnly = false,
        bool isPreview = false)
    {
        var reportModel = model.Report ?? throw new ArgumentException("Parameter 'model.Report' is required");
        if (model.Report.Template == null) throw new ArgumentException("Parameter 'model.Report.Template' is required");

        // Link each result with the section name.
        var sections = reportModel.Sections.OrderBy(s => s.SortOrder).ToDictionary(section => section.Name, section =>
        {
            var content = model.Content
                    .Where(c => c.SectionName == section.Name)
                    .OrderBy(c => c.SortOrder)
                    .Select(c => new ContentModel(c.Content ?? throw new InvalidOperationException("Report instance model is missing content")))
                    .ToArray();

            return new ReportSectionModel(section, content);
        });

        return await GenerateReportAsync(reportModel, model, sections, viewOnWebOnly, isPreview);
    }

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Fetch content from elasticsearch and folders.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.Report.ReportModel model,
        int? requestorId = null,
        bool viewOnWebOnly = false,
        bool isPreview = false)
    {
        if (model.Template == null) throw new ArgumentException("Parameter 'model.Template' is required");

        // Fetch all content for this report.
        var elasticResults = await _reportService.FindContentWithElasticsearchAsync(model.ToEntity(_serializerOptions), null, requestorId);

        // Link each result with the section name.
        var sections = model.Sections.OrderBy(s => s.SortOrder).ToDictionary(section => section.Name, section =>
        {
            var sortOrder = 0;
            elasticResults.TryGetValue(section.Name, out SearchResultModel<Areas.Services.Models.Content.ContentModel>? sectionResult);
            var content = sectionResult?.Hits.Hits.Select(hit => new ContentModel(hit.Source, sortOrder++)).ToArray() ?? Array.Empty<ContentModel>();
            Dictionary<string, TNO.TemplateEngine.Models.Reports.AggregationRootModel>? aggregations = null;
            if (sectionResult?.Aggregations != null)
            {
                aggregations = new Dictionary<string, TNO.TemplateEngine.Models.Reports.AggregationRootModel>();
                foreach (var aggregation in sectionResult.Aggregations)
                {
                    aggregations.Add(aggregation.Key, aggregation.Value.Convert());
                }
            }
            return new ReportSectionModel(section, content, aggregations);
        });

        var result = await GenerateReportAsync(model, null, sections, viewOnWebOnly, isPreview);
        result.Data = elasticResults;
        return result;

        throw new NotImplementedException($"Report template type '{model.Template.ReportType.GetName()}' has not been implemented");
    }

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstance"></param>
    /// <param name="sections"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.Report.ReportModel report,
        Areas.Services.Models.ReportInstance.ReportInstanceModel? reportInstance,
        Dictionary<string, ReportSectionModel> sections,
        bool viewOnWebOnly = false,
        bool isPreview = false)
    {
        var subject = await _reportEngine.GenerateReportSubjectAsync(report, reportInstance, sections, viewOnWebOnly, isPreview);
        var body = await _reportEngine.GenerateReportBodyAsync(report, reportInstance, sections, GetLinkedReportContent, _storageOptions.GetUploadPath(), viewOnWebOnly, isPreview);

        return new ReportResultModel(subject, body);
    }

    #region AV Overview
    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Fetch content from elasticsearch and folders.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        AVOverviewInstanceModel instance,
        bool isPreview = false)
    {
        var template = _overviewTemplateService.FindById(instance.TemplateType) ?? throw new InvalidOperationException($"AV overview template '{instance.TemplateType.GetName()}' not found.");
        if (template.Template == null) throw new InvalidOperationException($"Report template '{instance.TemplateType.GetName()}' not found.");
        var result = await GenerateReportAsync(new Areas.Services.Models.AVOverview.ReportTemplateModel(template.Template, _serializerOptions), instance, isPreview);
        result.Data = instance;
        return result;
    }

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="instance"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel instance,
        bool isPreview = false)
    {
        var subject = await _reportEngine.GenerateReportSubjectAsync(reportTemplate, instance, isPreview);
        var body = await _reportEngine.GenerateReportBodyAsync(reportTemplate, instance, isPreview);

        return new ReportResultModel(subject, body);
    }
    #endregion
    #endregion
}
