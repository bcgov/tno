using System.Reflection;
using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Nest;
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
        _builder.Logger.LogInformation("Applying migration {version}", this.Version);
        await RunScriptsAsync(_builder, $"up{Path.DirectorySeparatorChar}pre");
        await UpAsync(_builder);
        await RunScriptsAsync(_builder, "up");
        await RunScriptsAsync(_builder, $"up{Path.DirectorySeparatorChar}post");

        var version = new MigrationVersion(this.Version, "");
        var response = await _builder.Client.IndexAsync(version, id => id.Index(_builder.MigrationOptions.MigrationIndex));
        if (!response.IsValid)
        {
            _builder.Logger.LogError(response.OriginalException, "Failed to add migration {version} to index.  Error: {error}", this.Version, response.ServerError);
            throw response.OriginalException;
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

        var response = await _builder.Client.DeleteAsync<MigrationVersion>(this.Version, dd => dd.Index(_builder.MigrationOptions.MigrationIndex));
        if (!response.IsValid)
        {
            var error = response.OriginalException ?? new ElasticsearchClientException("Failed to perform delete");
            _builder.Logger.LogError(error, "Failed to remove migration {version} from index.  Error: {error}", this.Version, response.ServerError);
            throw error;
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
                    using var stream = File.OpenRead(file);
                    var step = JsonSerializer.Deserialize<MigrationStep>(stream, builder.SerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize '{file}'");
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
                }
                catch (ElasticsearchClientException ex)
                {
                    var body = ex.Response.RequestBodyInBytes != null ? System.Text.Encoding.UTF8.GetString(ex.Response.ResponseBodyInBytes) : "";
                    builder.Logger.LogError(ex, "Failed to run step '{file}'.  Error: {error}", file, body);
                    throw;
                }
            }
        }
    }

    #region Elastic actions
    private static async Task CreateIndexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

        var response = await builder.Client.LowLevel.Indices.CreateAsync<StringResponse>(name, PostData.String(JsonSerializer.Serialize(step.Data, builder.SerializerOptions)));
        if (!response.Success)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to create index '{index}'.  Error: {error}", name, response.Body);
            throw response.OriginalException;
        }
    }

    private static async Task UpdateMappingAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

        var response = await builder.Client.LowLevel.Indices.PutMappingAsync<StringResponse>(name, PostData.String(JsonSerializer.Serialize(step.Data, builder.SerializerOptions)));
        if (!response.Success)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to update index '{index}' mapping.  Error: {error}", name, response.Body);
            throw response.OriginalException;
        }
    }

    private async Task ReindexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var source = step.Settings.GetConfigurationValue<string>("sourceIndexName");
        var dest = step.Settings.GetConfigurationValue<string>("destIndexName");

        TaskId? task = null;

        if (!String.IsNullOrWhiteSpace(source) && !String.IsNullOrWhiteSpace(dest))
        {
            var response = await builder.Client.ReindexOnServerAsync(s => s
                .Source(s => s.Index(source))
                .Destination(s => s.Index(dest))
                .WaitForCompletion(false)
                .Timeout(new TimeSpan(0, 5, 0)));
            if (!response.IsValid)
            {
                builder.Logger.LogError(response.OriginalException, "Failed to reindex '{source}' to '{dest}'.  Error: {error}", source, dest, response.ServerError);
                throw response.OriginalException;
            }

            task = response.Task;
        }
        else
        {
            if (step.Data == null) throw new InvalidOperationException($"Migration step '{path}' is missing required property 'data'.");

            var data = PostData.String(JsonSerializer.Serialize(step.Data, builder.SerializerOptions));
            var response = await builder.Client.LowLevel.ReindexOnServerAsync<ReindexOnServerResponse>(data, new ReindexOnServerRequestParameters()
            {
                Timeout = new TimeSpan(0, 5, 0),
                WaitForCompletion = false
            });
            if (!response.IsValid)
            {
                builder.Logger.LogError(response.OriginalException, "Failed to reindex '{source}' to '{dest}'.  Error: {error}", source, dest, response.ServerError.Error.Reason);
                throw response.OriginalException;
            }
            task = response.Task;
        }

        if (task != null)
        {
            var completed = false;
            while (!completed)
            {
                try
                {
                    var response = await builder.Client.Tasks.GetTaskAsync(task);
                    if (!response.IsValid)
                    {
                        if (response.ServerError.Status == 502)
                        {
                            builder.Logger.LogError(response.OriginalException, "Reindex task 502 gateway error");
                            this.Failures++;
                            if (this.Failures >= builder.MigrationOptions.ReindexFailureLimit)
                                throw response.OriginalException;
                        }
                        else
                        {
                            builder.Logger.LogError(response.OriginalException, "Failed to reindex '{source}' to '{dest}'.  Error: {error}", source, dest, response.ServerError.Error.Reason);
                            throw response.OriginalException;
                        }
                    }

                    completed = response.Completed;
                    this.Failures = 0;
                    if (!completed)
                    {
                        builder.Logger.LogDebug("Reindex task not complete {count} of {total}", response.Task.Status.Created, response.Task.Status.Total);
                        Thread.Sleep(builder.MigrationOptions.ReindexDelay);
                    }
                }
                catch (ElasticsearchClientException ex)
                {
                    this.Failures++;
                    builder.Logger.LogError(ex, "Reindex task failure");
                    // Ignore 502 because we don't know why this is happening intermittently.
                    if (ex.Response.HttpStatusCode != 502)
                    {
                        if (this.Failures >= builder.MigrationOptions.ReindexFailureLimit)
                            throw ex;
                    }
                    Thread.Sleep(builder.MigrationOptions.ReindexDelay);
                }
            }
        }
    }

    private static async Task DeleteIndexAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");

        var response = await builder.Client.Indices.DeleteAsync(name);
        if (!response.IsValid)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to delete index '{index}'.  Error: {error}", name, response.ServerError);
            throw response.OriginalException;
        }
    }

    private static async Task CreateOrUpdateAliasAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        var alias = step.Settings.GetConfigurationValue<string>("aliasName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.aliasName'.");

        var data = step.Data == null ? PostData.Empty : PostData.String(JsonSerializer.Serialize(step.Data, builder.SerializerOptions));
        var response = await builder.Client.LowLevel.Indices.PutAliasAsync<StringResponse>(name, alias, data);
        if (!response.Success)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to create/update index '{index}' alias '{alias}'.  Error: {error}", name, alias, response.Body);
            throw response.OriginalException;
        }
    }

    private static async Task DeleteAliasAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("indexName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.indexName'.");
        var alias = step.Settings.GetConfigurationValue<string>("aliasName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.aliasName'.");

        var response = await builder.Client.Indices.DeleteAliasAsync(name, alias);
        if (!response.IsValid)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to delete index '{index}' alias '{alias}'.  Error: {error}", name, alias, response.ServerError);
            throw response.OriginalException;
        }
    }

    private static async Task CreateOrUpdatePipelineAsync(MigrationBuilder builder, MigrationStep step, string path)
    {
        var name = step.Settings.GetConfigurationValue<string>("pipelineName") ?? throw new InvalidOperationException($"Migration step '{path}' is missing required property 'settings.pipelineName'.");
        var data = step.Data == null ? PostData.Empty : PostData.String(JsonSerializer.Serialize(step.Data, builder.SerializerOptions));

        var response = await builder.Client.LowLevel.Ingest.PutPipelineAsync<StringResponse>(name, data);
        if (!response.Success)
        {
            builder.Logger.LogError(response.OriginalException, "Failed to create/update pipeline '{index}'.  Error: {error}", name, response.Body);
            throw response.OriginalException;
        }
    }
    #endregion
    #endregion
}
