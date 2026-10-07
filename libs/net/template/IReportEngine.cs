using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Charts;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.TemplateEngine;

public interface IReportEngine
{
    /// <summary>
    /// Makes a request to Elasticsearch if required to fetch content.
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// If the model includes a Filter it will make a request to Elasticsearch.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ChartResultModel> GenerateJsonAsync(ChartRequestModel model, bool isPreview = false);

    /// <summary>
    /// Order the content based on the session field.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="sortBy"></param>
    /// <param name="direction"></param>
    /// <returns>Ordered Content</returns>
    /// <exception cref="InvalidOperationException"></exception>
    IEnumerable<ContentModel> OrderBySectionField(IEnumerable<ContentModel> content, string sortBy, string direction);

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    Task<string> GenerateBase64ImageAsync(ChartRequestModel model, bool isPreview = false);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstance"></param>
    /// <param name="sectionContent"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? reportInstance,
        Dictionary<string, ReportSectionModel> sectionContent,
        bool viewOnWebOnly = false,
        bool isPreview = false);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstance"></param>
    /// <param name="sectionContent"></param>
    /// <param name="getLinkedReport"></param>
    /// <param name="getPreviousReport"></param>
    /// <param name="getLLMAsync"></param>
    /// <param name="uploadPath"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <param name="aiWait">Whether to generate missing AI sections, or leave them pending.</param>
    /// <returns></returns>
    Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? reportInstance,
        Dictionary<string, ReportSectionModel> sectionContent,
        Func<int, int?, Task<Dictionary<string, ReportSectionModel>>> getLinkedReport,
        Func<int, int?, int?, int, Task<IEnumerable<PreviousReportModel>>> getPreviousReport,
        Func<int, Task<API.Areas.Services.Models.LLM.LLMModel?>> getLLMAsync,
        string? uploadPath = null,
        bool viewOnWebOnly = false,
        bool isPreview = false,
        AISectionWait aiWait = AISectionWait.Wait);

    /// <summary>
    /// Generate and store the report's missing AI sections, without the rest of the report. A section
    /// another generator holds is left to it.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="reportInstanceId"></param>
    /// <param name="sectionContent"></param>
    /// <param name="getPreviousReport"></param>
    /// <param name="getLLMAsync"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="viewContentUrlOverride">Link settings supplied by the requesting preview.</param>
    /// <returns></returns>
    Task PrepareReportAISectionsAsync(
        API.Areas.Services.Models.Report.ReportModel report,
        long? reportInstanceId,
        Dictionary<string, ReportSectionModel> sectionContent,
        Func<int, int?, int?, int, Task<IEnumerable<PreviousReportModel>>> getPreviousReport,
        Func<int, Task<API.Areas.Services.Models.LLM.LLMModel?>> getLLMAsync,
        CancellationToken cancellationToken = default,
        string? viewContentUrlOverride = null);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="eveningOverview"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<string> GenerateReportSubjectAsync(
        API.Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel eveningOverview,
        bool isPreview = false);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="reportTemplate"></param>
    /// <param name="eveningOverview"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<string> GenerateReportBodyAsync(
        API.Areas.Services.Models.AVOverview.ReportTemplateModel reportTemplate,
        AVOverviewInstanceModel eveningOverview,
        bool isPreview = false);
}
