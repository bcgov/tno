
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Elastic.Models;
using TNO.TemplateEngine;
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
    /// Makes a request to Elasticsearch if required to fetch content.
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// If the model includes a Filter it will make a request to Elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ChartResultModel> GenerateJsonAsync(
        ChartRequestModel model,
        string? index = null,
        JsonDocument? filter = null,
        bool updateCache = false)
    {
        var chart = model.ChartTemplate;
        SearchResultModel<Areas.Services.Models.Content.ContentModel>? searchResults = null;
        var content = new List<ContentModel>(chart.Content ?? Array.Empty<ContentModel>());
        if (filter != null)
        {
            searchResults = await _contentService.FindWithElasticsearchAsync(index ?? _elasticOptions.PublishedIndex, filter);
            content.AddRange(searchResults.Hits.Hits.Select(h => new ContentModel(h.Source)).ToArray());
        }
        chart.Content = content.ToArray();

        var result = await _reportEngine.GenerateJsonAsync(model, updateCache);
        result.Results = searchResults != null ? JsonDocument.Parse(JsonSerializer.Serialize(searchResults)) : null;
        return result;
    }

    /// <summary>
    /// Execute the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <param name="updateCache"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64ImageAsync(
        ChartRequestModel model,
        string? index = null,
        JsonDocument? filter = null,
        bool updateCache = false)
    {
        // Get the Chart JSON data.
        model.ChartData ??= (await this.GenerateJsonAsync(model, index, filter, updateCache)).Json;
        return await _reportEngine.GenerateBase64ImageAsync(model);
    }

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Uses the content already in the report instance.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        bool updateCache = false)
    {
        var reportModel = model.Report ?? throw new ArgumentException("Parameter 'model.Report' is required");
        if (model.Report.Template == null) throw new ArgumentException("Parameter 'model.Report.Template' is required");

        // Link each result with the section name.
        var sections = reportModel.Sections.ToDictionary(section => section.Name, section =>
        {
            var content = model.Content
                    .Where(c => c.SectionName == section.Name)
                    .Select(c => new ContentModel(c.Content ?? throw new InvalidOperationException("Report instance model is missing content")))
                    .ToArray();

            return new ReportSectionModel(section, content);
        });

        return await GenerateReportAsync(reportModel, sections, updateCache);
    }

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Fetch content from elasticsearch and folders.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.Report.ReportModel model,
        bool updateCache = false)
    {
        if (model.Template == null) throw new ArgumentException("Parameter 'model.Template' is required");

        // Fetch all content for this report.
        var index = _elasticOptions.PublishedIndex;
        var elasticResults = await _reportService.FindContentWithElasticsearchAsync(index, model.ToEntity(_serializerOptions));

        // Link each result with the section name.
        var sections = model.Sections.ToDictionary(section => section.Name, section =>
        {
            elasticResults.TryGetValue(section.Name, out SearchResultModel<Areas.Services.Models.Content.ContentModel>? sectionResult);
            var content = sectionResult?.Hits.Hits.Select(hit => new ContentModel(hit.Source)).ToArray() ?? Array.Empty<ContentModel>();
            return new ReportSectionModel(section, content);
        });

        var result = await GenerateReportAsync(model, sections, updateCache);
        result.Data = elasticResults;
        return result;

        throw new NotImplementedException($"Report template type '{model.Template.ReportType.GetName()}' has not been implemented");
    }

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sections"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.Report.ReportModel report,
        Dictionary<string, ReportSectionModel> sections,
        bool updateCache = false)
    {
        var subject = await _reportEngine.GenerateReportSubjectAsync(report, sections, updateCache);
        var body = await _reportEngine.GenerateReportBodyAsync(report, sections, _storageOptions.GetUploadPath(), updateCache);

        return new ReportResultModel(subject, body);
    }

    #region AV Overview
    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// Fetch content from elasticsearch and folders.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<ReportResultModel> GenerateReportAsync(
        AVOverviewInstanceModel instance,
        bool updateCache = false)
    {
        var template = _overviewTemplateService.FindById(instance.TemplateType) ?? throw new InvalidOperationException($"AV overview template '{instance.TemplateType.GetName()}' not found.");
        if (template.Template == null) throw new InvalidOperationException($"Report template '{instance.TemplateType.GetName()}' not found.");
        var result = await GenerateReportAsync(new Areas.Services.Models.AVOverview.ReportTemplateModel(template.Template, _serializerOptions), instance, updateCache);
        result.Data = instance;
        return result;
    }

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="instance"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel instance,
        bool updateCache = false)
    {
        var subject = await _reportEngine.GenerateReportSubjectAsync(reportTemplate, instance, updateCache);
        var body = await _reportEngine.GenerateReportBodyAsync(reportTemplate, instance, updateCache);

        return new ReportResultModel(subject, body);
    }
    #endregion
    #endregion
}
