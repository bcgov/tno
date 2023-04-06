using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeTypes;
using TNO.Core.Exceptions;
using TNO.Services.Config;
using TNO.Core.Extensions;
using TNO.Core.Http;
using Microsoft.Extensions.Logging;

namespace TNO.Services;

/// <summary>
/// ApiService class, provides a way to interact with the API.
/// </summary>
public class ApiService : IApiService
{
    #region Variables
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Properties
    /// <summary>
    /// get - Number of sequential failures.
    /// </summary>
    public int FailureCount { get; private set; }

    /// <summary>
    /// get - Service configuration options.
    /// </summary>
    protected ServiceOptions Options { get; private set; }

    /// <summary>
    /// get - HTTP client to make HTTP requests.
    /// </summary>
    protected IOpenIdConnectRequestClient Client { get; private set; }

    /// <summary>
    /// get - The logger.
    /// </summary>
    protected ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ApiService(IOpenIdConnectRequestClient client, IOptions<JsonSerializerOptions> serializerOptions, IOptions<ServiceOptions> options, ILogger<IApiService> logger)
    {
        _serializerOptions = serializerOptions.Value;
        this.Client = client;
        this.Options = options.Value;
        this.Logger = logger;
    }
    #endregion

    #region Methods
    #region Helper Methods
    /// <summary>
    /// Depending on configuration the function will either throw the exception if it occurs, or it will return the specified 'defaultResponse'.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callbackDelegate"></param>
    /// <param name="ignoreError"></param>
    /// <param name="defaultResponse"></param>
    /// <returns></returns>
    public async Task<T> HandleRequestFailure<T>(Func<Task<T>> callbackDelegate, bool ignoreError, T defaultResponse)
    {
        try
        {
            return await callbackDelegate();
        }
        catch (Exception ex)
        {
            // If configured to reuse existing ingests it will ignore the error and continue running.
            if (!ignoreError)
                throw;

            this.Logger.LogError(ex, "Ignoring error and reusing existing ingests");
            return defaultResponse;
        }
    }

    /// <summary>
    /// Self referencing retry method that will log the error and retry the configured number of attempts.
    /// Delays the configured number of milliseconds before retrying.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="request"></param>
    /// <returns></returns>
    protected async Task<T> RetryRequestAsync<T>(Func<Task<T>> callbackDelegate)
    {
        try
        {
            var response = await callbackDelegate();
            this.FailureCount = 0;
            return response;
        }
        catch (Exception ex)
        {
            if (this.Options.RetryLimit <= ++this.FailureCount)
            {
                this.FailureCount = 0;
                throw;
            }

            // Wait before retrying.
            this.Logger.LogError(ex, "Retry attempt {count}", this.FailureCount);
            await Task.Delay(this.Options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }
    #endregion

    #region Kafka Methods
    /// <summary>
    /// Publish content to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.SourceContent>?> SendMessageAsync(string topic, TNO.Kafka.Models.SourceContent content)
    {
        var url = this.Options.ApiUrl.Append($"kafka/producers/content/{topic}");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.SourceContent>>(url, JsonContent.Create(content)));
    }
    #endregion

    #region Lookup Methods
    /// <summary>
    /// Make an HTTP request to the api to get the lookups.
    /// </summary>
    /// <returns></returns>
    public async Task<API.Areas.Editor.Models.Lookup.LookupModel?> GetLookupsAsync()
    {
        var url = this.Options.ApiUrl.Append($"editor/lookups");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Editor.Models.Lookup.LookupModel>(url));
    }
    #endregion

    #region Data Location Methods
    /// <summary>
    /// Make an HTTP request to the api to get the data location for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.DataLocation.DataLocationModel?> GetDataLocationAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/data/locations/{id}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TNO.API.Areas.Services.Models.DataLocation.DataLocationModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an HTTP request to the api to get the data location for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.DataLocation.DataLocationModel?> GetDataLocationAsync(string name)
    {
        var url = this.Options.ApiUrl.Append($"services/data/locations/{name}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TNO.API.Areas.Services.Models.DataLocation.DataLocationModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }
    #endregion

    #region Connection Methods
    /// <summary>
    /// Make an HTTP request to the api to get the connection.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.ConnectionModel?> GetConnectionAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/connections/{id}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TNO.API.Areas.Services.Models.Ingest.ConnectionModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }
    #endregion

    #region Source Methods
    /// <summary>
    /// Make an HTTP request to the api to fetch all sources.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TNO.API.Areas.Services.Models.Ingest.SourceModel>> GetSourcesAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/sources");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<TNO.API.Areas.Services.Models.Ingest.SourceModel[]>(url));
        return result ?? Array.Empty<TNO.API.Areas.Services.Models.Ingest.SourceModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.SourceModel?> GetSourceForCodeAsync(string code)
    {
        var url = this.Options.ApiUrl.Append($"services/sources/{code}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TNO.API.Areas.Services.Models.Ingest.SourceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }
    #endregion

    #region Ingest Methods
    /// <summary>
    /// Make an HTTP request to the api to fetch the ingest for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.IngestModel?> GetIngestAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/{id}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<TNO.API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/ingests");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
        return result ?? Array.Empty<TNO.API.Areas.Services.Models.Ingest.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch ingests for the specified ingest type.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TNO.API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsForIngestTypeAsync(string ingestType)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/for/ingest/type/{ingestType}");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
        return result ?? Array.Empty<TNO.API.Areas.Services.Models.Ingest.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task<IEnumerable<TNO.API.Areas.Services.Models.Ingest.IngestModel>> GetIngestsForTopicAsync(string topic)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/for/topic/{topic}");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
        return result ?? Array.Empty<TNO.API.Areas.Services.Models.Ingest.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to update the ingest.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.IngestModel?> UpdateIngestAsync(
        TNO.API.Areas.Services.Models.Ingest.IngestModel ingest,
        HttpRequestHeaders? headers = null)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/{ingest.Id}");
        return await RetryRequestAsync(async () => await Client.SendAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel>(url, HttpMethod.Put, headers, JsonContent.Create(ingest)));
    }
    #endregion

    #region Ingest Schedule Methods
    /// <summary>
    /// Delete specified 'schedule' from ingests.
    /// </summary>
    /// <param name="schedule"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.ScheduleModel?> DeleteIngestSchedule(TNO.API.Areas.Services.Models.Ingest.IngestScheduleModel schedule)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/schedules/{schedule.IngestId}");
        return await RetryRequestAsync(async () => await this.Client.DeleteAsync<TNO.API.Areas.Services.Models.Ingest.ScheduleModel>(url, JsonContent.Create(schedule.Schedule)));
    }
    #endregion

    #region Content Reference Methods
    /// <summary>
    /// Make an HTTP request to the api to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> FindContentReferenceAsync(string source, string uid)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{source}?uid={uid}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an HTTP request to the api to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> AddContentReferenceAsync(API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references");
        var content = JsonContent.Create(contentReference);
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(url, content));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> UpdateContentReferenceAsync(
        API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference,
        HttpRequestHeaders? headers = null)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{contentReference.Source}?uid={contentReference.Uid}");
        var content = JsonContent.Create(contentReference);
        return await RetryRequestAsync(async () => await Client.SendAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(url, HttpMethod.Put, headers, content));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the specified content reference with Kafka information.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> UpdateContentReferenceKafkaAsync(
        API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference,
        HttpRequestHeaders? headers = null)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{contentReference.Source}/kafka?uid={contentReference.Uid}");
        var content = JsonContent.Create(contentReference);
        return await RetryRequestAsync(async () => await Client.SendAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(url, HttpMethod.Put, headers, content));
    }
    #endregion

    #region Content Methods
    /// <summary>
    /// Make an HTTP request to the api to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> FindContentByUidAsync(string uid, string? source)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/find?uid={uid}&source={source}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.Content.ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> FindContentByIdAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.Content.ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> AddContentAsync(
        API.Areas.Services.Models.Content.ContentModel content,
        int? requestorId = null)
    {
        var url = this.Options.ApiUrl.Append($"services/contents{(requestorId.HasValue ? $"?requestorId={requestorId.Value}" : "")}");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Services.Models.Content.ContentModel>(url, JsonContent.Create(content)));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the content for the specified 'id'.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> UpdateContentAsync(
        API.Areas.Services.Models.Content.ContentModel content,
        HttpRequestHeaders? headers = null,
        bool index = false,
        int? requestorId = null)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{content.Id}?index={index}{(requestorId.HasValue ? $"&requestorId={requestorId.Value}" : "")}");
        return await RetryRequestAsync(async () => await Client.SendAsync<API.Areas.Services.Models.Content.ContentModel>(url, HttpMethod.Put, headers, JsonContent.Create(content)));
    }

    /// <summary>
    /// Make an HTTP request to the api to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{contentId}/upload?version={version}");
        var fileContent = new StreamContent(file);
        var ext = Path.GetExtension(fileName).Replace(".", "");
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(ext));
        var form = new MultipartFormDataContent
        {
            { fileContent, "files", fileName }
        };
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Services.Models.Content.ContentModel>(url, form));
    }

    /// <summary>
    /// Make a request to the API to get all notification instances for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Content.NotificationInstanceModel>> GetNotificationsForAsync(long contentId)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{contentId}/notifications");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<IEnumerable<API.Areas.Services.Models.Content.NotificationInstanceModel>>(url)) ??
            Array.Empty<API.Areas.Services.Models.Content.NotificationInstanceModel>();
    }
    #endregion

    #region Work Order Methods
    /// <summary>
    /// Make an HTTP request to the api to get the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> FindWorkOrderAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.WorkOrder.WorkOrderModel>(url));
    }

    /// <summary>
    /// Make an HTTP request to the aip and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> UpdateWorkOrderAsync(
        API.Areas.Services.Models.WorkOrder.WorkOrderModel workOrder,
        HttpRequestHeaders? headers = null)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/{workOrder.Id}");
        return await RetryRequestAsync(async () => await Client.SendAsync<API.Areas.Services.Models.WorkOrder.WorkOrderModel>(url, HttpMethod.Put, headers, JsonContent.Create(workOrder)));
    }
    #endregion

    #region Notifications
    /// <summary>
    /// Make a request to the API to fetch all the notifications.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Notification.NotificationModel>> GetAllNotificationsAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/notifications");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<IEnumerable<API.Areas.Services.Models.Notification.NotificationModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.Notification.NotificationModel>();
    }

    /// <summary>
    /// Make a request to the API to fetch the notification with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Notification.NotificationModel?> GetNotificationAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/notifications/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.Notification.NotificationModel?>(url));
    }

    /// <summary>
    /// Make a request to the API and add a new notification instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel?> AddNotificationInstanceAsync(API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/notification/instances");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel>(url, JsonContent.Create(instance)));
    }
    #endregion

    #region Reports
    /// <summary>
    /// Make a request to the API to fetch all the notifications.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Report.ReportModel>> GetAllReportsAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/reports");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<IEnumerable<API.Areas.Services.Models.Report.ReportModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.Report.ReportModel>();
    }

    /// <summary>
    /// Make a request to the API to fetch the report with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Report.ReportModel?> GetReportAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.Report.ReportModel?>(url));
    }

    /// <summary>
    /// Make a request to the API and add a new notification instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> AddReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(url, JsonContent.Create(instance)));
    }
    #endregion

    #region Reports
    /// <summary>
    /// Make a request to the API to fetch the user with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.User.UserModel?> GetUserAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/users/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<API.Areas.Services.Models.User.UserModel?>(url));
    }
    #endregion
    #endregion
}
