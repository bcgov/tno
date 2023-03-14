using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.ContentReference;
using DataLocationModels = TNO.API.Areas.Services.Models.DataLocation;
using IngestModels = TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.API.Areas.Kafka.Models;
using System.Net.Http.Headers;

namespace TNO.Services;

/// <summary>
/// IApiService interface, provides a way to interact with the API.
/// </summary>
public interface IApiService
{
    #region Helper Methods
    public Task<T> HandleRequestFailure<T>(Func<Task<T>> callbackDelegate, bool ignoreError, T defaultResponse);
    #endregion

    #region Lookups
    /// <summary>
    /// Make an AJAX request to the api to get the lookups.
    /// </summary>
    /// <returns></returns>
    public Task<LookupModel?> GetLookupsAsync();
    #endregion

    #region Sources
    /// <summary>
    /// Make an AJAX request to the api to fetch all sources.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<IngestModels.SourceModel>> GetSourcesAsync();

    /// <summary>
    /// Make an AJAX request to the api to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Task<IngestModels.SourceModel?> GetSourceForCodeAsync(string code);
    #endregion

    #region Connections
    /// <summary>
    /// Make an AJAX request to the api to get the connection for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<IngestModels.ConnectionModel?> GetConnectionAsync(int id);
    #endregion

    #region Data Locations
    /// <summary>
    /// Make an AJAX request to the api to get the data location for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<DataLocationModels.DataLocationModel?> GetDataLocationAsync(int id);

    /// <summary>
    /// Make an AJAX request to the api to get the data location for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Task<DataLocationModels.DataLocationModel?> GetDataLocationAsync(string name);
    #endregion

    #region Ingests
    /// <summary>
    /// Make an AJAX request to the api to fetch the ingest for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<IngestModels.IngestModel?> GetIngestAsync(int id);

    /// <summary>
    /// Make an AJAX request to the api to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<IngestModels.IngestModel>> GetIngestsAsync();

    /// <summary>
    /// Make an AJAX request to the api to fetch ingests for the specified ingest type.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    public Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForIngestTypeAsync(string ingestType);

    /// <summary>
    /// Make an AJAX request to the api to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForTopicAsync(string topic);
    #endregion

    #region Ingest Schedules
    /// <summary>
    /// Delete the specified 'schedule' from the ingests.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public Task<IngestModels.ScheduleModel?> DeleteIngestSchedule(IngestModels.IngestScheduleModel schedule);
    #endregion

    #region Contents
    /// <summary>
    /// Make an AJAX request to the api to update the content for the specified ContentModel.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public Task<ContentModel?> UpdateContentAsync(ContentModel content, HttpRequestHeaders? headers);

    /// <summary>
    /// Make an AJAX request to the api to update the ingest.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public Task<IngestModels.IngestModel?> UpdateIngestAsync(IngestModels.IngestModel ingest, HttpRequestHeaders? headers);

    /// <summary>
    /// Make an AJAX request to the api to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> FindContentReferenceAsync(string source, string uid);

    /// <summary>
    /// Make an AJAX request to the api to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> AddContentReferenceAsync(ContentReferenceModel contentReference);

    /// <summary>
    /// Make an AJAX request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel contentReference, HttpRequestHeaders? headers);

    /// <summary>
    /// Make an AJAX request to the api to update the specified content reference with Kafka information.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <param name="headers"></param>
    /// <returns></returns>
    public Task<ContentReferenceModel?> UpdateContentReferenceKafkaAsync(ContentReferenceModel contentReference, HttpRequestHeaders? headers);

    /// <summary>
    /// Make an AJAX request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<ContentModel?> AddContentAsync(ContentModel content);

    /// <summary>
    /// Make an AJAX request to the api to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task<ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName);

    /// <summary>
    /// Make an AJAX request to the api to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    Task<ContentModel?> FindContentByUidAsync(string uid, string? source);

    /// <summary>
    /// Make an AJAX request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ContentModel?> FindContentByIdAsync(long id);
    #endregion

    #region Work Orders
    /// <summary>
    /// Make an AJAX request to the aip and find the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<WorkOrderModel?> FindWorkOrderAsync(long id);

    /// <summary>
    /// Make an AJAX request to the aip and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="headers"></param>  
    /// <returns></returns> 
    Task<WorkOrderModel?> UpdateWorkOrderAsync(WorkOrderModel workOrder, HttpRequestHeaders? headers);
    #endregion

    #region Kafka
    /// <summary>
    /// Publish content to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    Task<DeliveryResultModel<TNO.Kafka.Models.SourceContent>?> SendMessageAsync(string topic, TNO.Kafka.Models.SourceContent content);
    #endregion
}
