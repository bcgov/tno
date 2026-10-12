
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
    Task<Dictionary<string, ReportSectionModel>> GetLinkedReportContentAsync(int reportId, int? ownerId = null);

    /// <summary>
    /// Get the content from the previous instance of the specified 'reportId' and 'ownerId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instanceId"></param>
    /// <param name="ownerId"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    Task<IEnumerable<PreviousReportModel>> GetPreviousReportsAsync(int reportId, int? instanceId, int? ownerId = null, int qty = 1);

    /// <summary>
    /// Get the content from the previous instance of the specified 'id' and 'ownerId'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Areas.Services.Models.LLM.LLMModel?> GetLLMAsync(int id);

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
    /// Render a historical instance with explicitly populated content and aggregations.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="sections"></param>
    /// <returns></returns>
    Task<ReportResultModel> GenerateReportSectionsAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        Dictionary<string, ReportSectionModel> sections);

    /// <summary>
    /// Preview the report instance without waiting for its AI sections. A section without a stored
    /// result shows that it is being generated, and the reporting service is asked to generate it;
    /// the requestor is notified when it is ready.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId">The user to notify when the AI sections are ready.</param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    Task<ReportResultModel> PreviewReportInstanceAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        int requestorId,
        bool isPreview = true);

    /// <summary>
    /// Describe the requests the AI section would send for the instance's content, without sending
    /// any. When 'settingsOverride' is given it stands in for the section's saved settings.
    /// </summary>
    /// <param name="model">The instance with its report and content.</param>
    /// <param name="sectionId">The AI section.</param>
    /// <param name="settingsOverride">Draft settings, or null for the saved ones.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    Task<Areas.Services.Models.ReportAIResult.ReportAISectionPromptModel> DescribeReportAISectionAsync(
        Areas.Services.Models.ReportInstance.ReportInstanceModel model,
        int sectionId,
        Models.Settings.ReportSectionSettingsModel? settingsOverride = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ask the reporting service to run the AI section once with draft settings against the
    /// instance's content and post the outcome back for the requestor.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="instanceId"></param>
    /// <param name="testId"></param>
    /// <param name="sectionId"></param>
    /// <param name="settings"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    Task RequestReportAISectionTestAsync(int reportId, long instanceId, Guid testId, int sectionId, Models.Settings.ReportSectionSettingsModel settings, int requestorId);

    /// <summary>
    /// Ask the section's model how to change the section's prompt. The first turn (no messages)
    /// composes the context; later turns replay the conversation the browser holds.
    /// </summary>
    /// <param name="sectionSettings">The draft section settings.</param>
    /// <param name="request">The question, the conversation so far, and the test run to discuss.</param>
    /// <param name="test">The test run the conversation is about, when one was made.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    Task<Areas.Subscriber.Models.Report.ReportAIPromptChatResultModel> ChatAboutReportAISectionAsync(
        Models.Settings.ReportSectionSettingsModel sectionSettings,
        Areas.Subscriber.Models.Report.ReportAIPromptChatRequestModel request,
        Areas.Services.Models.ReportAIResult.ReportAISectionTestResultModel? test,
        CancellationToken cancellationToken = default);

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
