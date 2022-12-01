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

namespace TNO.Services;

/// <summary>
/// ApiService class, provides a way to interact with the API.
/// </summary>
public class ApiService : IApiService
{
    #region Variables
    private readonly IOpenIdConnectRequestClient _client;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ServiceOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ServiceAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="options"></param>
    public ApiService(IOpenIdConnectRequestClient client, IOptions<JsonSerializerOptions> serializerOptions, IOptions<ServiceOptions> options)
    {
        _client = client;
        _serializerOptions = serializerOptions.Value;
        _options = options.Value;
    }
    #endregion

    #region data location Methods
    /// <summary>
    /// Make an AJAX request to the api to get the data location for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<DataLocationModels.DataLocationModel?> GetDataLocationAsync(string name)
    {
        var url = _options.ApiUrl.Append($"services/data/locations/{name}");
        var response = await _client.GetAsync(url);

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
    /// Make an AJAX request to the api to get the connection.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IngestModels.ConnectionModel?> GetConnectionAsync(int id)
    {
        var url = _options.ApiUrl.Append($"services/connections/{id}");
        var response = await _client.GetAsync(url);

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
    /// Make an AJAX request to the api to fetch all sources.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.SourceModel>> GetSourcesAsync()
    {
        var url = _options.ApiUrl.Append($"services/sources");
        var result = await _client.GetAsync<IngestModels.SourceModel[]>(url);
        return result ?? Array.Empty<IngestModels.SourceModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch the sources for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<IngestModels.SourceModel?> GetSourceForCodeAsync(string code)
    {
        var url = _options.ApiUrl.Append($"services/sources/{code}");
        var response = await _client.GetAsync(url);

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
    /// Make an AJAX request to the api to fetch the ingest for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IngestModels.IngestModel?> GetIngestAsync(int id)
    {
        var url = _options.ApiUrl.Append($"services/ingests/{id}");
        var response = await _client.GetAsync(url);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<IngestModels.IngestModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch all ingests.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsAsync()
    {
        var url = _options.ApiUrl.Append($"services/ingests");
        var result = await _client.GetAsync<IngestModels.IngestModel[]>(url);
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch ingests for the specified ingest type.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForIngestTypeAsync(string ingestType)
    {
        var url = _options.ApiUrl.Append($"services/ingests/for/ingest/type/{ingestType}");
        var result = await _client.GetAsync<IngestModels.IngestModel[]>(url);
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch the ingest for the specified 'topic'.
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task<IEnumerable<IngestModels.IngestModel>> GetIngestsForTopicAsync(string topic)
    {
        var url = _options.ApiUrl.Append($"services/ingests/for/topic/{topic}");
        var result = await _client.GetAsync<IngestModels.IngestModel[]>(url);
        return result ?? Array.Empty<IngestModels.IngestModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to update the ingest.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    public async Task<IngestModels.IngestModel?> UpdateIngestAsync(IngestModels.IngestModel ingest)
    {
        var url = _options.ApiUrl.Append($"services/ingests/{ingest.Id}");
        return await _client.PutAsync<IngestModels.IngestModel>(url, JsonContent.Create(ingest));
    }
    #endregion

    #region Content Reference Methods
    /// <summary>
    /// Make an AJAX request to the api to find the content reference for the specified key.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> FindContentReferenceAsync(string source, string uid)
    {
        var url = _options.ApiUrl.Append($"services/content/references/{source}?uid={uid}");
        var response = await _client.GetAsync(url);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<ContentReferenceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an AJAX request to the api to add the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> AddContentReferenceAsync(ContentReferenceModel contentReference)
    {
        var url = _options.ApiUrl.Append($"services/content/references");
        return await _client.PostAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference));
    }

    /// <summary>
    /// Make an AJAX request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel contentReference)
    {
        var url = _options.ApiUrl.Append($"services/content/references/{contentReference.Source}?uid={contentReference.Uid}");
        return await _client.PutAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference));
    }
    #endregion

    #region Content Methods
    /// <summary>
    /// Make an AJAX request to the api to find the specified content.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public async Task<ContentModel?> FindContentByUidAsync(string uid, string? source)
    {
        var url = _options.ApiUrl.Append($"services/contents/find?uid={uid}&source={source}");
        return await _client.GetAsync<ContentModel?>(url);
    }

    /// <summary>
    /// Make an AJAX request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ContentModel?> FindContentByIdAsync(long id)
    {
        var url = _options.ApiUrl.Append($"services/contents/{id}");
        return await _client.GetAsync<ContentModel?>(url);
    }

    /// <summary>
    /// Make an AJAX request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ContentModel?> AddContentAsync(ContentModel content)
    {
        var url = _options.ApiUrl.Append($"services/contents");
        return await _client.PostAsync<ContentModel>(url, JsonContent.Create(content));
    }

    /// <summary>
    /// Make an AJAX request to the api to upload the file and link to specified content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="version"></param>
    /// <param name="file"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public async Task<ContentModel?> UploadFileAsync(long contentId, long version, Stream file, string fileName)
    {
        var url = _options.ApiUrl.Append($"services/contents/{contentId}/upload?version={version}");
        var fileContent = new StreamContent(file);
        var ext = Path.GetExtension(fileName).Replace(".", "");
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(ext));
        var form = new MultipartFormDataContent
        {
            { fileContent, "files", fileName }
        };
        return await _client.PostAsync<ContentModel>(url, form);
    }

    /// <summary>
    /// Make an AJAX request to the api to update the content for the specified 'id'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ContentModel?> UpdateContentAsync(ContentModel content)
    {
        var url = _options.ApiUrl.Append($"editor/contents/{content.Id}");
        return await _client.PutAsync<ContentModel>(url, JsonContent.Create(content));
    }
    #endregion

    #region Lookup Methods
    /// <summary>
    /// Make an AJAX request to the api to get the lookups.
    /// </summary>
    /// <returns></returns>
    public async Task<LookupModel?> GetLookupsAsync()
    {
        var url = _options.ApiUrl.Append($"editor/lookups");
        return await _client.GetAsync<LookupModel>(url);
    }
    #endregion

    #region Work Orders
    /// <summary>
    /// Make an AJAX request to the api to get the work order for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<WorkOrderModel?> FindWorkOrderAsync(long id)
    {
        var url = _options.ApiUrl.Append($"services/work/orders/{id}");
        return await _client.GetAsync<WorkOrderModel>(url);
    }

    /// <summary>
    /// Make an AJAX request to the aip and update the specified 'workOrder'.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <returns></returns>
    public async Task<WorkOrderModel?> UpdateWorkOrderAsync(WorkOrderModel workOrder)
    {
        var url = _options.ApiUrl.Append($"services/work/orders/{workOrder.Id}");
        return await _client.PutAsync<WorkOrderModel>(url, JsonContent.Create(workOrder));
    }
    #endregion

    #region Kafka
    /// <summary>
    /// Publish content to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<DeliveryResultModel<SourceContent>?> SendMessageAsync(string topic, SourceContent content)
    {
        var url = _options.ApiUrl.Append($"kafka/producers/content/{topic}");
        return await _client.PostAsync<DeliveryResultModel<SourceContent>>(url, JsonContent.Create(content));
    }
    #endregion
}
