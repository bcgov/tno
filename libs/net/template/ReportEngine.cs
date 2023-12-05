
using System.Text;
using System.Text.Json;
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
    /// <param name="orderByField"></param>
    /// <returns>Ordered Content</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public ContentModel[] OrderBySectionField(ContentModel[] content, string orderByField)
    {
        return orderByField switch
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

        var optionsJson = model.ChartTemplate.SectionSettings.Options != null ? JsonSerializer.Serialize(model.ChartTemplate.SectionSettings.Options) : "{}";
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
    /// <param name="sectionContent"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        Dictionary<string, ReportSectionModel> sectionContent,
        bool isPreview = false)
    {
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");
        if (report.Template.ReportType != Entities.ReportType.Content) throw new InvalidOperationException("The report does not use a evening overview template.");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{report.Template.Id}-subject";
        var template = this.ReportEngineContent.GetOrAddTemplateInMemory(key, report.Template.Subject)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineContentModel(report, sectionContent);
        return await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
        });
    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sectionContent"></param>
    /// <param name="uploadPath"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        Dictionary<string, ReportSectionModel> sectionContent,
        string? uploadPath = null,
        bool isPreview = false)
    {
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");
        if (report.Template.ReportType != Entities.ReportType.Content) throw new InvalidOperationException("The report does not use a evening overview template.");

        var key = (isPreview ? "PREVIEW" : "FINAL") + $"-report-template-{report.Template.Id}-body";
        var template = this.ReportEngineContent.GetOrAddTemplateInMemory(key, report.Template.Body)
            ?? throw new InvalidOperationException("Template does not exist");

        var model = new ReportEngineContentModel(report, sectionContent, uploadPath);
        var body = await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;

            instance.SubscriberAppUrl = this.TemplateOptions.SubscriberAppUrl;
            instance.ViewContentUrl = this.TemplateOptions.ViewContentUrl;
            instance.RequestTranscriptUrl = this.TemplateOptions.RequestTranscriptUrl;
            instance.AddToReportUrl = this.TemplateOptions.AddToReportUrl;
        });

        var aggregateSection = new Dictionary<string, ReportSectionModel>();
        // Find all charts and make a request to the Charts API to generate the image.
        await report.Sections.ForEachAsync(async section =>
        {
            var settings = section.Settings;
            List<ContentModel> content = new();

            // If the section has content add it to the chart request.
            if (sectionContent.TryGetValue(section.Name, out ReportSectionModel? sectionData) && sectionData != null)
            {
                content.AddRange(sectionData.Content);
                aggregateSection.Add(section.Name, sectionData);
            }

            await section.ChartTemplates.ForEachAsync(async chart =>
            {
                // TODO: Merge with report specific configuration options.
                chart.SectionSettings ??= new();
                if (chart.SectionSettings.Options == null || chart.SectionSettings.Options.ToJson() == "{}")
                    chart.SectionSettings.Options = chart.Settings.Options;

                var chartModel = new ChartEngineContentModel(
                    ReportSectionModel.GenerateChartUid(section.Id, chart.Id),
                    chart,
                    aggregateSection,
                    settings.SectionType == Entities.ReportSectionType.Content ? content : null);
                var chartRequestModel = new ChartRequestModel(chartModel);
                var base64Image = await this.GenerateBase64ImageAsync(chartRequestModel);

                // Replace Chart Stubs with the generated image.
                body = body.Replace(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), base64Image);
            });
        });

        return body;
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
