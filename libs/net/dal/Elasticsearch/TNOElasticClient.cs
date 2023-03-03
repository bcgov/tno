using Microsoft.Extensions.Options;
using Nest;
using TNO.Core.Exceptions;
using TNO.DAL.Config;

namespace TNO.DAL.Elasticsearch
{
    /// <summary>
    /// The TNOElasticClient class
    /// </summary>
    public class TNOElasticClient : ElasticClient
    {
        #region Constructors
        /// <summary>
        /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
        /// </summary>
        /// <param name="options"></param>
        public TNOElasticClient(IOptions<ElasticOptions> options) : base(GetConnectionSettings(options.Value))
        {
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
            if (options.Url == null) throw new ConfigurationException($"Elastic configuration property 'Elastic:Url' is required'");
            var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME") ?? throw new ConfigurationException($"Elastic environment variable 'ELASTIC_USERNAME' is required.");
            var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD") ?? throw new ConfigurationException($"Elastic environment variable 'ELASTIC_PASSWORD' is required.");

            return new ConnectionSettings(options.Url)
                .BasicAuthentication(username, password)
                .DefaultIndex(options.UnpublishedIndex)
                .ThrowExceptions();
        }
        #endregion
    }
}
