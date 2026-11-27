using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Elastic.Models;

namespace TNO.Elastic
{
    /// <summary>
    /// The TNOElasticClient class
    /// </summary>
    public class TNOElasticClient : ITNOElasticClient
    {
        #region Variables
        /// <summary>
        /// Makes the requests that RequestAsync passes through. Its 8.x version sends
        /// 'compatible-with=8', which Elasticsearch 8.19 and 9.x both accept.
        /// </summary>
        private readonly ElasticsearchClient _client;

        /// <summary>
        /// One ElasticsearchClient per cluster and credentials, shared by every TNOElasticClient.
        /// The API registers TNOElasticClient per request scope; building a client each time opened
        /// a new connection pool per request that was never disposed.
        /// </summary>
        private static readonly ConcurrentDictionary<ClientKey, ElasticsearchClient> Clients = new();

        /// <summary>
        /// What makes two ElasticsearchClient instances interchangeable.
        /// </summary>
        private sealed record ClientKey(Uri Url, string ContentIndex, string? Username, string? Password, string? ApiKey);
        #endregion

        #region Properties
        protected ElasticOptions Options { get; }
        protected IHttpRequestClient Client { get; }
        protected ILogger<TNOElasticClient> Logger { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public TNOElasticClient(IHttpRequestClient httpClient, IOptions<ElasticOptions> options, ILogger<TNOElasticClient> logger)
        {
            _client = GetClient(options.Value);
            this.Client = httpClient;
            this.Options = options.Value;
            this.Logger = logger;
            if (this.Options.Url == null) throw new ConfigurationException("Elastic Url configuration is required.");
            var username = !String.IsNullOrWhiteSpace(this.Options.Username)
                ? this.Options.Username
                : Environment.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = !String.IsNullOrWhiteSpace(this.Options.Password)
                ? this.Options.Password
                : Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                var credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{username}:{password}"));
                this.Client.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }
            else if (!String.IsNullOrWhiteSpace(this.Options.ApiKey))
            {
                this.Client.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", this.Options.ApiKey);
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Get the connection string for elasticsearch.
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static ElasticsearchClient GetClient(ElasticOptions options)
        {
            if (options.Url == null) throw new ConfigurationException("Elastic configuration property 'Elastic:Url' is required'");
            var username = !String.IsNullOrWhiteSpace(options.Username)
                ? options.Username
                : Environment.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = !String.IsNullOrWhiteSpace(options.Password)
                ? options.Password
                : Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");
            var basic = !String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password);
            var apiKey = !basic && !String.IsNullOrWhiteSpace(options.ApiKey) ? options.ApiKey : null;
            var key = new ClientKey(options.Url, options.ContentIndex, basic ? username : null, basic ? password : null, apiKey);

            return Clients.GetOrAdd(key, static key =>
            {
                var settings = new ElasticsearchClientSettings(new SingleNodePool(key.Url))
                    .DefaultIndex(key.ContentIndex)
                    .RequestTimeout(new TimeSpan(0, 30, 0));

                if (key.Username != null && key.Password != null)
                    settings.Authentication(new BasicAuthentication(key.Username, key.Password));
                else if (key.ApiKey != null)
                    settings.Authentication(new ApiKey(key.ApiKey));

                return new ElasticsearchClient(settings);
            });
        }

        /// <summary>
        /// Make any request to Elasticsearch and return the unparsed response.
        /// An unsuccessful status, or no response at all, is returned rather than thrown; a cancelled
        /// 'cancellationToken' throws OperationCanceledException.
        /// Every segment of 'path' must be trusted or escaped with Uri.EscapeDataString: it is sent as given.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="path"></param>
        /// <param name="body"></param>
        /// <param name="opaqueId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<ElasticResponse> RequestAsync(System.Net.Http.HttpMethod method, string path, string? body = null, string? opaqueId = null, CancellationToken cancellationToken = default)
        {
            var configuration = opaqueId == null ? null : new RequestConfiguration { OpaqueId = opaqueId };
            // Relative to the cluster URL, which may carry a path of its own (a proxy route).
            var endpoint = new EndpointPath(ToTransportMethod(method), path.TrimStart('/'));
            var response = await _client.Transport.RequestAsync<StringResponse>(endpoint,
                body == null ? null : PostData.String(body), null, configuration, cancellationToken);
            // The transport reports a cancelled request as a failed response; callers stop on cancellation.
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException($"Elasticsearch request '{method} {path}' was cancelled.", response.ApiCallDetails?.OriginalException, cancellationToken);
            return new ElasticResponse(response.ApiCallDetails?.HttpStatusCode, response.Body, response.ApiCallDetails?.OriginalException);
        }

        private static global::Elastic.Transport.HttpMethod ToTransportMethod(System.Net.Http.HttpMethod method) => method.Method switch
        {
            "GET" => global::Elastic.Transport.HttpMethod.GET,
            "POST" => global::Elastic.Transport.HttpMethod.POST,
            "PUT" => global::Elastic.Transport.HttpMethod.PUT,
            "DELETE" => global::Elastic.Transport.HttpMethod.DELETE,
            "HEAD" => global::Elastic.Transport.HttpMethod.HEAD,
            _ => throw new NotSupportedException($"HTTP method '{method}' is not supported by Elasticsearch."),
        };

        /// <summary>
        /// Make a request to Elasticsearch 'index' with the specified 'query'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<SearchResultModel<T>> SearchAsync<T>(
            string index, JsonDocument query) where T : class
        {
            var url = this.Options.Url!.Append($"/{index}/_search?pretty=true");
            var content = JsonContent.Create(query);
            try
            {
                var response = await this.Client.PostAsync<SearchResultModel<T>>(url, content);
                return response ?? new SearchResultModel<T>();
            }
            catch (Exception ex)
            {
                // Elasticsearch rejects a query for a reason that only makes sense beside the
                // query itself, and the query is built by the caller (the browser posts it), so
                // it is not otherwise recoverable from a log.
                LogSearchFailure(ex, index, query.RootElement);
                throw;
            }
        }

        /// <summary>
        /// Make a request to Elasticsearch 'index' with the specified 'query'.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<SearchResultModel<T>> SearchAsync<T>(
            string index, JsonElement query) where T : class
        {
            var url = this.Options.Url!.Append($"/{index}/_search?pretty=true");
            var content = JsonContent.Create(query);
            try
            {
                var response = await this.Client.PostAsync<SearchResultModel<T>>(url, content);
                return response ?? new SearchResultModel<T>();
            }
            catch (Exception ex)
            {
                LogSearchFailure(ex, index, query);
                throw;
            }
        }

        /// <summary>
        /// Make a request to Elasticsearch 'index' with the specified 'query' and return the unparsed response.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<JsonDocument> SearchRawAsync(string index, JsonDocument query)
        {
            var url = this.Options.Url!.Append($"/{index}/_search");
            var content = JsonContent.Create(query);
            try
            {
                var response = await this.Client.PostAsync<JsonDocument>(url, content);
                return response ?? JsonDocument.Parse("{}");
            }
            catch (Exception ex)
            {
                LogSearchFailure(ex, index, query.RootElement);
                throw;
            }
        }

        /// <summary>The most of a rejected query to copy into the log.</summary>
        private const int MaxLoggedQueryChars = 4000;

        /// <summary>
        /// Log the index and the query a failed search was made with. The exception itself
        /// carries what Elasticsearch answered; this says what it was answering.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="index"></param>
        /// <param name="query"></param>
        private void LogSearchFailure(Exception ex, string index, JsonElement query)
        {
            var json = query.ToString() ?? "";
            if (json.Length > MaxLoggedQueryChars)
                json = $"{json[..MaxLoggedQueryChars]}...[truncated, {json.Length - MaxLoggedQueryChars} more character(s)]";
            this.Logger.LogError(ex, "Elasticsearch search failed. Index:{Index}, query:{Query}", index, json);
        }

        /// <summary>
        /// Make a request to Elasticsearch to validate query
        /// </summary>
        /// <param name="index"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<ValidateResultModel> ValidateAsync(string index, JsonDocument query)
        {
            var url = this.Options.Url!.Append($"/{index}/_validate/query?explain=true");
            var content = JsonContent.Create(query);
            var response = await this.Client.PostAsync<ValidateResultModel>(url, content);
            return response ?? new ValidateResultModel();
        }
        #endregion
    }
}
