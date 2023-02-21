using Nest;

namespace TNO.API.Elasticsearch
{
    /// <summary>
    /// The TNOElasticClient class
    /// </summary>
    public class TNOElasticClient: ElasticClient
    {
        /// <summary>
        /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
        /// </summary>
        /// <param name="config"></param> 
        public TNOElasticClient(IConfiguration config): base(GetConnectionSettings(config))
        {
        }

        private static ConnectionSettings GetConnectionSettings(IConfiguration config)
        {
            var elasticSearchUrl = config["ElasticSearch:Url"];
            if (string.IsNullOrEmpty(elasticSearchUrl)) throw new ArgumentNullException(nameof(elasticSearchUrl));

            var defaultIndex = config["ElasticSearch:UnpublishedIndex"];
            if (string.IsNullOrEmpty(defaultIndex)) throw new ArgumentNullException(nameof(defaultIndex));

            return new ConnectionSettings(new Uri(elasticSearchUrl))
                .BasicAuthentication(config["ELASTIC_USERNAME"]!, config["ELASTIC_PASSWORD"]!)
                .DefaultIndex(defaultIndex)
                .ThrowExceptions();
        }
    }
}
