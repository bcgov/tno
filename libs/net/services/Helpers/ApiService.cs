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
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Exceptions;
using TNO.Core.Http;
using TNO.Services.Config;

namespace TNO.Services;

/// <summary>
/// ApiService class, provides a way to interace with the API.
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

    #region Data Source Methods
    /// <summary>
    /// Make an AJAX request to the api to fetch all data sources.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync()
    {
        var url = new Uri(_options.ApiUrl, $"services/data/sources");
        var result = await _client.GetAsync<DataSourceModel[]>(url);
        return result ?? Array.Empty<DataSourceModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch data sources for the specified media type.
    /// </summary>
    /// <param name="mediaType"></param>
    /// <returns></returns>
    public async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync(string mediaType)
    {
        var url = new Uri(_options.ApiUrl, $"services/data/sources/for/media/type/{mediaType}");
        var result = await _client.GetAsync<DataSourceModel[]>(url);
        return result ?? Array.Empty<DataSourceModel>();
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch the data source for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public async Task<DataSourceModel?> GetDataSourceAsync(string code)
    {
        var url = new Uri(_options.ApiUrl, $"services/data/sources/{code}");
        var response = await _client.GetAsync(url);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<DataSourceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch the data source for the specified 'name'.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<DataSourceModel?> GetDataSourceByNameAsync(string name)
    {
        var url = new Uri(_options.ApiUrl, $"services/data/sources/name/{name}");
        var response = await _client.GetAsync(url);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => await response.Content.ReadFromJsonAsync<DataSourceModel>(_serializerOptions),
            HttpStatusCode.NoContent => null,
            _ => throw new HttpClientRequestException(response),
        };
    }

    /// <summary>
    /// Make an AJAX request to the api to update the data source.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    public async Task<DataSourceModel?> UpdateDataSourceAsync(DataSourceModel dataSource)
    {
        var url = new Uri(_options.ApiUrl, $"services/data/sources/{dataSource.Id}");
        return await _client.PutAsync<DataSourceModel>(url, JsonContent.Create(dataSource));
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
        var url = new Uri(_options.ApiUrl, $"services/content/references/{source}?uid={uid}");
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
        var url = new Uri(_options.ApiUrl, $"services/content/references");
        return await _client.PostAsync<ContentReferenceModel>(url, JsonContent.Create(contentReference));
    }

    /// <summary>
    /// Make an AJAX request to the api to update the specified content reference.
    /// </summary>
    /// <param name="contentReference"></param>
    /// <returns></returns>
    public async Task<ContentReferenceModel?> UpdateContentReferenceAsync(ContentReferenceModel contentReference)
    {
        var url = new Uri(_options.ApiUrl, $"services/content/references/{contentReference.Source}?uid={contentReference.Uid}");
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
        var url = new Uri(_options.ApiUrl, $"services/contents/find?uid={uid}&source={source}");
        return await _client.GetAsync<ContentModel?>(url);
    }

    /// <summary>
    /// Make an AJAX request to the api to get the specified content.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<ContentModel?> FindContentByIdAsync(long id)
    {
        var url = new Uri(_options.ApiUrl, $"services/contents/{id}");
        return await _client.GetAsync<ContentModel?>(url);
    }

    /// <summary>
    /// Make an AJAX request to the api to add the specified content.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public async Task<ContentModel?> AddContentAsync(ContentModel content)
    {
        var url = new Uri(_options.ApiUrl, $"services/contents");
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
        var url = new Uri(_options.ApiUrl, $"services/contents/{contentId}/upload?version={version}");
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
        var url = new Uri(_options.ApiUrl, $"editor/contents/{content.Id}");
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
        var url = new Uri(_options.ApiUrl, $"editor/lookups");
        return await _client.GetAsync<LookupModel>(url);
    }
    #endregion
}
