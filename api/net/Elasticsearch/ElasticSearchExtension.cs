using Nest;

namespace TNO.API.Elasticsearch
{
    /// <summary>
    /// The ElasticSearchExtension class
    /// </summary>
    public static class ElasticSearchExtension
    {
        /// <summary>
        /// Add a log message for the request.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config"></param> 
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration config)
        {
            var elasticSearchUrl = config["ElasticSearch:Url"];
            if (string.IsNullOrEmpty(elasticSearchUrl)) throw new ArgumentNullException(nameof(elasticSearchUrl));

            var defaultIndex = config["ElasticSearch:UnpublishedIndex"];
            if (string.IsNullOrEmpty(defaultIndex)) throw new ArgumentNullException(nameof(defaultIndex));

            var settings = new ConnectionSettings(new Uri(elasticSearchUrl))
                .BasicAuthentication(config["ELASTIC_USERNAME"]!, config["ELASTIC_PASSWORD"]!)
                .DefaultIndex(defaultIndex);

            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
        }
    }
}
