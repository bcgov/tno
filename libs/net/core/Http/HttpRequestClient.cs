using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

namespace TNO.Core.Http
{
    /// <summary>
    /// HttpRequestClient class, provides a generic way to make HTTP requests to other services.
    /// </summary>
    public class HttpRequestClient : IHttpRequestClient
    {
        #region Variables
        protected readonly JsonSerializerOptions _serializeOptions;
        private readonly ILogger<HttpRequestClient> _logger;
        #endregion

        #region Properties
        /// <summary>
        /// get - The HttpClient use to make requests.
        /// </summary>
        public HttpClient Client { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a HttpRequestClient class, initializes it with the specified arguments.
        /// </summary>
        /// <param name="clientFactory"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public HttpRequestClient(IHttpClientFactory clientFactory, IOptionsMonitor<JsonSerializerOptions> options, ILogger<HttpRequestClient> logger)
        {
            this.Client = clientFactory.CreateClient();
            _serializeOptions = options?.CurrentValue ?? new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            };
            if (options?.CurrentValue != null)
            {
                if (!_serializeOptions.PropertyNameCaseInsensitive)
                    _serializeOptions.PropertyNameCaseInsensitive = true;
                if (_serializeOptions.PropertyNamingPolicy != JsonNamingPolicy.CamelCase)
                    _serializeOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                if (_serializeOptions.DefaultIgnoreCondition != JsonIgnoreCondition.WhenWritingNull)
                    _serializeOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            }
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Dispose the HttpClient.
        /// </summary>
        public void Dispose()
        {
            this.Client.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Deserialize the specified 'response' into the specified type of 'TModel'.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<TModel?> DeserializeAsync<TModel>(HttpResponseMessage response)
        {
            var responseStream = await response.Content.ReadAsStreamAsync();
            // var data = await response.Content.ReadAsByteArrayAsync();
            var contentType = response.Content.Headers.ContentType;
            try
            {
                if (contentType?.MediaType?.Contains("json", StringComparison.InvariantCultureIgnoreCase) == true)
                    return JsonSerializer.Deserialize<TModel>(responseStream, _serializeOptions);
            }
            catch (Exception ex)
            {
                var data = responseStream.ReadAllBytes();
                var body = Encoding.Default.GetString(data);
                _logger.LogError(ex, "Failed to deserialize response: {body}", body);
                throw;
            }

            throw new HttpClientRequestException(response, $"Response must contain JSON but was '{contentType?.MediaType}'.");
        }

        #region HttpResponseMessage Methods
        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendJsonAsync<T>(string url, HttpMethod? method = null, T? data = null)
            where T : class
        {
            return await SendJsonAsync(url, method, null, data);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(string url, HttpMethod? method = null, HttpContent? content = null)
        {
            return await SendAsync(url, method, null, content);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendJsonAsync<T>(string url, HttpMethod? method, HttpRequestHeaders? headers, T? data = null)
            where T : class
        {
            HttpContent? content = null;

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _serializeOptions);
                content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            }

            return await SendAsync(url, method, headers, content);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(string url, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null,
                string? etag = null)
        {
            if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException($"Argument '{nameof(url)}' must be a valid URL.");

            if (method == null) method = HttpMethod.Get;

            var message = new HttpRequestMessage(method, url)
            {
                Content = content
            };

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            var userAgent = message.Headers.UserAgent;
            if (string.IsNullOrEmpty(userAgent.ToString()))
            {
                message.Headers.Add("User-Agent", "TNO");
            }
            if (!string.IsNullOrEmpty(etag))
            {
                message.Headers.TryAddWithoutValidation("If-None-Match", etag);
            }

            _logger.LogDebug("HTTP request made: {method}:{uri} (User-Agent: {userAgent})",
                message.Method, message.RequestUri, userAgent);
            return await this.Client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead);
        }

        /// <summary>
        /// GET request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await SendAsync(url, HttpMethod.Get);
        }

        /// <summary>
        /// POST request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostJsonAsync<T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync(url, HttpMethod.Post, data);
        }

        /// <summary>
        /// POST request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(string url, HttpContent? content = null)
        {
            return await SendAsync(url, HttpMethod.Post, content);
        }

        /// <summary>
        /// PUT request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutJsonAsync<T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync(url, HttpMethod.Put, data);
        }

        /// <summary>
        /// PUT request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutAsync(string url, HttpContent? content = null)
        {
            return await SendAsync(url, HttpMethod.Put, content);
        }

        /// <summary>
        /// DELETE request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteJsonAsync<T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync(url, HttpMethod.Delete, data);
        }

        /// <summary>
        /// DELETE request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteAsync(string url, HttpContent? content = null)
        {
            return await SendAsync(url, HttpMethod.Delete, content);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendJsonAsync<T>(Uri uri, HttpMethod? method = null, T? data = null)
            where T : class
        {
            return await SendJsonAsync(uri, method, null, data);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(Uri uri, HttpMethod? method = null, HttpContent? content = null,
                string? etag = null)
        {
            return await SendAsync(uri, method, null, content, etag);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendJsonAsync<T>(Uri uri, HttpMethod? method, HttpRequestHeaders? headers, T? data = null)
            where T : class
        {
            return await SendJsonAsync(uri.OriginalString, method, headers, data);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <param name="etag"></param>
        /// <returns></returns>
        public virtual async Task<HttpResponseMessage> SendAsync(Uri uri, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null,
                string? etag = null)
        {
            return await SendAsync(uri.OriginalString, method, headers, content, etag);
        }

        /// <summary>
        /// GET request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> GetAsync(Uri uri)
        {
            return await SendAsync(uri, HttpMethod.Get);
        }
        public async Task<HttpResponseMessage> GetAsync(Uri uri, string etag)
        {
            return await SendAsync(uri, HttpMethod.Get, null, etag);
        }

        /// <summary>
        /// POST request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostJsonAsync<T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync(uri, HttpMethod.Post, data);
        }

        /// <summary>
        /// POST request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent? content = null)
        {
            return await SendAsync(uri, HttpMethod.Post, content);
        }

        /// <summary>
        /// PUT request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutJsonAsync<T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync(uri, HttpMethod.Put, data);
        }

        /// <summary>
        /// PUT request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> PutAsync(Uri uri, HttpContent? content = null)
        {
            return await SendAsync(uri, HttpMethod.Put, content);
        }

        /// <summary>
        /// DELETE request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteJsonAsync<T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync(uri, HttpMethod.Delete, data);
        }

        /// <summary>
        /// DELETE request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DeleteAsync(Uri uri, HttpContent? content = null)
        {
            return await SendAsync(uri, HttpMethod.Delete, content);
        }
        #endregion

        #region Serialization Methods
        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendJsonAsync<TModel, T>(string url, HttpMethod? method = null, T? data = null, Func<HttpResponseMessage, bool>? onError = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(url, method, null, data, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendAsync<TModel>(string url, HttpMethod? method = null, HttpContent? content = null, Func<HttpResponseMessage, bool>? onError = null)
        {
            return await SendAsync<TModel>(url, method, null, content, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendJsonAsync<TModel, T>(string url, HttpMethod? method, HttpRequestHeaders? headers, T? data = null, Func<HttpResponseMessage, bool>? onError = null)
            where T : class
        {
            StringContent? content = null;

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data, _serializeOptions);
                content = new StringContent(json, Encoding.UTF8, MediaTypeNames.Application.Json);
            }

            return await SendAsync<TModel>(url, method, headers, content, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendAsync<TModel>(string url, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null, Func<HttpResponseMessage, bool>? onError = null)
        {
            var response = await SendAsync(url, method, headers, content);

            if (response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NoContent)
                    return default;

                return await DeserializeAsync<TModel>(response);
            }

            // If the error handle is not provided, or if it returns false throw an error.
            if ((onError?.Invoke(response) ?? false) == false)
            {
                var error = new HttpClientRequestException(response);
                _logger.LogError(error, "{message}", error.Message);
                throw error;
            }

            return default;
        }

        /// <summary>
        /// GET request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> GetAsync<TModel>(string url)
        {
            return await SendAsync<TModel>(url, HttpMethod.Get);
        }

        /// <summary>
        /// POST request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PostJsonAsync<TModel, T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(url, HttpMethod.Post, data);
        }

        /// <summary>
        /// POST request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PostAsync<TModel>(string url, HttpContent? content = null)
        {
            return await SendAsync<TModel>(url, HttpMethod.Post, content);
        }

        /// <summary>
        /// PUT request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PutJsonAsync<TModel, T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(url, HttpMethod.Put, data);
        }

        /// <summary>
        /// PUT request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PutAsync<TModel>(string url, HttpContent? content = null)
        {
            return await SendAsync<TModel>(url, HttpMethod.Put, content);
        }

        /// <summary>
        /// DELETE request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> DeleteJsonAsync<TModel, T>(string url, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(url, HttpMethod.Delete, data);
        }

        /// <summary>
        /// DELETE request to the specified 'url'.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> DeleteAsync<TModel>(string url, HttpContent? content = null)
        {
            return await SendAsync<TModel>(url, HttpMethod.Delete, content);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="data"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendJsonAsync<TModel, T>(Uri uri, HttpMethod? method = null, T? data = null, Func<HttpResponseMessage, bool>? onError = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(uri, method, null, data, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="content"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendAsync<TModel>(Uri uri, HttpMethod? method = null, HttpContent? content = null, Func<HttpResponseMessage, bool>? onError = null)
        {
            return await SendAsync<TModel>(uri, method, null, content, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="data"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendJsonAsync<TModel, T>(Uri uri, HttpMethod? method, HttpRequestHeaders? headers, T? data = null, Func<HttpResponseMessage, bool>? onError = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(uri.OriginalString, method, headers, data, onError);
        }

        /// <summary>
        /// Send an HTTP request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="method"></param>
        /// <param name="headers"></param>
        /// <param name="content"></param>
        /// <param name="onError"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public virtual async Task<TModel?> SendAsync<TModel>(Uri uri, HttpMethod? method, HttpRequestHeaders? headers, HttpContent? content = null, Func<HttpResponseMessage, bool>? onError = null)
        {
            return await SendAsync<TModel>(uri.OriginalString, method, headers, content, onError);
        }

        /// <summary>
        /// GET request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> GetAsync<TModel>(Uri uri, Func<HttpResponseMessage, bool>? onError = null)
        {
            return await SendAsync<TModel>(uri, HttpMethod.Get, onError: onError);
        }

        /// <summary>
        /// POST request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PostJsonAsync<TModel, T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(uri, HttpMethod.Post, data);
        }

        /// <summary>
        /// POST request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PostAsync<TModel>(Uri uri, HttpContent? content = null)
        {
            return await SendAsync<TModel>(uri, HttpMethod.Post, content);
        }

        /// <summary>
        /// PUT request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PutJsonAsync<TModel, T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(uri, HttpMethod.Put, data);
        }

        /// <summary>
        /// PUT request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> PutAsync<TModel>(Uri uri, HttpContent? content = null)
        {
            return await SendAsync<TModel>(uri, HttpMethod.Put, content);
        }

        /// <summary>
        /// DELETE request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="data"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> DeleteJsonAsync<TModel, T>(Uri uri, T? data = null)
            where T : class
        {
            return await SendJsonAsync<TModel, T>(uri, HttpMethod.Delete, data);
        }

        /// <summary>
        /// DELETE request to the specified 'uri'.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <exception cref="HttpClientRequestException">Response did not return a success status code.</exception>
        /// <returns></returns>
        public async Task<TModel?> DeleteAsync<TModel>(Uri uri, HttpContent? content = null)
        {
            return await SendAsync<TModel>(uri, HttpMethod.Delete, content);
        }
        #endregion
        #endregion
    }
}
