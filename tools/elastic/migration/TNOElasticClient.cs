using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;

namespace TNO.Elastic.Migration;

/// <summary>
/// TNOElasticClient class, provides an Elasticsearch client that supports System.Text.Json.
/// </summary>
public class TNOElasticClient : ElasticsearchClient
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
    /// </summary>
    /// <param name="elasticOptions"></param>
    /// <param name="serializerOptions"></param>
    public TNOElasticClient(IOptions<ElasticOptions> elasticOptions, IOptions<JsonSerializerOptions> serializerOptions)
        : base(GetClientSettings(elasticOptions.Value, serializerOptions.Value))
    {

    }

    /// <summary>
    /// Creates a new instance of a TNOElasticClient object, initializes with specified parameters.
    /// </summary>
    /// <param name="elasticsearchClientSettings"></param>
    public TNOElasticClient(IElasticsearchClientSettings elasticsearchClientSettings) : base(elasticsearchClientSettings)
    {

    }
    #endregion

    #region Methods
    /// <summary>
    /// Get the connection string for elasticsearch.
    /// </summary>
    /// <param name="elasticOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    private static IElasticsearchClientSettings GetClientSettings(ElasticOptions elasticOptions, JsonSerializerOptions serializerOptions)
    {
        if (elasticOptions.Url == null) throw new ConfigurationException($"Elastic configuration property 'Elastic:Url' is required'");
        var username = Environment.GetEnvironmentVariable("ELASTIC_USERNAME") ?? throw new ConfigurationException($"Elastic environment variable 'ELASTIC_USERNAME' is required.");
        var password = Environment.GetEnvironmentVariable("ELASTIC_PASSWORD") ?? throw new ConfigurationException($"Elastic environment variable 'ELASTIC_PASSWORD' is required.");

        var pool = new SingleNodePool(elasticOptions.Url);
        return new ElasticsearchClientSettings(pool, (serializer, settings) => new DefaultSourceSerializer(settings, (options) =>
        {
            options.AllowTrailingCommas = serializerOptions.AllowTrailingCommas;
            options.DefaultBufferSize = serializerOptions.DefaultBufferSize;
            options.DefaultIgnoreCondition = serializerOptions.DefaultIgnoreCondition;
            options.DictionaryKeyPolicy = serializerOptions.DictionaryKeyPolicy;
            options.Encoder = serializerOptions.Encoder;
            options.IgnoreReadOnlyFields = serializerOptions.IgnoreReadOnlyFields;
            options.IgnoreReadOnlyProperties = serializerOptions.IgnoreReadOnlyProperties;
            options.IncludeFields = serializerOptions.IncludeFields;
            options.MaxDepth = serializerOptions.MaxDepth;
            options.NumberHandling = serializerOptions.NumberHandling;
            options.PropertyNameCaseInsensitive = serializerOptions.PropertyNameCaseInsensitive;
            options.PropertyNamingPolicy = serializerOptions.PropertyNamingPolicy;
            options.ReadCommentHandling = serializerOptions.ReadCommentHandling;
            options.ReferenceHandler = serializerOptions.ReferenceHandler;
            options.TypeInfoResolver = serializerOptions.TypeInfoResolver;
            options.UnknownTypeHandling = serializerOptions.UnknownTypeHandling;
            options.WriteIndented = serializerOptions.WriteIndented;
        }))
            .Authentication(new BasicAuthentication(username, password))
            .PrettyJson(true)
            .ThrowExceptions();
    }
    #endregion
}
