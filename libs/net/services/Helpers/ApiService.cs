using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FTTLib;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Services.Config;

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
    public IOpenIdConnectRequestClient OpenClient { get; private set; }

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
        this.OpenClient = client;
        client.Client.Timeout = new TimeSpan(0, 0, 0, 0, options.Value.HttpRequestTimeoutMs);
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
            this.Logger.LogError(ex, "Retry attempt {count}.{newline}Error:{body}", this.FailureCount, Environment.NewLine, ex.Data["Body"]);
            await Task.Delay(this.Options.RetryDelayMS);
            return await RetryRequestAsync<T>(callbackDelegate);
        }
    }

    /// <summary>
    /// Keep trying a request if the failure is caused by an optimistic concurrency error.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="callbackDelegate"></param>
    /// <returns></returns>
    public async Task<T> HandleConcurrencyAsync<T>(Func<Task<T>> callbackDelegate)
    {
        // Keep trying to update the record and handle concurrency errors.
        while (true)
        {
            try
            {
                return await callbackDelegate();
            }
            catch (HttpClientRequestException ex)
            {
                // If it's a concurrency error, keep trying.  Otherwise throw the error.
                this.Logger.LogError(ex, "Failed to complete request.  Determining if this is a concurrency error.");
                var data = ex.Data["Body"] as string;
                if (!String.IsNullOrWhiteSpace(data))
                {
                    var json = JsonSerializer.Deserialize<API.Models.ErrorResponseModel>(data, _serializerOptions);
                    if (json != null && json.Type == nameof(Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException))
                    {
                        // A concurrency error can be resolved by loading the latest and reapplying the values.
                        continue;
                    }
                }
                // It wasn't a concurrency error, throw as a real failure.
                throw;
            }
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
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.SourceContent>>(url, JsonContent.Create(content)));
    }

    /// <summary>
    /// Publish notification request to Kafka.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.NotificationRequestModel>?> SendMessageAsync(TNO.Kafka.Models.NotificationRequestModel request)
    {
        var url = this.Options.ApiUrl.Append($"kafka/producers/notification");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.NotificationRequestModel>>(url, JsonContent.Create(request)));
    }

    /// <summary>
    /// Publish report request to Kafka.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.ReportRequestModel>?> SendMessageAsync(TNO.Kafka.Models.ReportRequestModel request)
    {
        var url = this.Options.ApiUrl.Append($"kafka/producers/report");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.ReportRequestModel>>(url, JsonContent.Create(request)));
    }

    /// <summary>
    /// Publish event schedule request to Kafka.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.EventScheduleRequestModel>?> SendMessageAsync(TNO.Kafka.Models.EventScheduleRequestModel request)
    {
        var url = this.Options.ApiUrl.Append($"kafka/producers/event");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Kafka.Models.DeliveryResultModel<TNO.Kafka.Models.EventScheduleRequestModel>>(url, JsonContent.Create(request)));
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
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Editor.Models.Lookup.LookupModel>(url));
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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        var result = await RetryRequestAsync(async () => await this.OpenClient.GetAsync<TNO.API.Areas.Services.Models.Ingest.SourceModel[]>(url));
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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        var result = await RetryRequestAsync(async () => await this.OpenClient.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
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
        var result = await RetryRequestAsync(async () => await this.OpenClient.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
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
        var result = await RetryRequestAsync(async () => await this.OpenClient.GetAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel[]>(url));
        return result ?? Array.Empty<TNO.API.Areas.Services.Models.Ingest.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to update the ingest state.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public async Task<TNO.API.Areas.Services.Models.Ingest.IngestModel?> UpdateIngestStateAsync(
        TNO.API.Areas.Services.Models.Ingest.IngestModel ingest)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/{ingest.Id}/state");
        var jsonString = JsonSerializer.Serialize(ingest);
        return await RetryRequestAsync(async () => await this.OpenClient.SendAsync<TNO.API.Areas.Services.Models.Ingest.IngestModel>(url, HttpMethod.Put, JsonContent.Create(ingest)));
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
        return await RetryRequestAsync(async () => await this.OpenClient.DeleteAsync<TNO.API.Areas.Services.Models.Ingest.ScheduleModel>(url, JsonContent.Create(schedule.Schedule)));
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
        var response = await RetryRequestAsync(async () => await this.OpenClient.GetAsync(url));

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
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(url, content));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ContentReference.ContentReferenceModel?> UpdateContentReferenceAsync(
        API.Areas.Services.Models.ContentReference.ContentReferenceModel contentReference)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{contentReference.Source}?uid={contentReference.Uid}");
        var content = JsonContent.Create(contentReference);
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.ContentReference.ContentReferenceModel>(url, content));
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
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Content.ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeUserNotifications"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> FindContentByIdAsync(long id, bool includeUserNotifications = false)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{id}{(includeUserNotifications ? "?includeUserNotifications=true" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Content.ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to get the specified image content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<string?> GetImageFile(long id)
    {
        var url = Options.ApiUrl.Append($"services/contents/{id}/image");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<string>(url,
            (response) =>
            {
                var isBadRequest = response.StatusCode == HttpStatusCode.BadRequest;
                if (isBadRequest) Logger.LogWarning("Unable to get the image file (id: {id}).", id);
                return isBadRequest;
            }));
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
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.Content.ContentModel>(url, JsonContent.Create(content)));
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
        bool index = false,
        int? requestorId = null)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{content.Id}?index={index}{(requestorId.HasValue ? $"&requestorId={requestorId.Value}" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.Content.ContentModel>(url, JsonContent.Create(content)));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the content status for the specified 'id'.
    /// Will not trigger any re-index or audit trail update
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="newStatus"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> UpdateContentStatusAsync(
        API.Areas.Services.Models.Content.ContentModel content)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{content.Id}/status");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.Content.ContentModel>(url, JsonContent.Create(content)));
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
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(FTT.GetMimeType(Path.GetFileName(fileName)));
        var form = new MultipartFormDataContent
        {
            { fileContent, "files", fileName }
        };
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.Content.ContentModel>(url, form));
    }

    /// <summary>
    /// Make a request to the API to update the file for the specified ContentModel.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="index">Be careful this can result in a indexing loop.</param>
    /// <param name="requestorId">The user ID who is requesting the update.</param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> UpdateFileAsync(API.Areas.Services.Models.Content.ContentModel content, bool index = false, int? requestorId = null)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{content.Id}/file?index={index}{(requestorId.HasValue ? $"&requestorId={requestorId.Value}" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.Content.ContentModel>(url, JsonContent.Create(content)));
    }

    /// <summary>
    /// Make a request to the API to get all notification instances for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Content.NotificationInstanceModel>> GetNotificationsForAsync(long contentId)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{contentId}/notifications");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.Content.NotificationInstanceModel>>(url)) ??
            Array.Empty<API.Areas.Services.Models.Content.NotificationInstanceModel>();
    }
    #endregion

    #region Actions
    /// <summary>
    /// Update the content action.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentActionModel?> UpdateContentActionAsync(API.Areas.Services.Models.Content.ContentActionModel action)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{action.ContentId}/actions/{action.Id}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.Content.ContentActionModel>(url, JsonContent.Create(action)));
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
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.WorkOrder.WorkOrderModel>(url));
    }

    /// <summary>
    /// Make a request to the API and find the work orders for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.WorkOrder.WorkOrderModel>> FindWorkOrderForContentIdAsync(long contentId)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/content/{contentId}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.WorkOrder.WorkOrderModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.WorkOrder.WorkOrderModel>();
    }

    /// <summary>
    /// Make a request to the API and add the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> AddWorkOrderAsync(
        API.Areas.Services.Models.WorkOrder.WorkOrderModel workOrder)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.WorkOrder.WorkOrderModel>(url, JsonContent.Create(workOrder)));
    }

    /// <summary>
    /// Make an HTTP request to the aip and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> UpdateWorkOrderAsync(
        API.Areas.Services.Models.WorkOrder.WorkOrderModel workOrder)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/{workOrder.Id}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.WorkOrder.WorkOrderModel>(url, JsonContent.Create(workOrder)));
    }
    #endregion

    #region Notifications
    /// <summary>
    /// Make a request to the API to fetch all the notifications for the specified filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Notification.NotificationModel>> FindNotificationsAsync(TNO.Models.Filters.NotificationFilter filter)
    {
        var url = this.Options.ApiUrl.Append($"services/notifications/find");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<IEnumerable<API.Areas.Services.Models.Notification.NotificationModel>>(url, JsonContent.Create(filter))) ?? Array.Empty<API.Areas.Services.Models.Notification.NotificationModel>();
    }

    /// <summary>
    /// Make a request to the API to fetch the notification with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Notification.NotificationModel?> GetNotificationAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/notifications/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Notification.NotificationModel?>(url));
    }

    /// <summary>
    /// Make a request to the API and add a new notification instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel?> AddNotificationInstanceAsync(API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/notification/instances");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel>(url, JsonContent.Create(instance)));
    }


    /// <summary>
    /// Make a request to the API to fetch the content for the specified notification 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    public async Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>?> FindContentForNotificationIdAsync(int id, int? requestorId)
    {
        var url = this.Options.ApiUrl.Append($"services/notifications/{id}/content{(requestorId != null ? $"?requestorId={requestorId}" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>(url));
    }
    #endregion

    #region Reports
    /// <summary>
    /// Make a request to the API to fetch the report with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Report.ReportModel?> GetReportAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Report.ReportModel?>(url));
    }

    /// <summary>
    /// Make a request to the API to fetch the content for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>> FindContentForReportIdAsync(int id, int? requestorId)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/{id}/content{(requestorId != null ? $"?requestorId={requestorId}" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>>(url)) ?? new Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>();
    }

    /// <summary>
    /// Make a request to the API to clear all content from folders in this report.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Report.ReportModel?> ClearFoldersInReport(int reportId)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/{reportId}/clear/folders");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.Report.ReportModel>(url));
    }

    /// <summary>
    /// Get the current instance for the specified report 'reportId'.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Report.ReportInstanceModel?> GetCurrentReportInstanceAsync(int reportId, int? ownerId)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/{reportId}/instance{(ownerId.HasValue ? $"?ownerId={ownerId}" : "")}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Report.ReportInstanceModel?>(url));
    }

    /// <summary>
    /// Make a request to the API to fetch the report instance with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> GetReportInstanceAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?>(url));
    }

    /// <summary>
    /// Make a request to the API to fetch the content for the specified report instance 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>> GetContentForReportInstanceIdAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/{id}/content");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>>(url))
            ?? Array.Empty<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>();
    }

    /// <summary>
    /// Make a request to the API and add a new report instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> AddReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(url, JsonContent.Create(instance)));
    }

    /// <summary>
    /// Make a request to the API and update a report instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="updateContent"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> UpdateReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance, bool updateContent)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/{instance.Id}?updateContent={updateContent}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.ReportInstance.ReportInstanceModel>(url, JsonContent.Create(instance)));
    }

    /// <summary>
    /// Get user report instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>> GetUserReportInstancesAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/{id}/responses");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>>(url))
            ?? Array.Empty<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
    }

    /// <summary>
    /// Add or update user report instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel?> AddOrUpdateUserReportInstanceAsync(API.Areas.Services.Models.ReportInstance.UserReportInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/response");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>(url, JsonContent.Create(instance)));
    }

    /// <summary>
    /// Add or update user report instances.
    /// </summary>
    /// <param name="instances"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>> AddOrUpdateUserReportInstancesAsync(IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel> instances)
    {
        var url = this.Options.ApiUrl.Append($"services/report/instances/responses");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>>(url, JsonContent.Create(instances))) ?? Array.Empty<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
    }
    #endregion

    #region Users
    /// <summary>
    /// Make a request to the API to fetch the user with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.User.UserModel?> GetUserAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/users/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.User.UserModel?>(url));
    }


    /// <summary>
    /// Make a request to the API to fetch all the users in a distribution list for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.User.UserModel>> GetDistributionListAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/users/{id}/distribution");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.User.UserModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.User.UserModel>();
    }
    #endregion

    #region Ministers
    /// <summary>
    /// Make a request to the API to fetch the list of all Ministers.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Minister.MinisterModel>> GetMinistersAsync()
    {
        var url = this.Options.ApiUrl.Append($"editor/ministers");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.Minister.MinisterModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.Minister.MinisterModel>();
    }

    #endregion

    #region Event Schedules
    /// <summary>
    /// Make a request to the API to fetch all event schedules.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.EventSchedule.EventScheduleModel>> GetEventSchedulesAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/events/schedules");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.EventSchedule.EventScheduleModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.EventSchedule.EventScheduleModel>();
    }

    /// <summary>
    /// Make a request to the API for the event schedule for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.EventSchedule.EventScheduleModel?> GetEventScheduleAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/events/schedules/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.EventSchedule.EventScheduleModel?>(url));
    }

    /// <summary>
    /// Make a request to the API to update the event schedule for the specified 'model'.
    /// The most common issue with this endpoint is concurrency errors.  Retrying won't fix that, so use the HandleConcurrency function and set retry = false.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="retry"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.EventSchedule.EventScheduleModel?> UpdateEventScheduleAsync(API.Areas.Services.Models.EventSchedule.EventScheduleModel model, bool retry = true)
    {
        var url = this.Options.ApiUrl.Append($"services/events/schedules/{model.Id}");

        if (retry)
            return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.EventSchedule.EventScheduleModel?>(url, JsonContent.Create(model)));
        else
            return await this.OpenClient.PutAsync<API.Areas.Services.Models.EventSchedule.EventScheduleModel?>(url, JsonContent.Create(model));
    }
    #endregion

    #region AV Overview
    /// <summary>
    /// Make a request to the API for the evening overview instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?> GetAVOverviewInstanceAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/av/overviews/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?>(url));
    }

    /// <summary>
    /// Make a request to the API to update the evening overview instance for the specified 'model'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?> UpdateAVOverviewInstanceAsync(API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel model)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/av/overviews/{model.Id}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync<API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel?>(url, JsonContent.Create(model)));
    }

    /// <summary>
    /// Get user report instance for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>> GetUserAVOverviewInstancesAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/av/overviews/{id}/responses");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>>(url))
            ?? Array.Empty<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>();
    }

    /// <summary>
    /// Add or update user report instance.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel?> AddOrUpdateUserAVOverviewInstanceAsync(API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel instance)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/av/overviews/response");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>(url, JsonContent.Create(instance)));
    }

    /// <summary>
    /// Add or update user report instances.
    /// </summary>
    /// <param name="instances"></param>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>> AddOrUpdateUserAVOverviewInstancesAsync(IEnumerable<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel> instances)
    {
        var url = this.Options.ApiUrl.Append($"services/reports/av/overviews/responses");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<IEnumerable<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>>(url, JsonContent.Create(instances))) ?? Array.Empty<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>();
    }
    #endregion

    #region Folders
    /// <summary>
    /// Get the folder with the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Folder.FolderModel?> GetFolderAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/folders/{id}");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<API.Areas.Services.Models.Folder.FolderModel?>(url));
    }

    /// <summary>
    /// Removes the specified content from all folders.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> RemoveContentFromFoldersAsync(long contentId)
    {
        var url = this.Options.ApiUrl.Append($"services/folders/content/{contentId}");
        return await RetryRequestAsync(async () => await this.OpenClient.DeleteAsync(url));
    }

    /// <summary>
    /// Removes the content from the folder based on the folder configuration settings.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> RemoveContentFromFolder(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/folders/{id}/clean");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync(url));
    }

    /// <summary>
    /// Get all folders with enabled filters
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Folder.FolderModel>?> GetFoldersWithFiltersAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/folders/with-filters");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.Folder.FolderModel>?>(url));
    }

    /// <summary>
    /// Add the specified content to the specified folder.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="folderId"></param>
    /// <param name="toBottom"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> AddContentToFolderAsync(long contentId, int folderId, bool toBottom = true)
    {
        var url = this.Options.ApiUrl.Append($"services/folders/{folderId}/contents/{contentId}?bottom={toBottom}");
        return await RetryRequestAsync(async () => await this.OpenClient.PutAsync(url));
    }
    #endregion

    #region Quotes
    /// <summary>
    /// Add the quotes to the specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="quotes"></param>
    /// <returns></returns>
    public async Task<API.Areas.Services.Models.Content.ContentModel?> AddQuotesToContentAsync(long contentId, IEnumerable<API.Areas.Services.Models.Content.QuoteModel> quotes)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{contentId}/quotes");
        return await RetryRequestAsync(async () => await this.OpenClient.PostAsync<API.Areas.Services.Models.Content.ContentModel?>(url, JsonContent.Create(quotes)));
    }
    #endregion

    #region Settings
    /// <summary>
    /// Get all of the settings
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<API.Areas.Services.Models.Setting.SettingModel>> GetSettings()
    {
        var url = this.Options.ApiUrl.Append($"services/settings");
        return await RetryRequestAsync(async () => await this.OpenClient.GetAsync<IEnumerable<API.Areas.Services.Models.Setting.SettingModel>>(url)) ?? Array.Empty<API.Areas.Services.Models.Setting.SettingModel>();
    }
    #endregion

    #endregion
}
