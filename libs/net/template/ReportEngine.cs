
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
    ///  logger
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
    /// <param name="logger"></param>
    public ReportEngine(
        ITemplateEngine<ReportEngineContentModel> reportEngineContent,
        ITemplateEngine<ReportEngineAVOverviewModel> reportEngineAVOverview,
        ITemplateEngine<ChartEngineContentModel> chartEngineContent,
        IHttpRequestClient httpClient,
        IOptions<TemplateOptions> templateOptions,
        IOptions<ChartsOptions> chartsOptions,
        ILogger<ReportEngine> logger)
    {
        this.ReportEngineContent = reportEngineContent;
        this.ReportEngineAVOverview = reportEngineAVOverview;
        this.ChartEngineContent = chartEngineContent;
        this.HttpClient = httpClient;
        this.TemplateOptions = templateOptions.Value;
        this.ChartsOptions = chartsOptions.Value;
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
    /// <returns>Ordered Content</returns>
    public IEnumerable<ContentModel> OrderBySectionField(IEnumerable<ContentModel> content, string sortBy)
    {
        // If you edit this function, also edit the related function in ReportService.OrderBySectionField
        return sortBy switch
        {
            "PublishedOn" => content.OrderBy(c => c.PublishedOn).ToArray(),
            "MediaType" => content.OrderBy(c => c.MediaType?.Name).ToArray(),
            "Series" => content.OrderBy(c => c.Series?.Name).ToArray(),
            "Source" => content.OrderBy(c => c.Source?.Name).ToArray(),
            "Sentiment" => content.OrderBy(c => c.TonePools.Select(s => s.Value).Sum(v => v)).ToArray(),
            "Byline" => content.OrderBy(c => c.Byline).ToArray(),
            "Contributor" => content.OrderBy(c => c.Contributor?.Name).ToArray(),
            "Topic" => content.OrderBy(c => string.Join(",", c.Topics.Select(x => x.Name).ToList())).ToArray(),
            _ => content.OrderBy(c => c.SortOrder).ToArray(),
        };
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
        return await template.RunAsync(instance =>
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

                    // Replace Chart Stubs with the generated image.
                    body = body.Replace(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), base64Image);
                });
            });
        }

        return body;
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
        var json = JsonNode.Parse(chartTemplateSettings.Options.ToJson())?.AsObject();
        if (json == null) return chartTemplateSettings.Options;

        try
        {

            if (sectionSettings.IsHorizontal.HasValue)
            {
                // There appears to be no way to modify a value...
                if (json.ContainsKey("indexAxis"))
                    json.Remove("indexAxis");
                json.Add("indexAxis", sectionSettings.IsHorizontal.Value ? "y" : "x");
            }

            if (sectionSettings.ShowAxis.HasValue)
            {
                if (json.TryGetPropertyValue("scales", out JsonNode? scales))
                {
                    if (scales?.AsObject().TryGetPropertyValue("x", out JsonNode? scalesX) == true)
                    {
                        if (scalesX?.AsObject().ContainsKey("display") == true)
                            scalesX.AsObject().Remove("display");
                        scalesX?.AsObject().Add("display", sectionSettings.ShowAxis);
                    }
                    else
                    {
                        scales?.AsObject().Add("x", JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowAxis} }}"));
                    }

                    if (scales?.AsObject().TryGetPropertyValue("y", out JsonNode? scalesY) == true)
                    {
                        if (scalesY?.AsObject().ContainsKey("display") == true)
                            scalesY.AsObject().Remove("display");
                        scalesY?.AsObject().Add("display", sectionSettings.ShowAxis);
                    }
                    else
                    {
                        scales?.AsObject().Add("y", JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowAxis} }}"));
                    }
                }
                else
                {
                    scales = JsonNode.Parse($"{{ \"scales\": {{}} }}");
                    scales?.AsObject().Add("x", JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowAxis} }}"));
                    scales?.AsObject().Add("y", JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowAxis} }}"));
                }
            }

            if (sectionSettings.ShowLegend.HasValue || sectionSettings.ShowLegendTitle.HasValue || !String.IsNullOrWhiteSpace(sectionSettings.Title))
            {
                if (json.TryGetPropertyValue("plugins", out JsonNode? plugins))
                {
                    if (plugins?.AsObject().TryGetPropertyValue("legend", out JsonNode? legend) == true)
                    {
                        if (legend?.AsObject().ContainsKey("display") == true)
                            legend.AsObject().Remove("display");
                        legend?.AsObject().Add("display", sectionSettings.ShowLegend ?? true);

                        if (sectionSettings.ShowLegendTitle.HasValue || !String.IsNullOrWhiteSpace(sectionSettings.Title))
                        {
                            if (legend?.AsObject().TryGetPropertyValue("title", out JsonNode? title) == true)
                            {
                                if (title?.AsObject().ContainsKey("display") == true)
                                    title.AsObject().Remove("display");
                                title?.AsObject().Add("display", sectionSettings.ShowLegendTitle ?? true);

                                if (!String.IsNullOrWhiteSpace(sectionSettings.Title))
                                {
                                    if (title?.AsObject().ContainsKey("text") == true)
                                        title.AsObject().Remove("text");
                                    title?.AsObject().Add("text", sectionSettings.Title ?? "");
                                }
                            }
                            else
                            {
                                if (!String.IsNullOrWhiteSpace(sectionSettings.Title))
                                {
                                    title = JsonNode.Parse($"{{ \"title\": {{ \"text\": \"{sectionSettings.Title ?? ""}\", \"display\": {sectionSettings.ShowLegendTitle ?? true} }} }}");
                                    legend?.AsObject().Add("title", title);
                                }
                            }
                        }
                    }
                    else
                    {
                        legend = JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowLegend}, \"title\": {{ \"text\": \"{sectionSettings.Title ?? ""}\", \"display\": {sectionSettings.ShowLegendTitle ?? true} }} }}");
                        plugins?.AsObject().Add("legend", legend);
                    }
                }
                else
                {
                    plugins = JsonNode.Parse($"{{ \"legend\": {{ \"display\": {sectionSettings.ShowLegend}, \"title\": {{ \"text\": \"{sectionSettings.Title ?? ""}\", \"display\": {sectionSettings.ShowLegendTitle ?? true} }} }} }}");
                    json.Add("plugins", plugins);
                }
            }

            if (sectionSettings.ShowDataLabels.HasValue)
            {
                if (json.TryGetPropertyValue("plugins", out JsonNode? plugins))
                {
                    if (plugins?.AsObject().TryGetPropertyValue("datalabels", out JsonNode? datalabels) == true)
                    {
                        if (datalabels?.AsObject().TryGetPropertyValue("labels", out JsonNode? labels) == true)
                        {
                            if (labels?.AsObject().TryGetPropertyValue("title", out JsonNode? title) == true)
                            {
                                if (title?.AsObject().ContainsKey("display") == true)
                                    title?.AsObject().Remove("display");
                                title?.AsObject().Add("display", sectionSettings.ShowDataLabels.Value);
                            }
                            else
                            {
                                title = JsonNode.Parse($"{{ \"display\": {sectionSettings.ShowDataLabels.Value} }}");
                                labels?.AsObject().Add("title", title);
                            }
                        }
                        else
                        {
                            labels = JsonNode.Parse($"{{ \"title\": {{ \"display\": {sectionSettings.ShowDataLabels.Value} }} }}");
                            datalabels?.AsObject().Add("labels", labels);
                        }
                    }
                    else
                    {
                        datalabels = JsonNode.Parse($"{{ \"anchor\": \"center\", \"labels\": {{ \"title\": {{ \"display\": {sectionSettings.ShowDataLabels.Value} }} }} }}");
                        plugins?.AsObject().Add("datalabels", datalabels);
                    }
                }
                else
                {
                    plugins = JsonNode.Parse($"{{ \"datalabels\": {{ \"anchor\": \"center\", \"labels\": {{ \"title\": {{ \"display\": {sectionSettings.ShowLegendTitle ?? true} }} }} }} }}");
                    json.Add("plugins", plugins);
                }
            }

            return JsonDocument.Parse(json.ToJsonString());
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
