
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.TemplateEngine.Config;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Charts;
using TNO.TemplateEngine.Models.Charts.Options;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

/// <summary>
/// ReportEngine class, provides a centralize collection of methods to generate reports and charts.
/// </summary>
public class ReportEngine : IReportEngine
{

    #region Properties
    /// <summary>
    /// get - Report template engine for content.
    /// </summary>
    protected ITemplateEngine<ReportEngineContentModel> ReportEngineContent { get; }

    /// <summary>
    /// get - Report template engine for evening overview.
    /// </summary>
    protected ITemplateEngine<ReportEngineAVOverviewModel> ReportEngineAVOverview { get; }

    /// <summary>
    /// get - Chart template engine for content.
    /// </summary>
    protected ITemplateEngine<ChartEngineContentModel> ChartEngineContent { get; }

    /// <summary>
    /// get - HTTP client.
    /// </summary>
    protected IHttpRequestClient HttpClient { get; }

    /// <summary>
    /// get - Template options.
    /// </summary>
    protected TemplateOptions TemplateOptions { get; }

    /// <summary>
    /// get - Charts options.
    /// </summary>
    protected ChartsOptions ChartsOptions { get; }

    /// <summary>
    /// get - Serialization options.
    /// </summary>
    protected JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// get - logger
    /// </summary>
    protected ILogger<ReportEngine> Logger { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportEngineContent"></param>
    /// <param name="reportEngineAVOverview"></param>
    /// <param name="chartEngineContent"></param>
    /// <param name="httpClient"></param>
    /// <param name="templateOptions"></param>
    /// <param name="chartsOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public ReportEngine(
        ITemplateEngine<ReportEngineContentModel> reportEngineContent,
        ITemplateEngine<ReportEngineAVOverviewModel> reportEngineAVOverview,
        ITemplateEngine<ChartEngineContentModel> chartEngineContent,
        IHttpRequestClient httpClient,
        IOptions<TemplateOptions> templateOptions,
        IOptions<ChartsOptions> chartsOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ReportEngine> logger)
    {
        this.ReportEngineContent = reportEngineContent;
        this.ReportEngineAVOverview = reportEngineAVOverview;
        this.ChartEngineContent = chartEngineContent;
        this.HttpClient = httpClient;
        this.TemplateOptions = templateOptions.Value;
        this.ChartsOptions = chartsOptions.Value;
        this.SerializerOptions = serializerOptions.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
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
        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-chart-template-{model.ChartTemplate.Id}";
        var template = this.ChartEngineContent.GetOrAddTemplateInMemory(key, model.ChartTemplate.Template);

        var json = await template.RunAsync(instance =>
        {
            instance.Model = model.ChartTemplate;
            instance.Content = model.ChartTemplate.Content;
            instance.Settings = model.ChartTemplate.Settings;
            instance.SectionSettings = model.ChartTemplate.SectionSettings;
            instance.Sections = model.ChartTemplate.Sections;
        });

        return new ChartResultModel(json);
    }

    /// <summary>
    /// Order the content based on the session field.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="sortBy"></param>
    /// <param name="direction"></param>
    /// <returns>Ordered Content</returns>
    public IEnumerable<ContentModel> OrderBySectionField(IEnumerable<ContentModel> content, string sortBy, string? direction)
    {
        // If you edit this function, also edit the related function in ReportService.OrderBySectionField
        IEnumerable<ContentModel> results;

        if (direction == "desc")
        {
            results = sortBy switch
            {
                "Headline" => content.OrderByDescending(c => c.Headline),
                "PublishedOn" => content.OrderByDescending(c => c.PublishedOn),
                "MediaType" => content.OrderByDescending(c => c.MediaType?.Name),
                "Series" => content.OrderByDescending(c => c.Series?.Name),
                "Source" => content.OrderByDescending(c => c.Source?.SortOrder).ThenByDescending(c => c.OtherSource),
                "Sentiment" => content.OrderByDescending(c => c.TonePools.Select(s => s.Value).Sum(v => v)), // TODO: Support custom sentiment.
                "Byline" => content.OrderByDescending(c => c.Byline),
                "Contributor" => content.OrderByDescending(c => c.Contributor?.Name),
                "Topic" => content.OrderByDescending(c => string.Join(",", c.Topics.Select(x => x.Name) ?? Array.Empty<string>())),
                _ => content.OrderByDescending(c => c.SortOrder),
            };
        }
        else
        {
            results = sortBy switch
            {
                "Headline" => content.OrderBy(c => c.Headline),
                "PublishedOn" => content.OrderBy(c => c.PublishedOn),
                "MediaType" => content.OrderBy(c => c.MediaType?.Name),
                "Series" => content.OrderBy(c => c.Series?.Name),
                "Source" => content.OrderBy(c => c.Source?.SortOrder).ThenBy(c => c.OtherSource),
                "Sentiment" => content.OrderBy(c => c.TonePools.Select(s => s.Value).Sum(v => v)), // TODO: Support custom sentiment.
                "Byline" => content.OrderBy(c => c.Byline),
                "Contributor" => content.OrderBy(c => c.Contributor?.Name),
                "Topic" => content.OrderBy(c => string.Join(",", c.Topics.Select(x => x.Name) ?? Array.Empty<string>())),
                _ => content.OrderBy(c => c.SortOrder),
            };
        }

        var sortOrder = 0;
        return results.Select(c =>
        {
            c.SortOrder = sortOrder++;
            return c;
        }).ToArray();
    }

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64ImageAsync(
        ChartRequestModel model,
        bool isPreview = false)
    {
        // Get the Chart JSON data.
        var data = model.ChartData ?? (await this.GenerateJsonAsync(model, isPreview)).Json;
        var dataJson = data.ToJson();

        // If chart settings require auto scale
        if (model.ChartTemplate.SectionSettings.ScaleCalcMax.HasValue)
        {
            // Determine the maximum scale and add the auto max to it.
            var dataModel = JsonSerializer.Deserialize<ChartDataModel>(dataJson, this.SerializerOptions);
            var max = dataModel?.Datasets.Any() == true ? dataModel?.Datasets.Max(ds => ds.Data.Any() ? ds.Data.Max(v => v ?? 0) : 0) : null;
            if (max.HasValue)
            {
                var suggestedMax = (int)max + model.ChartTemplate.SectionSettings.ScaleCalcMax.Value;
                var sectionJsonText = model.ChartTemplate.SectionSettings.Options.ToJson();
                if (sectionJsonText != "{}")
                {
                    var chartOptions = JsonSerializer.Deserialize<ChartOptionsModel>(model.ChartTemplate.SectionSettings.Options);
                    if (chartOptions != null)
                    {
                        chartOptions.Scales.X.SuggestedMax = suggestedMax;
                        chartOptions.Scales.Y.SuggestedMax = suggestedMax;
                        model.ChartTemplate.SectionSettings.Options = JsonDocument.Parse(JsonSerializer.Serialize(chartOptions, this.SerializerOptions));
                    }
                }
            }
        }

        var optionsJson = model.ChartTemplate.SectionSettings.Options != null ? JsonSerializer.Serialize(MergeChartOptions(model.ChartTemplate.Settings, model.ChartTemplate.SectionSettings)) : "{}";
        // Modify the chart options based on section settings.
        var optionsBytes = Encoding.UTF8.GetBytes(optionsJson);
        var optionsBase64 = Convert.ToBase64String(optionsBytes);

        // Send request to Charts API to generate base64
        var body = new StringContent(dataJson, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);
        var response = await this.HttpClient.PostAsync(
            this.ChartsOptions.Url.Append(
                this.ChartsOptions.Base64Path,
                model.ChartTemplate.SectionSettings.ChartType ?? "bar",
                $"?width={model.ChartTemplate.SectionSettings.Width ?? 250}&height={model.ChartTemplate.SectionSettings.Height ?? 250}&options={optionsBase64}"),
            body);
        return await response.Content.ReadAsStringAsync();
    }

    #region Content
    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="sectionContent"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? reportInstance,
        Dictionary<string, ReportSectionModel> sectionContent,
        bool viewOnWebOnly = false,
        bool isPreview = false)
    {
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");
        if (report.Template.ReportType != Entities.ReportType.Content) throw new InvalidOperationException("The report does not use a evening overview template.");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{report.Template.Id}-subject";
        var template = this.ReportEngineContent.GetOrAddTemplateInMemory(key, report.Template.Subject)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineContentModel(report, reportInstance, sectionContent, this.TemplateOptions);
        var subject = await template.RunAsync(instance =>
        {
            instance.ReportId = report.Id;
            instance.ReportInstanceId = reportInstance?.Id;
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;
            instance.ViewOnWebOnly = viewOnWebOnly;
            instance.OwnerId = model.OwnerId;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });

        return subject.RemoveInvalidUtf8Characters().RemoveInvalidUnicodeCharacters();
    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="sectionContent"></param>
    /// <param name="getLinkedReport"></param>
    /// <param name="uploadPath"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? reportInstance,
        Dictionary<string, ReportSectionModel> sectionContent,
        Func<int, int?, Task<Dictionary<string, ReportSectionModel>>> getLinkedReportAsync,
        string? uploadPath = null,
        bool viewOnWebOnly = false,
        bool isPreview = false)
    {
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");
        if (report.Template.ReportType != Entities.ReportType.Content) throw new InvalidOperationException("The report does not use a evening overview template.");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{report.Template.Id}-body";
        var template = this.ReportEngineContent.GetOrAddTemplateInMemory(key, report.Template.Body)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineContentModel(report, reportInstance, sectionContent, this.TemplateOptions, uploadPath);
        var body = await template.RunAsync(instance =>
        {
            instance.ReportId = report.Id;
            instance.ReportInstanceId = reportInstance?.Id;
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;
            instance.ViewOnWebOnly = viewOnWebOnly;
            instance.OwnerId = model.OwnerId;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });

        if (!viewOnWebOnly)
        {
            var aggregateSection = new Dictionary<string, ReportSectionModel>();
            // Collect all section content for report summary charts.
            report.Sections.Where(section => section.SectionType == Entities.ReportSectionType.Content && section.IsEnabled).ForEach(section =>
            {
                // If the section has content add it to the chart request.
                if (sectionContent.TryGetValue(section.Name, out ReportSectionModel? sectionData) && sectionData != null)
                {
                    aggregateSection.Add(section.Name, sectionData);
                }
            });

            await report.Sections.Where(section => section.SectionType == Entities.ReportSectionType.MediaAnalytics && section.IsEnabled).ForEachAsync(async section =>
            {
                var settings = section.Settings;
                List<ContentModel> content = new();
                var linkedReport = new Dictionary<string, ReportSectionModel>();

                // If the section has content add it to the chart request.
                if (!settings.UseAllContent && sectionContent.TryGetValue(section.Name, out ReportSectionModel? sectionData) && sectionData != null)
                {
                    content.AddRange(sectionData.Content);
                }
                if (section.LinkedReportId.HasValue)
                {
                    // Make request for linked report content if the current instance doesn't have any content for this section.
                    linkedReport = await getLinkedReportAsync(section.LinkedReportId.Value, null);
                }

                await section.ChartTemplates.ForEachAsync(async chart =>
                {
                    chart.SectionSettings ??= new();
                    chart.SectionSettings.Options = MergeChartOptions(chart.Settings, chart.SectionSettings);

                    var chartModel = new ChartEngineContentModel(
                        ReportSectionModel.GenerateChartUid(section.Id, chart.Id),
                        chart,
                        settings.UseAllContent ? aggregateSection : linkedReport,
                        settings.UseAllContent ? null : content);
                    var chartRequestModel = new ChartRequestModel(chartModel);
                    var base64Image = await this.GenerateBase64ImageAsync(chartRequestModel);
                    this.Logger.LogDebug("Chart generated, sectionId:{sectionId} chartId:{chartId}", section.Id, chart.Id);

                    // Replace Chart Stubs with the generated image.
                    body = body.Replace(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), base64Image);
                });
            });
        }

        return body.RemoveInvalidUtf8Characters().RemoveInvalidUnicodeCharacters();
    }

    /// <summary>
    /// Each chart template has its own default settings.
    /// A section can override those setting options.
    /// </summary>
    /// <param name="chartTemplateSettings"></param>
    /// <param name="sectionSettings"></param>
    /// <returns></returns>
    private JsonDocument MergeChartOptions(API.Models.Settings.ChartTemplateSettingsModel chartTemplateSettings, API.Models.Settings.ChartSectionSettingsModel sectionSettings)
    {
        var defaultJsonText = chartTemplateSettings.Options.ToJson();
        var defaultJson = JsonNode.Parse(defaultJsonText)?.AsObject();
        if (defaultJson == null || defaultJsonText == "{}") return sectionSettings.Options;

        var sectionJsonText = sectionSettings.Options.ToJson();
        var sectionJson = JsonNode.Parse(sectionJsonText)?.AsObject();
        if (sectionJson == null || sectionJsonText == "{}") return chartTemplateSettings.Options;

        try
        {
            // There appears to be no way to modify a value...
            if (sectionJson.TryGetPropertyValue("indexAxis", out JsonNode? sectionIndexAxis))
            {
                if (defaultJson.ContainsKey("indexAxis"))
                    defaultJson.Remove("indexAxis");
                defaultJson.Add("indexAxis", sectionIndexAxis.CopyNode());
            }

            if (sectionJson.TryGetPropertyValue("scales", out JsonNode? sectionScales))
            {
                if (defaultJson.TryGetPropertyValue("scales", out JsonNode? scales))
                    defaultJson.Remove("scales");
                defaultJson.Add("scales", sectionScales.CopyNode());
            }

            if (sectionJson.TryGetPropertyValue("plugins", out JsonNode? sectionPlugins))
            {
                if (defaultJson.TryGetPropertyValue("plugins", out JsonNode? plugins))
                    defaultJson.Remove("plugins");
                defaultJson.Add("plugins", sectionPlugins.CopyNode());
            }

            return JsonDocument.Parse(defaultJson.ToJsonString());
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to modify chart options");
        }

        return chartTemplateSettings.Options;
    }
    #endregion

    #region AV Overview
    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="eveningOverview"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel eveningOverview,
        bool isPreview = false)
    {
        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{reportTemplate.Id}-subject";
        var template = this.ReportEngineAVOverview.GetOrAddTemplateInMemory(key, reportTemplate.Subject)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineAVOverviewModel(eveningOverview, eveningOverview.Settings);
        return await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Instance = model.Instance;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });
    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="eveningOverview"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel eveningOverview,
        bool isPreview = false)
    {
        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{reportTemplate.Id}-body";
        var template = this.ReportEngineAVOverview.AddOrUpdateTemplateInMemory(key, reportTemplate.Body)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineAVOverviewModel(eveningOverview, eveningOverview.Settings);
        return await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Instance = model.Instance;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });
    }
    #endregion
    #endregion
}
