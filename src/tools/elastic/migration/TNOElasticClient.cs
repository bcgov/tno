using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
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
    private static ElasticsearchClientSettings GetClientSettings(ElasticOptions elasticOptions, JsonSerializerOptions serializerOptions)
    {
        if (elasticOptions.Url == null) throw new ConfigurationException($"Elastic configuration property 'Elastic:Url' is required'");
        var username = !String.IsNullOrWhiteSpace(elasticOptions.Username)
            ? elasticOptions.Username
            : Environment.GetEnvironmentVariable("ELASTIC_USERNAME");
        var password = !String.IsNullOrWhiteSpace(elasticOptions.Password)
            ? elasticOptions.Password
            : Environment.GetEnvironmentVariable("ELASTIC_PASSWORD");

        var pool = new SingleNodePool(elasticOptions.Url);
        var settings = new ElasticsearchClientSettings(pool, (serializer, settings) => new DefaultSourceSerializer(settings, (options) =>
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
            .PrettyJson(true)
            .ThrowExceptions();

        if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
            settings.Authentication(new BasicAuthentication(username, password));
        else if (!String.IsNullOrWhiteSpace(elasticOptions.ApiKey))
            settings.Authentication(new ApiKey(elasticOptions.ApiKey));

        return settings;
    }
    #endregion
}
