using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Elasticsearch.Net;
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
        protected BasicAuthenticationCredentials Credentials { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="options"></param>
        public TNOElasticClient(IHttpRequestClient httpClient, IOptions<ElasticOptions> options) : base(GetConnectionSettings(options.Value))
        {
            this.Client = httpClient;
            this.Options = options.Value;
            if (this.Options.Url == null) throw new ConfigurationException("Elastic Url configuration is required.");
            var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME") ?? throw new ConfigurationException("Elastic environment variable 'ELASTIC_USERNAME' is required.");
            var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD") ?? throw new ConfigurationException("Elastic environment variable 'ELASTIC_PASSWORD' is required.");
            this.Credentials = new BasicAuthenticationCredentials(username, password);
            var credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes($"{this.Credentials.Username}:{password}"));
            this.Client.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
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
            var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME") ?? throw new ConfigurationException("Elastic environment variable 'ELASTIC_USERNAME' is required.");
            var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD") ?? throw new ConfigurationException("Elastic environment variable 'ELASTIC_PASSWORD' is required.");

            return new ConnectionSettings(options.Url)
                .BasicAuthentication(username, password)
                .DefaultIndex(options.UnpublishedIndex)
                .EnableApiVersioningHeader()
                .RequestTimeout(new TimeSpan(0, 30, 0))
                .ThrowExceptions();
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
            var response = await this.Client.PostAsync<SearchResultModel<T>>(url, content);
            return response ?? new SearchResultModel<T>();
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
            var response = await this.Client.PostAsync<SearchResultModel<T>>(url, content);
            return response ?? new SearchResultModel<T>();
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
