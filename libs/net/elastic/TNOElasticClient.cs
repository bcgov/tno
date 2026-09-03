using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.Elastic.Models;

namespace TNO.Elastic
{
    /// <summary>
    /// The TNOElasticClient class
    /// </summary>
    public class TNOElasticClient : ElasticClient, ITNOElasticClient
    {
        #region Variables
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
        public TNOElasticClient(IHttpRequestClient httpClient, IOptions<ElasticOptions> options, ILogger<TNOElasticClient> logger) : base(GetConnectionSettings(options.Value))
        {
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
        private static ConnectionSettings GetConnectionSettings(ElasticOptions options)
        {
            if (options.Url == null) throw new ConfigurationException("Elastic configuration property 'Elastic:Url' is required'");
            var username = !String.IsNullOrWhiteSpace(options.Username)
                ? options.Username
                : Environment.GetEnvironmentVariable("ELASTIC_USERNAME");
            var password = !String.IsNullOrWhiteSpace(options.Password)
                ? options.Password
                : Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");

            var connection = new ConnectionSettings(options.Url)
                .DefaultIndex(options.ContentIndex)
                .EnableApiVersioningHeader()
                .RequestTimeout(new TimeSpan(0, 30, 0))
                .ThrowExceptions();

            if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            {
                connection.BasicAuthentication(username, password);
            }
            else if (!String.IsNullOrWhiteSpace(options.ApiKey))
            {
                connection.ApiKeyAuthentication(new ApiKeyAuthenticationCredentials(options.ApiKey));
            }

            return connection;
        }

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
