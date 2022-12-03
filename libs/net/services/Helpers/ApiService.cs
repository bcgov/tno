using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MimeTypes;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.ContentReference;
using DataLocationModels = TNO.API.Areas.Services.Models.DataLocation;
using IngestModels = TNO.API.Areas.Services.Models.Ingest;
using TNO.API.Areas.Services.Models.WorkOrder;
using TNO.Core.Exceptions;
using TNO.Kafka.Models;
using TNO.Services.Config;
using TNO.API.Areas.Kafka.Models;
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
            if (this.Options.RetryLimit >= ++this.FailureCount)
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

    #region Data Location Methods
    /// <summary>
    /// Make an HTTP request to the api to get the data location for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<DataLocationModels.DataLocationModel?> GetDataLocationAsync(string name)
    {
        var url = this.Options.ApiUrl.Append($"services/data/locations/{name}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<DataLocationModels.DataLocationModel>(_serializerOptions),
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
    public async Task<IngestModels.ConnectionModel?> GetConnectionAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/connections/{id}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<IngestModels.ConnectionModel>(_serializerOptions),
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
    public async Task<IEnumerable<IngestModels.SourceModel>> GetSourcesAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/sources");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<IngestModels.SourceModel[]>(url));
        return result ?? Array.Empty<IngestModels.SourceModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<IngestModels.SourceModel?> GetSourceForCodeAsync(string code)
    {
        var url = this.Options.ApiUrl.Append($"services/sources/{code}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<IngestModels.SourceModel>(_serializerOptions),
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
    public async Task<IngestModels.IngestModel?> GetIngestAsync(int id)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/{id}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<IngestModels.IngestModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsAsync()
    {
        var url = this.Options.ApiUrl.Append($"services/ingests");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<IngestModels.IngestModel[]>(url));
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch ingests for the specified ingest type.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForIngestTypeAsync(string ingestType)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/for/ingest/type/{ingestType}");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<IngestModels.IngestModel[]>(url));
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForTopicAsync(string topic)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/for/topic/{topic}");
        var result = await RetryRequestAsync(async () => await this.Client.GetAsync<IngestModels.IngestModel[]>(url));
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an HTTP request to the api to update the ingest.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public async Task<IngestModels.IngestModel?> UpdateIngestAsync(IngestModels.IngestModel ingest)
    {
        var url = this.Options.ApiUrl.Append($"services/ingests/{ingest.Id}");
        return await RetryRequestAsync(async () => await this.Client.PutAsync<IngestModels.IngestModel>(url, JsonContent.Create(ingest)));
    }
    #endregion

    #region Content Reference Methods
    /// <summary>
    /// Make an HTTP request to the api to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> FindContentReferenceAsync(string source, string uid)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{source}?uid={uid}");
        var response = await RetryRequestAsync(async () => await this.Client.GetAsync(url));

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<ContentReferenceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an HTTP request to the api to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> AddContentReferenceAsync(ContentReferenceModel contentReference)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference)));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel contentReference)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{contentReference.Source}?uid={contentReference.Uid}");
        return await RetryRequestAsync(async () => await this.Client.PutAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference)));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the specified content reference with Kafka information.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> UpdateContentReferenceKafkaAsync(ContentReferenceModel contentReference)
    {
        var url = this.Options.ApiUrl.Append($"services/content/references/{contentReference.Source}/kafka?uid={contentReference.Uid}");
        return await RetryRequestAsync(async () => await this.Client.PutAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference)));
    }
    #endregion

    #region Content Methods
    /// <summary>
    /// Make an HTTP request to the api to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public async Task<ContentModel?> FindContentByUidAsync(string uid, string? source)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/find?uid={uid}&source={source}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ContentModel?> FindContentByIdAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<ContentModel?>(url));
    }

    /// <summary>
    /// Make an HTTP request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ContentModel?> AddContentAsync(ContentModel content)
    {
        var url = this.Options.ApiUrl.Append($"services/contents");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<ContentModel>(url, JsonContent.Create(content)));
    }

    /// <summary>
    /// Make an HTTP request to the api to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName)
    {
        var url = this.Options.ApiUrl.Append($"services/contents/{contentId}/upload?version={version}");
        var fileContent = new StreamContent(file);
        var ext = Path.GetExtension(fileName).Replace(".", "");
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(ext));
        var form = new MultipartFormDataContent
        {
            { fileContent, "files", fileName }
        };
        return await RetryRequestAsync(async () => await this.Client.PostAsync<ContentModel>(url, form));
    }

    /// <summary>
    /// Make an HTTP request to the api to update the content for the specified 'id'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ContentModel?> UpdateContentAsync(ContentModel content)
    {
        var url = this.Options.ApiUrl.Append($"editor/contents/{content.Id}");
        return await RetryRequestAsync(async () => await this.Client.PutAsync<ContentModel>(url, JsonContent.Create(content)));
    }
    #endregion

    #region Lookup Methods
    /// <summary>
    /// Make an HTTP request to the api to get the lookups.
    /// </summary>
    /// <returns></returns>
    public async Task<LookupModel?> GetLookupsAsync()
    {
        var url = this.Options.ApiUrl.Append($"editor/lookups");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<LookupModel>(url));
    }
    #endregion

    #region Work Order Methods
    /// <summary>
    /// Make an HTTP request to the api to get the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<WorkOrderModel?> FindWorkOrderAsync(long id)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/{id}");
        return await RetryRequestAsync(async () => await this.Client.GetAsync<WorkOrderModel>(url));
    }

    /// <summary>
    /// Make an HTTP request to the aip and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task<WorkOrderModel?> UpdateWorkOrderAsync(WorkOrderModel workOrder)
    {
        var url = this.Options.ApiUrl.Append($"services/work/orders/{workOrder.Id}");
        return await RetryRequestAsync(async () => await this.Client.PutAsync<WorkOrderModel>(url, JsonContent.Create(workOrder)));
    }
    #endregion

    #region Kafka Methods
    /// <summary>
    /// Publish content to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<DeliveryResultModel<SourceContent>?> SendMessageAsync(string topic, SourceContent content)
    {
        var url = this.Options.ApiUrl.Append($"kafka/producers/content/{topic}");
        return await RetryRequestAsync(async () => await this.Client.PostAsync<DeliveryResultModel<SourceContent>>(url, JsonContent.Create(content)));
    }
    #endregion
    #endregion
}
