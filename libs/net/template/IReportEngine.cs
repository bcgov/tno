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
    /// <param name="updateCache"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ChartResultModel> GenerateJsonAsync(ChartRequestModel model, bool updateCache = false);

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="updateCache"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    Task<string> GenerateBase64ImageAsync(ChartRequestModel model, bool updateCache = false);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sectionContent"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    Task<string> GenerateReportSubjectAsync(API.Areas.Services.Models.Report.ReportModel report, Dictionary<string, ReportSectionModel> sectionContent, bool updateCache = false);

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="sectionContent"></param>
    /// <param name="uploadPath"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    Task<string> GenerateReportBodyAsync(API.Areas.Services.Models.Report.ReportModel report, Dictionary<string, ReportSectionModel> sectionContent, string? uploadPath = null, bool updateCache = false);
}
