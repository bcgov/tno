using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationBuilder class, provides a way to build a migration.
/// </summary>
public class MigrationBuilder
{
    #region Properties
    /// <summary>
    /// get - Elastic client.
    /// </summary>
    public IElasticClient Client { get; }

    /// <summary>
    /// get - Elastic migration options.
    /// </summary>
    public ElasticMigrationOptions MigrationOptions { get; }

    /// <summary>
    /// get - JSON serializer options.
    /// </summary>
    public JsonSerializerOptions SerializerOptions { get; }

    /// <summary>
    /// get - A logger.
    /// </summary>
    public ILogger Logger { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationBuilder object, initializes with specified parameters.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="migrationOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public MigrationBuilder(
        IElasticClient client,
        IOptions<ElasticMigrationOptions> migrationOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<MigrationBuilder> logger)
    {
        this.Client = client;
        this.MigrationOptions = migrationOptions.Value;
        this.SerializerOptions = serializerOptions.Value;
        this.Logger = logger;
    }
    #endregion
}
