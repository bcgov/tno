
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.DAL.Config;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.Elastic.Models;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Config;
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
    private readonly ITemplateEngine<ReportTemplateModel> _reportTemplateEngine;
    private readonly ITemplateEngine<ChartTemplateModel> _chartTemplateEngine;
    private readonly IReportService _reportService;
    private readonly IContentService _contentService;
    private readonly IFolderService _folderService;
    private readonly IHttpRequestClient _httpClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly ChartsOptions _chartsOptions;
    private readonly StorageOptions _storageOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportTemplateEngine"></param>
    /// <param name="chartTemplateEngine"></param>
    /// <param name="reportService"></param>
    /// <param name="contentService"></param>
    /// <param name="folderService"></param>
    /// <param name="httpClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="chartsOptions"></param>
    /// <param name="storageOptions"></param>
    /// <param name="serializerOptions"></param>
    public ReportHelper(
        ITemplateEngine<ReportTemplateModel> reportTemplateEngine,
        ITemplateEngine<ChartTemplateModel> chartTemplateEngine,
        IReportService reportService,
        IContentService contentService,
        IFolderService folderService,
        IHttpRequestClient httpClient,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<ChartsOptions> chartsOptions,
        IOptions<StorageOptions> storageOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportTemplateEngine = reportTemplateEngine;
        _chartTemplateEngine = chartTemplateEngine;
        _reportService = reportService;
        _contentService = contentService;
        _folderService = folderService;
        _httpClient = httpClient;
        _elasticOptions = elasticOptions.Value;
        _chartsOptions = chartsOptions.Value;
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
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ChartResultModel> GenerateJsonAsync(ChartRequestModel model)
    {
        if (model.Template == null) throw new InvalidOperationException("Chart template is missing from model");
        var template = _chartTemplateEngine.AddOrUpdateTemplateInMemory($"chart-template", model.Template);

        SearchResultModel<Areas.Services.Models.Content.ContentModel>? searchResults = null;
        var content = new List<ContentModel>(model.Content ?? Array.Empty<ContentModel>());
        if (model.Filter != null)
        {
            searchResults = await _contentService.FindWithElasticsearchAsync(model.Index ?? _elasticOptions.PublishedIndex, model.Filter);
            content.AddRange(searchResults.Hits.Hits.Select(h => new ContentModel(h.Source)).ToArray());
        }

        var templateModel = new ChartTemplateModel(model.Settings, content);

        var json = await template.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Content = templateModel.Content;
            instance.Settings = templateModel.Settings;
            instance.SectionSettings = templateModel.SectionSettings;
            instance.Sections = templateModel.Sections;
        });

        return new ChartResultModel(json, searchResults != null ? JsonSerializer.Serialize(searchResults) : null);
    }

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64ImageAsync(ChartRequestModel model)
    {
        // Get the Chart JSON data.
        var data = model.ChartData ?? (await this.GenerateJsonAsync(model)).Json;
        var dataJson = data.ToJson();

        var optionsJson = model.Settings.Options != null ? JsonSerializer.Serialize(model.Settings.Options) : "{}";
        var optionsBytes = Encoding.UTF8.GetBytes(optionsJson);
        var optionsBase64 = Convert.ToBase64String(optionsBytes);

        // Send request to Charts API to generate base64
        var body = new StringContent(dataJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync(
            _chartsOptions.Url.Append(_chartsOptions.Base64Path, model.Settings.ChartType ?? "bar", $"?width={model.Settings.Width ?? 250}&height={model.Settings.Height ?? 250}&options={optionsBase64}"),
            body);
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<object> GenerateReportAsync(Areas.Services.Models.Report.ReportModel model)
    {
        if (model.Template == null) throw new InvalidOperationException("Report template is missing from model");
        var subjectTemplate = _reportTemplateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}-subject", model.Template?.Subject ?? throw new InvalidOperationException("Report subject template must be provided."));
        var bodyTemplate = _reportTemplateEngine.AddOrUpdateTemplateInMemory($"report-{model.Id}", model.Template?.Body ?? throw new InvalidOperationException("Report body template must be provided."));

        var index = _elasticOptions.PublishedIndex;
        var elasticResults = await _reportService.FindContentWithElasticsearchAsync(index, model.ToEntity(_serializerOptions));

        // Link each result with the section name.
        var sections = model.Sections.ToDictionary(section => section.Name, section =>
        {
            elasticResults.TryGetValue(section.Name, out SearchResultModel<Areas.Services.Models.Content.ContentModel>? value);
            var content = value?.Hits.Hits.Select(hit => new ContentModel(hit.Source)).ToArray()
                ?? Array.Empty<ContentModel>();
            return new ReportSectionModel(section, content);
        });

        var templateModel = new ReportTemplateModel(sections, model.Settings, _storageOptions.GetUploadPath());

        var subject = await subjectTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Settings = templateModel.Settings;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });
        var body = await bodyTemplate.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Settings = templateModel.Settings;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
        });

        // Find all charts and make a request to the Charts API to generate the image.
        await model.Sections.ForEachAsync(async section =>
        {
            await section.ChartTemplates.ForEachAsync(async chart =>
            {
                chart.SectionSettings ??= new();
                var filter = section.Filter?.Query;
                List<ContentModel> content = new();
                if (section.Folder != null)
                {
                    // If the section references a folder then it will need to make a request for that content.
                    var folder = _folderService.FindById(section.Folder.Id) ?? throw new InvalidOperationException($"Folder 'ID:{section.Folder.Id}' does not exist");
                    content.AddRange(folder.ContentManyToMany
                        .Where(c => c.Content != null)
                        .OrderBy(c => c.SortOrder)
                        .Select(c => new ContentModel(c.Content!, c.SortOrder))
                        .ToArray());
                }
                // If the section has a filter then pull the content for this section from the above fetch.
                if (elasticResults.TryGetValue(section.Name, out SearchResultModel<Areas.Services.Models.Content.ContentModel>? sectionContent))
                {
                    if (sectionContent != null)
                        content.AddRange(sectionContent.Hits.Hits.Select(hit => new ContentModel(hit.Source)));
                }

                // TODO: Merge with report specific configuration options.
                if (chart.SectionSettings.Options == null || chart.SectionSettings.Options.ToJson() == "{}")
                    chart.SectionSettings.Options = chart.Settings.Options;
                var base64Image = await this.GenerateBase64ImageAsync(new ChartRequestModel(chart.SectionSettings, chart.Template, content));
                // Replace Chart Stubs with the generated image.
                body = body.Replace(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), base64Image);
            });
        });

        return new ReportResultModel(subject, body, elasticResults);
    }
    #endregion
}
