using TNO.Core.Http;

namespace TNO.Services;

/// <summary>
/// IApiService interface, provides a way to interact with the API.
/// </summary>
public interface IApiService
{
    #region Properties
    /// <summary>
    /// get - HTTP client to make HTTP requests.
    /// </summary>
    IOpenIdConnectRequestClient OpenClient { get; }
    #endregion

    #region Helper Methods
    public Task<T> HandleRequestFailure<T>(Func<Task<T>> callbackDelegate, bool ignoreError, T defaultResponse);
    #endregion

    #region Kafka
    /// <summary>
    /// Publish content to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.SourceContent>?> SendMessageAsync(string topic, TNO.Kafka.Models.SourceContent content);

    /// <summary>
    /// Publish notification request to Kafka.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.NotificationRequestModel>?> SendMessageAsync(TNO.Kafka.Models.NotificationRequestModel request);

    /// <summary>
    /// Publish report request to Kafka.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.ReportRequestModel>?> SendMessageAsync(TNO.Kafka.Models.ReportRequestModel request);
    #endregion

    #region Lookups
    /// <summary>
    /// Make a request to the API to get the lookups.
    /// </summary>
    /// <returns></returns>
    public Task<API.Areas.Editor.Models.Lookup.LookupModel?> GetLookupsAsync();
    #endregion

    #region Sources
    /// <summary>
    /// Make a request to the API to fetch all sources.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<API.Areas.Services.Models.Ingest.SourceModel>> GetSourcesAsync();

    /// <summary>
    /// Make a request to the API to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.SourceModel?> GetSourceForCodeAsync(string code);
    #endregion

    #region Connections
    /// <summary>
    /// Make a request to the API to get the connection for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.ConnectionModel?> GetConnectionAsync(int id);
    #endregion

    #region Data Locations
    /// <summary>
    /// Make a request to the API to get the data location for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.DataLocation.DataLocationModel?> GetDataLocationAsync(int id);

    /// <summary>
    /// Make a request to the API to get the data location for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.DataLocation.DataLocationModel?> GetDataLocationAsync(string name);
    #endregion

    #region Ingests
    /// <summary>
    /// Make a request to the API to fetch the ingest for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.IngestModel?> GetIngestAsync(int id);

    /// <summary>
    /// Make a request to the API to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsAsync();

    /// <summary>
    /// Make a request to the API to fetch ingests for the specified ingest type.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    public Task<IEnumerable<API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsForIngestTypeAsync(string ingestType);

    /// <summary>
    /// Make a request to the API to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public Task<IEnumerable<API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsForTopicAsync(string topic);
    #endregion

    #region Ingest Schedules
    /// <summary>
    /// Delete the specified 'schedule' from the ingests.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.ScheduleModel?> DeleteIngestSchedule(API.Areas.Services.Models.Ingest.IngestScheduleModel schedule);
    #endregion

    #region Ingest
    /// <summary>
    /// Make a request to the API to update the ingest state.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.IngestModel?> UpdateIngestStateAsync(API.Areas.Services.Models.Ingest.IngestModel ingest);

    /// <summary>
    /// Make a request to the API to update the ingest configuration.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Ingest.IngestModel?> UpdateIngestConfigAsync(API.Areas.Services.Models.Ingest.IngestModel ingest);
    #endregion

    #region Contents
    /// <summary>
    /// Make a request to the API to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> FindContentReferenceAsync(string source, string uid);

    /// <summary>
    /// Make a request to the API to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> AddContentReferenceAsync(API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference);

    /// <summary>
    /// Make a request to the API to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> UpdateContentReferenceAsync(API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference);

    /// <summary>
    /// Make a request to the API to update the specified content reference with Kafka information.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> UpdateContentReferenceKafkaAsync(API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference);

    /// <summary>
    /// Make a request to the API to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Content.ContentModel?> AddContentAsync(API.Areas.Services.Models.Content.ContentModel content, int? requestorId = null);

    /// <summary>
    /// Make a request to the API to update the content for the specified ContentModel.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Content.ContentModel?> UpdateContentAsync(API.Areas.Services.Models.Content.ContentModel content, bool index = false, int? requestorId = null);

    /// <summary>
    /// Make a request to the API to update the content status for the specified ContentModel.
    /// Will not trigger any re-index or audit trail update
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Content.ContentModel?> UpdateContentStatusAsync(API.Areas.Services.Models.Content.ContentModel content);

    /// <summary>
    /// Make a request to the API to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Content.ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName);

    /// <summary>
    /// Make a request to the API to update the file for the specified ContentModel.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public Task<API.Areas.Services.Models.Content.ContentModel?> UpdateFileAsync(API.Areas.Services.Models.Content.ContentModel content, bool index = false, int? requestorId = null);

    /// <summary>
    /// Make a request to the API to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Content.ContentModel?> FindContentByUidAsync(string uid, string? source);

    /// <summary>
    /// Make a request to the API to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Content.ContentModel?> FindContentByIdAsync(long id);

    /// <summary>
    /// Make a request to the API to get all notification instances for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    Task<IEnumerable<API.Areas.Services.Models.Content.NotificationInstanceModel>> GetNotificationsForAsync(long contentId);

    /// <summary>
    /// Make an HTTP request to the api to get the specified image content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<string?> GetImageFile(long id);
    #endregion

    #region Actions
    /// <summary>
    /// Update the content action.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Content.ContentActionModel?> UpdateContentActionAsync(API.Areas.Services.Models.Content.ContentActionModel action);
    #endregion

    #region Work Orders
    /// <summary>
    /// Make a request to the API and find the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> FindWorkOrderAsync(long id);

    /// <summary>
    /// Make a request to the API and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> UpdateWorkOrderAsync(API.Areas.Services.Models.WorkOrder.WorkOrderModel workOrder);
    #endregion

    #region Users
    /// <summary>
    /// Make a request to the API to fetch the user with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.User.UserModel?> GetUserAsync(int id);
    #endregion

    #region Notifications
    /// <summary>
    /// Make a request to the API to fetch all the notifications.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<API.Areas.Services.Models.Notification.NotificationModel>> GetAllNotificationsAsync();

    /// <summary>
    /// Make a request to the API to fetch the notification with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Notification.NotificationModel?> GetNotificationAsync(int id);

    /// <summary>
    /// Make a request to the API and add a new notification instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel?> AddNotificationInstanceAsync(API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel instance);
    #endregion

    #region Reports
    /// <summary>
    /// Make a request to the API to fetch the report with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Report.ReportModel?> GetReportAsync(int id);

    /// <summary>
    /// Make a request to the API to fetch the content for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentForReportIdAsync(int id);

    /// <summary>
    /// Make a request to the API to fetch the report instance with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> GetReportInstanceAsync(long id);

    /// <summary>
    /// Make a request to the API to fetch the content for the specified report instance 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<IEnumerable<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>> GetContentForReportInstanceIdAsync(long id);

    /// <summary>
    /// Make a request to the API and add a new notification instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> AddReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance);

    /// <summary>
    /// Make a request to the API and update a report instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> UpdateReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance);

    /// <summary>
    /// Make a request to the API to clear all content from folders in this report.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.Report.ReportModel?> ClearFoldersInReport(int reportId);
    #endregion

    #region Event Schedules
    /// <summary>
    /// Make a request to the API to fetch all event schedules.
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<API.Areas.Services.Models.EventSchedule.EventScheduleModel>> GetEventSchedulesAsync();

    /// <summary>
    /// Make a request to the API for the event schedule for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.EventSchedule.EventScheduleModel?> GetEventScheduleAsync(int id);

    /// <summary>
    /// Make a request to the API to update the event schedule for the specified 'model'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.EventSchedule.EventScheduleModel?> UpdateEventScheduleAsync(API.Areas.Services.Models.EventSchedule.EventScheduleModel model);
    #endregion

    #region AV Overview
    /// <summary>
    /// Make a request to the API for the evening overview instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?> GetAVOverviewInstanceAsync(int id);

    /// <summary>
    /// Make a request to the API to update the evening overview instance for the specified 'model'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?> UpdateAVOverviewInstanceAsync(API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel model);
    #endregion

    #region Folders
    /// <summary>
    /// Removes the specified content from all folders.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> RemoveContentFromFoldersAsync(long contentId);

    /// <summary>
    /// Get all folders with enabled filters
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<API.Areas.Services.Models.Folder.FolderModel>?> GetFoldersWithFiltersAsync();

    /// <summary>
    /// Add the specified content to the specified folder.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="folderId"></param>
    /// <param name="toBottom"></param>
    /// <returns></returns>
    Task<HttpResponseMessage> AddContentToFolderAsync(long contentId, int folderId, bool toBottom = true);
    #endregion
}
