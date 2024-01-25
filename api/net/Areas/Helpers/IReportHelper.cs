
using System.Text.Json;
using TNO.TemplateEngine.Models.Charts;
using TNO.TemplateEngine.Models.Reports;

namespace TNO.API.Helpers;

/// <summary>
/// IReportHelper interface, provides helper methods to generate reports and charts.
/// </summary>
public interface IReportHelper
{
    #region Properties
    #endregion

    #region Methods
    /// <summary>
    /// If a filter is provided make a request to Elasticsearch for content.
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    Task<IEnumerable<TNO.TemplateEngine.Models.ContentModel>> FindContentAsync(JsonDocument filter, string? index);

    /// <summary>
    /// Get the content from the current instance of the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    Task<Dictionary<string, ReportSectionModel>> GetLinkedReportContent(int reportId, int? ownerId = null);

    /// <summary>
    /// Makes a request to Elasticsearch if required to fetch content.
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ChartResultModel> GenerateJsonAsync(ChartRequestModel model, bool isPreview = false);

    /// <summary>
    /// Executes the chart template provided to generate JSON, which is then sent with a request to the Charts API to generate a base 64 image.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <returns>Returns the base64 image from the Charts API.</returns>
    Task<string> GenerateBase64ImageAsync(ChartRequestModel model, bool isPreview = false);

    /// <summary>
    /// Generate an instance of the report.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <param name="instanceId"></param>
    /// <returns></returns>
    Task<Entities.ReportInstance> GenerateReportInstanceAsync(
        Areas.Services.Models.Report.ReportModel model,
        int? requestorId = null,
        long instanceId = 0);

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
    Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        bool viewOnWebOnly = false,
        bool isPreview = false);

    /// <summary>
    /// Execute the report template to generate the subject and body.
    /// If the report sections contain charts it will also generate them and include them in the results.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <param name="viewOnWebOnly"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ReportResultModel> GenerateReportAsync(
        Areas.Services.Models.Report.ReportModel model,
        int? requestorId = null,
        bool viewOnWebOnly = false,
        bool isPreview = false);

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
    Task<ReportResultModel> GenerateReportAsync(
        AVOverviewInstanceModel instance,
        bool isPreview = false);
    #endregion
}
