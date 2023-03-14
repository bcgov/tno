using System.Reflection;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TNO.Elastic.Migration.Models;

namespace TNO.Elastic.Migration;

/// <summary>
/// MigrationService class, provides a service that will perform Elasticsearch migrations.
/// Migration file names must follow the convention to work.
/// - They must be alphanumerically sorted
/// - Their version number can be their full name, or found after the first "_" character (i.e. "1.0.0.cs", "20230101_1.0.0.cs").
/// </summary>
public class MigrationService
{
    #region Variables
    private readonly IElasticClient _elasticClient;
    private readonly ElasticMigrationOptions _options;
    private readonly IServiceProvider _provider;
    private readonly ILogger _logger;
    private bool _rollback;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MigrationService object, initializes with specified parameters.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="elasticClient"></param>
    /// <param name="provider"></param>
    /// <param name="logger"></param>
    public MigrationService(
        IOptions<ElasticMigrationOptions> options,
        IElasticClient elasticClient,
        IServiceProvider provider,
        ILogger<MigrationService> logger)
    {
        _options = options.Value;
        _elasticClient = elasticClient;
        _provider = provider;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Service starting");
        _logger.LogWarning("The Elastic migration process is not transaction safe. If a failure occurs, any completed steps will leave a migration in a partially completed state.");

        var types = await GetMigrationVersionsAsync(cancellationToken);
        foreach (var type in types)
        {
            var migration = (_provider.GetRequiredService(type) as Migration) ?? throw new InvalidOperationException($"Migration '{type.Name}' missing from service provider");
            if (!_rollback)
                await migration.RunUpAsync();
            else
                await migration.RunDownAsync();
        }

        if (types.Length == 0) _logger.LogInformation("Elastic already up-to-date.");
        else _logger.LogInformation("Elastic migration completed successfully.");
    }

    /// <summary>
    /// Create the migration index if required.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task CreateMigrationIndexAsync(CancellationToken cancellationToken)
    {
        // Check if migration index exists.
        try
        {
            var exists = await _elasticClient.Indices.ExistsAsync(Indices.Index(_options.MigrationIndex), ied => ied.ErrorTrace(true), cancellationToken);

            // If the index does not exist, create it.
            if (!exists.Exists)
            {
                var model = new MigrationVersion();
                var descriptor = new CreateIndexDescriptor(_options.MigrationIndex)
                    .Map(tmd => tmd.AutoMap());

                var createResponse = await _elasticClient.Indices.CreateAsync(
                    _options.MigrationIndex,
                    cir => cir.Map<MigrationVersion>(m => m.AutoMap())
                        .Settings(s => s
                            .NumberOfReplicas(_options.NumberOfReplicas)
                            .NumberOfShards(_options.NumberOfShards)),
                    cancellationToken);

                if (!createResponse.IsValid)
                {
                    _logger.LogError(createResponse.OriginalException, "Failed to create migration index.");
                    throw createResponse.OriginalException;
                }
            }
        }
        catch (ElasticsearchClientException ex)
        {
            _logger.LogError(ex, "Failure to create index");
            if (ex.Response.HttpStatusCode == 502)
            {
                Thread.Sleep(5000);
                await CreateMigrationIndexAsync(cancellationToken);
            }
            else
                throw;
        }
    }

    /// <summary>
    /// Get the current migration version applied to Elastic.
    /// This is determined by the migrations index.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<string> GetCurrentMigrationVersionAsync(CancellationToken cancellationToken)
    {
        // Determine what the current version in Elasticsearch is.
        await CreateMigrationIndexAsync(cancellationToken);
        var response = await _elasticClient.SearchAsync<MigrationVersion>(sd => sd
            .Index(_options.MigrationIndex)
            .Size(1)
            .Sort(ss => ss.Descending(p => p.Version))
            , cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError(response.OriginalException, "Failed to determine migration version. Error: {error}", response.ServerError);
            throw response.OriginalException;
        }

        return response.Documents.FirstOrDefault()?.Version ?? "";
    }

    /// <summary>
    /// Get the migration object types that should be run.
    /// This will use the configured 'Elastic__MigrationVersion' if provided.
    /// Otherwise it will determine the value from requesting the information from Elasticsearch version index.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<Type[]> GetMigrationVersionsAsync(CancellationToken cancellationToken)
    {
        var currentVersion = await GetCurrentMigrationVersionAsync(cancellationToken);
        var types = Assembly.GetExecutingAssembly().GetMigrationTypes().OrderBy(t => t.Name).ToArray();
        var currentIndex = Array.FindIndex(types, 0, types.Length, t => t.GetCustomAttribute<MigrationAttribute>()?.Id == currentVersion);
        var requestedIndex = Array.FindIndex(types, 0, types.Length, t => t.GetCustomAttribute<MigrationAttribute>()?.Id == _options.MigrationVersion);

        if (!String.IsNullOrWhiteSpace(currentVersion))
            _logger.LogInformation("Current migration version is '{version}'", currentVersion);
        if (!String.IsNullOrWhiteSpace(_options.MigrationVersion))
            _logger.LogInformation("Requested migration version is '{version}'", _options.MigrationVersion);

        // No version was requested, return migrations to update fully.
        if (requestedIndex == -1)
        {
            if (!String.IsNullOrWhiteSpace(_options.MigrationVersion))
                throw new InvalidOperationException($"Requested migration does not exist '{_options.MigrationVersion}'.");
            // No migrations required.
            if (currentIndex == types.Length - 1) return Array.Empty<Type>();
            // Only update from the current.
            return types[(currentIndex + 1)..];
        }

        // No migration required.
        if (currentIndex == requestedIndex) return Array.Empty<Type>();
        // Only update from the current to the requested.
        else if (currentIndex < requestedIndex) return types.Skip(currentIndex + 1).Take(requestedIndex - currentIndex).ToArray();
        // Rollback to the requested migration.
        _rollback = true;
        return types.Skip(requestedIndex + 1).Take(currentIndex - requestedIndex).OrderByDescending(t => t.Name).ToArray();
    }
    #endregion
}
