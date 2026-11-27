using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using TNO.Elastic.Migration.Models;
using TNO.Models.Extensions;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration abstract class, provides a base for defining and running elastic migrations.
/// </summary>
public abstract class Migration
{
    #region Variables
    private readonly MigrationBuilder _builder;
    #endregion

    #region Properties
    /// <summary>
    /// get - The migration version number.
    /// </summary>
    public string Version
    {
        get
        {
            var type = this.GetType();
            var attr = type.GetCustomAttribute<MigrationAttribute>(true);
            if (attr != null)
                return attr.Id;

            // Extract the version after first "_".
            var index = type.Name.IndexOf('_');
            return index == -1 ? type.Name : type.Name[(index + 1)..];
        }
    }

    /// <summary>
    /// get/set - Number of failures.
    /// </summary>
    protected int Failures { get; set; }

    /// <summary>
    /// get - Whether the migration can run one step at a time (Elastic__Step).
    /// </summary>
    public virtual bool SupportsSteps => false;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    public Migration(MigrationBuilder builder)
    {
        _builder = builder;
    }
    #endregion

    #region Methods
    /// <summary>
    /// The steps to update elastic to this version.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <returns></returns>

    protected abstract Task UpAsync(MigrationBuilder migrationBuilder);

    /// <summary>
    /// The steps to rollback elastic to the prior version.
    /// </summary>
    /// <param name="migrationBuilder"></param>
    /// <returns></returns>
    protected abstract Task DownAsync(MigrationBuilder migrationBuilder);

    /// <summary>
    /// Run the migration to upgrade elastic.
    /// </summary>
    /// <returns></returns>
    public async Task RunUpAsync()
    {
        // A step migration runs its 'pre' scripts with its first step, and finishes with its last.
        var step = _builder.MigrationOptions.Step;
        _builder.Logger.LogInformation("Applying migration {version}{step}", this.Version, step == MigrationStepName.All ? "" : $", step '{step.ToString().ToLowerInvariant()}'");
        if (step == MigrationStepName.All || step == MigrationStepName.Prepare)
            await RunScriptsAsync(_builder, $"up{Path.DirectorySeparatorChar}pre");
        await UpAsync(_builder);
        if (step != MigrationStepName.All && step != MigrationStepName.Cutover)
        {
            _builder.Logger.LogInformation("Completed step '{step}' of migration {version}; it is recorded once 'cutover' completes.", step.ToString().ToLowerInvariant(), this.Version);
            return;
        }
        await RunScriptsAsync(_builder, "up");
        await RunScriptsAsync(_builder, $"up{Path.DirectorySeparatorChar}post");

        var version = new MigrationVersion(this.Version, "");
        var response = await _builder.Client.RequestAsync(HttpMethod.Put, $"{_builder.MigrationOptions.MigrationIndex}/_doc/{this.Version}?refresh=wait_for", JsonSerializer.Serialize(version));
        if (!response.Success)
        {
            _builder.Logger.LogError(response.OriginalException, "Failed to add migration {version} to index.  Error: {error}", this.Version, response.Body);
            throw new InvalidOperationException($"Failed to add migration {this.Version} to index. {response.Body}", response.OriginalException);
        }
    }

    /// <summary>
    /// Run the rollback migration to downgrade tot he prior version.
    /// </summary>
    /// <returns></returns>
    public async Task RunDownAsync()
    {
        _builder.Logger.LogInformation("Rolling back migration {version}", this.Version);
        await RunScriptsAsync(_builder, $"down{Path.DirectorySeparatorChar}pre");
        await DownAsync(_builder);
        await RunScriptsAsync(_builder, "down");
        await RunScriptsAsync(_builder, $"down{Path.DirectorySeparatorChar}post");

        var response = await _builder.Client.RequestAsync(HttpMethod.Delete, $"{_builder.MigrationOptions.MigrationIndex}/_doc/{this.Version}?refresh=wait_for");
        if (!response.Success && response.HttpStatusCode != 404)
        {
            _builder.Logger.LogError(response.OriginalException, "Failed to remove migration {version} from index.  Error: {error}", this.Version, response.Body);
            throw new InvalidOperationException($"Failed to remove migration {this.Version} from index. {response.Body}", response.OriginalException);
        }
    }

    /// <summary>
    /// Run all the scripts found at the specified path for this version.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private async Task RunScriptsAsync(MigrationBuilder builder, string path)
    {
        var fullPath = $"{builder.MigrationOptions.MigrationsPath}{Path.DirectorySeparatorChar}{this.Version}{Path.DirectorySeparatorChar}{path}";
        if (Directory.Exists(fullPath))
        {
            var files = Directory.GetFiles(fullPath).OrderBy(v => v).ToArray();

            foreach (var file in files.Where(n => n.EndsWith(".json")))
            {
                try
                {
                    // Perform each action in order.
                    builder.Logger.LogInformation("Applying migration step '{file}'", file);
                    var json = ReplaceIndexNames(builder, await File.ReadAllTextAsync(file));
                    var step = JsonSerializer.Deserialize<MigrationStep>(json, builder.SerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize '{file}'");
                    if (step.Action == MigrationAction.CreateIndex)
                        await CreateIndexAsync(builder, step, path);
                    else if (step.Action == MigrationAction.DeleteIndex)
                        await DeleteIndexAsync(builder, step, path);
                    else if (step.Action == MigrationAction.UpdateMapping)
                        await UpdateMappingAsync(builder, step, path);
                    else if (step.Action == MigrationAction.Reindex)
                        await ReindexAsync(builder, step, path);
                    else if (step.Action == MigrationAction.CreateAlias)
                        await CreateOrUpdateAliasAsync(builder, step, path);
                    else if (step.Action == MigrationAction.UpdateAlias)
                        await CreateOrUpdateAliasAsync(builder, step, path);
                    else if (step.Action == MigrationAction.DeleteAlias)
                        await DeleteAliasAsync(builder, step, path);
                    else if (step.Action == MigrationAction.CreatePipeline)
                        await CreateOrUpdatePipelineAsync(builder, step, path);
                    else if (step.Action == MigrationAction.UpdateByQuery)
                        await UpdateByQueryAsync(builder, step, path);
                }
                catch (InvalidOperationException ex)
                {
                    builder.Logger.LogError(ex, "Failed to run step '{file}'.", file);
                    throw;
                }
            }
        }
    }

    /// <summary>
    /// Replace the index name placeholders a step may use ('${ContentIndex}', '${PublishedIndex}',
    /// '${EvidenceIndex}') with the configured names, so one migration runs on clusters that name
    /// their indexes differently (on-premise and Elastic Cloud).
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="json"></param>
    /// <returns></returns>
    protected static string ReplaceIndexNames(MigrationBuilder builder, string json)
    {
        return json
            .Replace("${ContentIndex}", builder.MigrationOptions.ContentIndex)
            .Replace("${PublishedIndex}", builder.MigrationOptions.PublishedIndex)
            .Replace("${EvidenceIndex}", builder.MigrationOptions.EvidenceIndex);
    }

    #region Elastic actions
    /// <summary>
    /// Send a request and throw when Elasticsearch does not answer with success.
    /// </summary>
    private static async Task<ElasticResponse> SendAsync(MigrationBuilder builder, HttpMethod method, string path, string? body, string failure)
    {
        var response = await builder.Client.RequestAsync(method, path, body);
        if (!response.Success)
        {
            builder.Logger.LogError(response.OriginalException, "{failure}  Error: {error}", failure, response.Body);
            throw new InvalidOperationException($"{failure} {response.Body}", response.OriginalException);
        }
        return response;
    }

    private static string Serialize(MigrationBuilder builder, object data) => JsonSerializer.Serialize(data, builder.SerializerOptions);

    private static async Task CreateIndexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

        await SendAsync(builder, HttpMethod.Put, name, Serialize(builder, step.Data), $"Failed to create index '{name}'.");
    }

    private static async Task UpdateMappingAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

        await SendAsync(builder, HttpMethod.Put, $"{name}/_mapping", Serialize(builder, step.Data), $"Failed to update index '{name}' mapping.");
    }

    private async Task ReindexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var source = step.Settings.GetDictionaryJsonValue<string>("sourceIndexName");
        var dest = step.Settings.GetDictionaryJsonValue<string>("destIndexName");

        JsonNode request;
        // you can not use these settings to identify the to/from if you want
        // to also run a script.  You must use the RAW method...
        if (!String.IsNullOrWhiteSpace(source) && !String.IsNullOrWhiteSpace(dest))
        {
            request = new JsonObject
            {
                ["source"] = new JsonObject { ["index"] = source },
                // Keep the source documents' versions, so a copy never regresses a newer write.
                ["dest"] = new JsonObject { ["index"] = dest, ["version_type"] = "external" },
            };
        }
        else
        {
            // Data element must follow structure as per guidelines at link below
            // https://www.elastic.co/guide/en/elasticsearch/reference/current/docs-reindex.html
            if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

            request = JsonNode.Parse(Serialize(builder, step.Data))!;
            // Keep the source documents' versions unless the step says otherwise ('create' only
            // supports internal versioning, so older steps that use it are left as they are).
            if (request["dest"] is JsonObject destination && destination["version_type"] == null
                && destination["op_type"]?.GetValue<string>() != "create")
                destination["version_type"] = "external";
        }

        var started = await SendAsync(builder, HttpMethod.Post, "_reindex?wait_for_completion=false&timeout=5m", request.ToJsonString(),
            $"Failed to reindex '{source}' to '{dest}'.");
        var task = JsonNode.Parse(started.Body)?["task"]?.GetValue<string>();
        if (task == null) return;

        while (true)
        {
            var response = await builder.Client.RequestAsync(HttpMethod.Get, $"_tasks/{task}");
            if (!response.Success)
            {
                this.Failures++;
                builder.Logger.LogError(response.OriginalException, "Reindex task failure.  Error: {error}", response.Body);
                // Ignore 502 because we don't know why this is happening intermittently.
                if (response.HttpStatusCode != 502 && this.Failures >= builder.MigrationOptions.ReindexFailureLimit)
                    throw new InvalidOperationException($"Failed to reindex '{source}' to '{dest}'. {response.Body}", response.OriginalException);
                Thread.Sleep(builder.MigrationOptions.ReindexDelay);
                continue;
            }

            this.Failures = 0;
            var result = JsonNode.Parse(response.Body)!;
            if (result["completed"]?.GetValue<bool>() == true) return;
            builder.Logger.LogDebug("Reindex task not complete {count} of {total}", result["task"]?["status"]?["created"], result["task"]?["status"]?["total"]);
            Thread.Sleep(builder.MigrationOptions.ReindexDelay);
        }
    }

    private static async Task DeleteIndexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");

        await SendAsync(builder, HttpMethod.Delete, name, null, $"Failed to delete index '{name}'.");
    }

    private static async Task CreateOrUpdateAliasAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        var alias = step.Settings.GetDictionaryJsonValue<string>("aliasName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.aliasName'.");

        var data = step.Data == null ? null : Serialize(builder, step.Data);
        await SendAsync(builder, HttpMethod.Put, $"{name}/_alias/{alias}", data, $"Failed to create/update index '{name}' alias '{alias}'.");
    }

    private static async Task DeleteAliasAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        var alias = step.Settings.GetDictionaryJsonValue<string>("aliasName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.aliasName'.");

        await SendAsync(builder, HttpMethod.Delete, $"{name}/_alias/{alias}", null, $"Failed to delete index '{name}' alias '{alias}'.");
    }

    private static async Task CreateOrUpdatePipelineAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("pipelineName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.pipelineName'.");
        var data = step.Data == null ? null : Serialize(builder, step.Data);

        await SendAsync(builder, HttpMethod.Put, $"_ingest/pipeline/{name}", data, $"Failed to create/update pipeline '{name}'.");
    }

    private static async Task UpdateByQueryAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetDictionaryJsonValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

        await SendAsync(builder, HttpMethod.Post, $"{name}/_update_by_query", Serialize(builder, step.Data), $"Failed to update index '{name}' using query.");
    }
    #endregion
    #endregion
}
