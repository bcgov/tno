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
        var migrations = types.Select(type => (_provider.GetRequiredService(type) as Migration) ?? throw new InvalidOperationException($"Migration '{type.Name}' missing from service provider")).ToArray();
        ValidateStep(migrations);
        foreach (var migration in migrations)
        {
            if (!_rollback)
                await migration.RunUpAsync();
            else
                await migration.RunDownAsync();
        }

        if (types.Length == 0) _logger.LogInformation("Elastic already up-to-date.");
        else _logger.LogInformation("Elastic migration completed successfully.");
    }

    /// <summary>
    /// A single step applies to one migration that supports steps, and never to a rollback (which
    /// always runs every step).
    /// </summary>
    /// <param name="migrations"></param>
    private void ValidateStep(Migration[] migrations)
    {
        if (_options.Step == MigrationStepName.All || migrations.Length == 0) return;
        var step = _options.Step.ToString().ToLowerInvariant();
        if (_rollback)
            throw new InvalidOperationException($"A rollback runs every step; remove step '{step}'. Nothing was changed.");
        if (migrations.Length > 1)
            throw new InvalidOperationException($"Step '{step}' applies to one migration, but {migrations.Length} are pending ({String.Join(", ", migrations.Select(m => m.Version))}). Name it with Elastic__MigrationVersion (m=). Nothing was changed.");
        if (!migrations[0].SupportsSteps)
            throw new InvalidOperationException($"Migration {migrations[0].Version} does not run in steps; remove step '{step}'. Nothing was changed.");
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
            .Size(1000)
            , cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError(response.OriginalException, "Failed to determine migration version. Error: {error}", response.ServerError);
            throw response.OriginalException;
        }

        return response.Documents.OrderByDescending(ss => GenerateVersionKey(ss.Version)).FirstOrDefault()?.Version ?? "";
    }

    /// <summary>
    /// A cluster with no migration history must be empty, or be given a baseline: running the
    /// first migrations against indexes that already exist would fail or replace them.
    /// </summary>
    /// <param name="types"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The current version after the baseline ("" for an empty cluster).</returns>
    private async Task<string> ApplyBaselineAsync(Type[] types, CancellationToken cancellationToken)
    {
        var existing = new List<string>();
        foreach (var name in new[] { _options.ContentIndex, _options.PublishedIndex }.Where(n => !String.IsNullOrWhiteSpace(n)))
        {
            var exists = await _elasticClient.Indices.ExistsAsync(Indices.Index(name), null, cancellationToken);
            if (exists.Exists) existing.Add(name);
        }

        if (String.IsNullOrWhiteSpace(_options.BaselineVersion))
        {
            if (existing.Count == 0) return "";
            throw new InvalidOperationException(
                $"The cluster has indexes ({String.Join(", ", existing)}) but no migration history in '{_options.MigrationIndex}'. " +
                "Set Elastic__BaselineVersion to the version the cluster should be treated as already at (e.g. 1.0.10); nothing was changed.");
        }

        var baselineIndex = Array.FindIndex(types, t => t.GetCustomAttribute<MigrationAttribute>()?.Id == _options.BaselineVersion);
        if (baselineIndex == -1) throw new InvalidOperationException($"Baseline migration does not exist '{_options.BaselineVersion}'.");
        foreach (var type in types.Take(baselineIndex + 1))
        {
            var version = type.GetCustomAttribute<MigrationAttribute>()!.Id;
            var response = await _elasticClient.IndexAsync(new MigrationVersion(version, "baseline"), i => i.Index(_options.MigrationIndex).Id(version).Refresh(Refresh.WaitFor), cancellationToken);
            if (!response.IsValid) throw response.OriginalException;
        }
        _logger.LogWarning("Recorded baseline migration version '{version}'; earlier migrations were not run.", _options.BaselineVersion);
        return _options.BaselineVersion;
    }

    private long GenerateVersionKey(string value)
    {
        var values = value.Split(".");
        var major = values.Length > 0 ? $"{"000"[values[0].Length..]}{values[0]}" : "000";
        var minor = values.Length > 1 ? $"{"000"[values[1].Length..]}{values[1]}" : "000";
        var patch = values.Length > 2 ? $"{"000"[values[2].Length..]}{values[2]}" : "000";

        return long.Parse($"{major}{minor}{patch}");
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
        var types = Assembly.GetExecutingAssembly().GetMigrationTypes().OrderBy(t => GenerateVersionKey(t.GetCustomAttribute<MigrationAttribute>()?.Id ?? "")).ToArray();
        if (String.IsNullOrWhiteSpace(currentVersion))
            currentVersion = await ApplyBaselineAsync(types, cancellationToken);
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
