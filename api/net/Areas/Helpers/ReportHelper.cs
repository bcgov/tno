
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Models.Charts;

namespace TNO.API.Helpers;

/// <summary>
/// ReportHelper class, provides helper methods to generate reports and charts.
/// </summary>
public class ReportHelper : IReportHelper
{
    #region Variables
    private readonly ITemplateEngine<TNO.TemplateEngine.Models.Reports.ChartTemplateModel> _chartTemplateEngine;
    private readonly IContentService _contentService;
    private readonly IHttpRequestClient _httpClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly ChartsOptions _chartsOptions;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="chartTemplateEngine"></param>
    /// <param name="contentService"></param>
    /// <param name="httpClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="chartsOptions"></param>
    public ReportHelper(
        ITemplateEngine<TNO.TemplateEngine.Models.Reports.ChartTemplateModel> chartTemplateEngine,
        IContentService contentService,
        IHttpRequestClient httpClient,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<ChartsOptions> chartsOptions)
    {
        _chartTemplateEngine = chartTemplateEngine;
        _contentService = contentService;
        _httpClient = httpClient;
        _elasticOptions = elasticOptions.Value;
        _chartsOptions = chartsOptions.Value;
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

        Elastic.Models.SearchResultModel<TNO.API.Areas.Services.Models.Content.ContentModel>? searchResults = null;
        List<TNO.TemplateEngine.Models.Reports.ContentModel> content = new();
        if (model.Content?.Any() == true)
        {
            content.AddRange(model.Content.Select(c => new TNO.TemplateEngine.Models.Reports.ContentModel(c)).ToArray());
        }
        if (model.Filter != null)
        {
            searchResults = await _contentService.FindWithElasticsearchAsync(model.Index ?? _elasticOptions.PublishedIndex, model.Filter);
            content.AddRange(searchResults.Hits.Hits.Select(h => new TNO.TemplateEngine.Models.Reports.ContentModel(h.Source)).ToArray());
        }

        var templateModel = new TNO.TemplateEngine.Models.Reports.ChartTemplateModel("chart", model.Settings, content);

        var json = await template.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
            instance.Settings = templateModel.Settings;
        });

        return new ChartResultModel(json, searchResults != null ? JsonSerializer.Serialize(searchResults) : null);
    }

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base 64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64Async(ChartRequestModel model)
    {
        // Get the Chart JSON data.
        var data = model.ChartData ?? (await this.GenerateJsonAsync(model)).Json;
        var dataJson = data.ToJson();

        var optionsJson = model.Settings.Options != null ? JsonSerializer.Serialize(model.Settings.Options) : "{}";
        var optionsBytes = System.Text.Encoding.UTF8.GetBytes(optionsJson);
        var optionsBase64 = Convert.ToBase64String(optionsBytes);

        // Send request to Charts API to generate base64
        var body = new StringContent(dataJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync(
            _chartsOptions.Url.Append(_chartsOptions.Base64Path, model.Settings.ChartType ?? "bar", $"?width={model.Width ?? 250}&height={model.Height ?? 250}&options={optionsBase64}"),
            body);
        return await response.Content.ReadAsStringAsync();
    }
    #endregion
}
