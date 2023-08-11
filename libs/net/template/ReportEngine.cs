
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.TemplateEngine.Config;
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
    /// get - Report template engine.
    /// </summary>
    protected ITemplateEngine<ReportEngineTemplateModel> ReportTemplateEngine { get; }

    /// <summary>
    /// get - Chart template engine.
    /// </summary>
    protected ITemplateEngine<ChartEngineTemplateModel> ChartTemplateEngine { get; }

    /// <summary>
    /// get - HTTP client.
    /// </summary>
    protected IHttpRequestClient HttpClient { get; }

    /// <summary>
    /// get - Charts options.
    /// </summary>
    protected ChartsOptions ChartsOptions { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEngine object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportTemplateEngine"></param>
    /// <param name="chartTemplateEngine"></param>
    /// <param name="httpClient"></param>
    /// <param name="chartsOptions"></param>
    public ReportEngine(
        ITemplateEngine<ReportEngineTemplateModel> reportTemplateEngine,
        ITemplateEngine<ChartEngineTemplateModel> chartTemplateEngine,
        IHttpRequestClient httpClient,
        IOptions<ChartsOptions> chartsOptions)
    {
        this.ReportTemplateEngine = reportTemplateEngine;
        this.ChartTemplateEngine = chartTemplateEngine;
        this.HttpClient = httpClient;
        this.ChartsOptions = chartsOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Makes a request to Elasticsearch if required to fetch content.
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// If the model includes a Filter it will make a request to Elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<ChartResultModel> GenerateJsonAsync(
        ChartRequestModel model,
        bool updateCache = false)
    {
        var key = $"chart-template-{model.ChartTemplate.Id}";
        var template = updateCache
            ? this.ChartTemplateEngine.AddOrUpdateTemplateInMemory(key, model.ChartTemplate.Template)
            : this.ChartTemplateEngine.GetOrAddTemplateInMemory(key, model.ChartTemplate.Template);

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
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateCache"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    public async Task<string> GenerateBase64ImageAsync(
        ChartRequestModel model,
        bool updateCache = false)
    {
        // Get the Chart JSON data.
        var data = model.ChartData ?? (await this.GenerateJsonAsync(model, updateCache)).Json;
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

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sectionContent"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    public async Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        Dictionary<string, ReportSectionModel> sectionContent,
        bool updateCache = false)
    {
        var key = $"report-{report.Id}-subject";
        var model = new TNO.TemplateEngine.Models.Reports.ReportEngineTemplateModel(sectionContent, report.Settings);
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");
        var template = (!updateCache ?
            this.ReportTemplateEngine.GetOrAddTemplateInMemory(key, report.Template.Subject) :
            this.ReportTemplateEngine.AddOrUpdateTemplateInMemory(key, report.Template.Subject))
            ?? throw new InvalidOperationException("Template does not exist");
        return await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;
        });
    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sectionContent"></param>
    /// <param name="uploadPath"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    public async Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        Dictionary<string, ReportSectionModel> sectionContent,
        string? uploadPath = null,
        bool updateCache = false)
    {
        var key = $"report-{report.Id}";
        var model = new TNO.TemplateEngine.Models.Reports.ReportEngineTemplateModel(sectionContent, report.Settings, uploadPath);
        if (report.Template == null) throw new InvalidOperationException("Report template is missing from model");

        var template = (!updateCache ?
            this.ReportTemplateEngine.GetOrAddTemplateInMemory(key, report.Template.Body) :
            this.ReportTemplateEngine.AddOrUpdateTemplateInMemory(key, report.Template.Body))
            ?? throw new InvalidOperationException("Template does not exist");

        var body = await template.RunAsync(instance =>
        {
            instance.Model = model;
            instance.Settings = model.Settings;
            instance.Content = model.Content;
            instance.Sections = model.Sections;
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

                var chartModel = new ChartEngineTemplateModel(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), chart, aggregateSection, !settings.IsSummary ? content : null);
                var chartRequestModel = new ChartRequestModel(chartModel);
                var base64Image = await this.GenerateBase64ImageAsync(chartRequestModel);

                // Replace Chart Stubs with the generated image.
                body = body.Replace(ReportSectionModel.GenerateChartUid(section.Id, chart.Id), base64Image);
            });
        });

        return body;
    }
    #endregion
}
