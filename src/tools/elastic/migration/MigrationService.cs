using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly ITNOElasticClient _elasticClient;
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
        ITNOElasticClient elasticClient,
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
        var gatewayFailures = 0;
        while (true)
        {
            // Check if migration index exists.
            var exists = await _elasticClient.RequestAsync(HttpMethod.Head, _options.MigrationIndex, cancellationToken: cancellationToken);
            if (exists.HttpStatusCode == 200) return;
            if (exists.HttpStatusCode == 404)
            {
                var settings = new JsonObject();
                if (_options.NumberOfReplicas != null) settings["number_of_replicas"] = _options.NumberOfReplicas;
                if (_options.NumberOfShards != null) settings["number_of_shards"] = _options.NumberOfShards;
                var index = new JsonObject
                {
                    ["settings"] = settings,
                    ["mappings"] = new JsonObject
                    {
                        ["properties"] = new JsonObject
                        {
                            ["version"] = new JsonObject { ["type"] = "keyword" },
                            ["description"] = new JsonObject { ["type"] = "text", ["index"] = false },
                        },
                    },
                };
                var created = await _elasticClient.RequestAsync(HttpMethod.Put, _options.MigrationIndex, index.ToJsonString(), cancellationToken: cancellationToken);
                if (created.Success) return;
                exists = created;
            }

            _logger.LogError(exists.OriginalException, "Failed to create migration index. Error: {error}", exists);
            // A gateway in front of Elasticsearch Cloud intermittently answers 502.
            if (exists.HttpStatusCode != 502 || ++gatewayFailures >= _options.GatewayRetryLimit)
                throw new InvalidOperationException($"Failed to create migration index '{_options.MigrationIndex}'. {exists}", exists.OriginalException);
            await Task.Delay(5000, cancellationToken);
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
        var response = await _elasticClient.RequestAsync(HttpMethod.Post, $"{_options.MigrationIndex}/_search", "{\"size\":1000}", cancellationToken: cancellationToken);
        if (!response.Success)
        {
            _logger.LogError(response.OriginalException, "Failed to determine migration version. Error: {error}", response);
            throw new InvalidOperationException($"Failed to determine migration version. {response}", response.OriginalException);
        }

        using var document = JsonDocument.Parse(response.Body);
        return document.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray()
            .Select(hit => hit.GetProperty("_source").Deserialize<MigrationVersion>()!)
            .OrderByDescending(ss => GenerateVersionKey(ss.Version)).FirstOrDefault()?.Version ?? "";
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
            var exists = await _elasticClient.RequestAsync(HttpMethod.Head, name, cancellationToken: cancellationToken);
            if (exists.Success) existing.Add(name);
            // Anything but a clear "missing" stops: treating an unknown answer as missing would run every
            // migration from the start against a populated cluster.
            else if (!exists.NotFound)
                throw new InvalidOperationException($"Cannot tell whether index '{name}' exists; nothing was changed. {exists}", exists.OriginalException);
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
            var response = await _elasticClient.RequestAsync(HttpMethod.Put, $"{_options.MigrationIndex}/_doc/{version}?refresh=wait_for",
                JsonSerializer.Serialize(new MigrationVersion(version, "baseline")), cancellationToken: cancellationToken);
            if (!response.Success) throw new InvalidOperationException($"Failed to record baseline migration '{version}'. {response}", response.OriginalException);
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
