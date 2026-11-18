using System.Text.Json;
using System.Text.Json.Nodes;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.DAL;
using TNO.DAL.Services;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_1013 class, provides a way to migration elastic to version 1.0.13.
/// Stores 'status' on the published index as the 'Published' label the indexing service writes.
/// Earlier copies stored the number 2. 'status' is a keyword field, so the two are different terms,
/// and a search for 'Published' missed every copied document.
/// Documents are copied rather than updated in place: most have external version 0, which
/// _update_by_query rejects, and it would raise the versions the indexing service compares against.
/// Only the published index changes.
///
/// Runs as four steps (Elastic__Step), or all of them in order:
/// - prepare: create the new index, ready for loading, so a second indexing service can write to
///   it while the existing one keeps writing to the live index;
/// - copy: Elasticsearch copies existing documents server-side with external projection revisions;
/// - verify: compare the document count with the live index and require every status to be the label;
/// - cutover: restore the index settings, move the alias atomically, and verify again.
/// The old index is retained.
/// </summary>
[Migration("1.0.13")]
public class Migration_1013 : TNOMigration
{
    #region Variables
    private const string PreviousVersion = "1.0.12";
    // Deletes are remembered this long while the index loads, so the copy cannot bring back
    // content the second indexing service deleted (cutover restores the index's own value).
    private const string LoadingGcDeletes = "48h";
    // The PostgreSQL advisory lock that keeps a second 1.0.13 run out while one is running.
    private const long LockKey = 1013001013;
    // The fewest documents the copy may differ by while writers keep running.
    private const long MinCountDifference = 1000;
    private readonly TNOContext _context;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_1013 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="context"></param>
    /// <param name="serializerOptions"></param>
    public Migration_1013(
        MigrationBuilder builder,
        IContentService contentService,
        TNOContext context,
        IOptions<JsonSerializerOptions> serializerOptions) : base(builder, contentService, serializerOptions)
    {
        _context = context;
    }
    #endregion

    #region Properties
    /// <inheritdoc />
    public override bool SupportsSteps => true;
    #endregion

    #region Methods
    /// <inheritdoc />
    protected override Task UpAsync(MigrationBuilder builder) => WithLockAsync(builder, () => RunStepsAsync(builder, builder.MigrationOptions.Step, rollback: false));

    /// <inheritdoc />
    protected override Task DownAsync(MigrationBuilder builder) => WithLockAsync(builder, () => RunStepsAsync(builder, MigrationStepName.All, rollback: true));

    private Task WithLockAsync(MigrationBuilder builder, Func<Task> action)
    {
        var throttle = builder.MigrationOptions.ReindexRequestsPerSecond;
        if (throttle != -1 && throttle <= 0)
            throw new InvalidOperationException("ReindexRequestsPerSecond must be positive or -1.");
        return DatabaseSession.RunLockedAsync(_context, LockKey, builder.Logger, action);
    }

    /// <summary>
    /// Run one step, or every step in order. Nothing users see changes until cutover moves the alias.
    /// Rolling back keeps the label: the number is the fault this migration corrects.
    /// </summary>
    private async Task RunStepsAsync(MigrationBuilder builder, MigrationStepName step, bool rollback)
    {
        var target = await GetTargetAsync(builder, rollback);
        if (step == MigrationStepName.All || step == MigrationStepName.Prepare) await PrepareAsync(builder, target, rollback, step == MigrationStepName.Prepare);
        if (step == MigrationStepName.All || step == MigrationStepName.Copy) await CopyAsync(builder, target);
        if (step == MigrationStepName.All || step == MigrationStepName.Verify) await VerifyStepAsync(builder, target);
        if (step == MigrationStepName.All || step == MigrationStepName.Cutover) await CutoverAsync(builder, target);
    }

    private async Task PrepareAsync(MigrationBuilder builder, MigrationTarget target, bool rollback, bool announce)
    {
        await CreateIndexAsync(builder, target, Path.Combine(rollback ? "rollback" : "indexes", "index-published.json"));
        await CopyDynamicMappingsAsync(builder, target);
        await new NativeReindex(builder).UpdateStateAsync(target.Index, state => state["prepared"] = true);

        if (announce)
            builder.Logger.LogWarning("Prepared the new index. Point the second indexing service at it, then run step 'copy':{newline}  PUBLISHED_INDEX (Elastic__PublishedIndex)={published}",
                Environment.NewLine, target.Index);
    }

    private static async Task CopyAsync(MigrationBuilder builder, MigrationTarget target)
    {
        await RequireStepAsync(builder, target, "prepared", MigrationStepName.Prepare);
        await new NativeReindex(builder).CopyAsync(target.Alias, target.Index, true, true, keepAnalysis: true);
    }

    private static async Task VerifyStepAsync(MigrationBuilder builder, MigrationTarget target)
    {
        await RequireStepAsync(builder, target, "complete", MigrationStepName.Copy);
        await VerifyAsync(builder, target);
        await new NativeReindex(builder).UpdateStateAsync(target.Index, state => state["verified"] = true);
    }

    private static async Task CutoverAsync(MigrationBuilder builder, MigrationTarget target)
    {
        await RequireStepAsync(builder, target, "verified", MigrationStepName.Verify);
        await RestoreSettingsAsync(builder, target);
        var previous = await SwitchAliasAsync(builder, target);
        await VerifyAsync(builder, target);

        foreach (var index in previous)
            builder.Logger.LogWarning("Kept index '{index}' (no longer behind the alias). Delete it once the migration is confirmed: DELETE /{index}", index, index);
    }

    #region Helpers
    /// <summary>
    /// The published alias, the versioned index this migration writes, and the cycle that owns it.
    /// </summary>
    private record MigrationTarget(string Alias, string Index, string Generation, string Direction);

    // Keeps the generation of the live indexes, so retries select the same destination, before and
    // after cutover.
    private async Task<MigrationTarget> GetTargetAsync(MigrationBuilder builder, bool rollback)
    {
        var native = new NativeReindex(builder);
        var direction = rollback ? "down" : "up";
        var alias = builder.MigrationOptions.PublishedIndex;
        var settings = (await native.ReadAsync($"{alias}/_settings")).AsObject();
        if (settings.Count != 1) throw new InvalidOperationException($"Expected one source index for '{alias}'.");
        var source = settings.Single();
        var meta = (await native.ReadAsync($"{source.Key}/_mapping"))[source.Key]?["mappings"]?["_meta"];
        var generation = meta?["owner"]?.GetValue<string>() == NativeReindex.Owner && meta?["direction"]?.GetValue<string>() == direction
            ? meta?["generation"]?.GetValue<string>()
            : null;
        generation ??= Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(
            $"{source.Value!["settings"]!["index"]!["uuid"]!.GetValue<string>()}|{direction}"))).ToLowerInvariant()[..24];
        var suffix = rollback ? $"_v{PreviousVersion}-rollback-from-{this.Version}-{generation}" : $"_v{this.Version}-{generation}";
        return new MigrationTarget(alias, $"{alias}{suffix}", generation, direction);
    }

    private static async Task<StringResponse> RequestAsync(MigrationBuilder builder, Elasticsearch.Net.HttpMethod method, string path, string? body = null, IRequestParameters? parameters = null)
    {
        parameters ??= new SearchRequestParameters();
        parameters.RequestConfiguration = new RequestConfiguration { ThrowExceptions = false };
        return await builder.Client.LowLevel.DoRequestAsync<StringResponse>(method, path, CancellationToken.None, body == null ? null : PostData.String(body), parameters);
    }

    private static InvalidOperationException Fail(MigrationBuilder builder, string message, StringResponse response)
    {
        builder.Logger.LogError(response.OriginalException, "{message}  Error: {error}", message, response.Body);
        return new InvalidOperationException($"{message} {response.Body}", response.OriginalException);
    }

    /// <summary>
    /// Create the index from a file in this migration's folder. An owned destination left by a failed run is retained for recovery.
    /// </summary>
    private async Task CreateIndexAsync(MigrationBuilder builder, MigrationTarget target, string file)
    {
        var native = new NativeReindex(builder);
        if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, target.Index)).HttpStatusCode == 200)
        {
            var meta = (await native.ReadAsync($"{target.Index}/_mapping"))[target.Index]?["mappings"]?["_meta"];
            if (meta?["owner"]?.GetValue<string>() != NativeReindex.Owner)
                throw new InvalidOperationException($"Index '{target.Index}' already exists without native migration metadata. Verify it is an unused partial destination and clean it up explicitly before retrying; nothing was deleted.");
            if (meta?["generation"]?.GetValue<string>() != target.Generation || meta?["direction"]?.GetValue<string>() != target.Direction)
                throw new InvalidOperationException($"Index '{target.Index}' belongs to a different migration cycle; nothing was deleted.");
            builder.Logger.LogInformation("Resuming retained destination {index}", target.Index);
            return;
        }
        var path = Path.Combine(builder.MigrationOptions.MigrationsPath, this.Version, file);
        var json = ReplaceIndexNames(builder, await File.ReadAllTextAsync(path));
        var step = JsonSerializer.Deserialize<MigrationStep>(json, builder.SerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize '{path}'");
        var data = JsonNode.Parse(JsonSerializer.Serialize(step.Data, builder.SerializerOptions))!;
        var state = new JsonObject { ["owner"] = NativeReindex.Owner, ["generation"] = target.Generation, ["direction"] = target.Direction };
        data["mappings"]!["_meta"] = state;
        var indexSettings = data["settings"]!["index"]!;
        // Preserve cluster-specific topology instead of hard-coding three replicas on Cloud.
        var original = (await native.ReadAsync($"{target.Alias}/_settings")).AsObject().First().Value!["settings"]!["index"]!;
        state["replicas"] = builder.MigrationOptions.NumberOfReplicas ?? Int32.Parse(original["number_of_replicas"]!.GetValue<string>());
        if (indexSettings["gc_deletes"] != null) state["gcDeletes"] = indexSettings["gc_deletes"]!.DeepClone();
        indexSettings["number_of_shards"] = builder.MigrationOptions.NumberOfShards ?? Int32.Parse(original["number_of_shards"]!.GetValue<string>());
        // Load without replicas or refreshes; cutover restores them before the alias moves.
        indexSettings["number_of_replicas"] = 0;
        indexSettings["refresh_interval"] = "-1";
        indexSettings["gc_deletes"] = LoadingGcDeletes;
        var response = await builder.Client.LowLevel.Indices.CreateAsync<StringResponse>(target.Index, PostData.String(data.ToJsonString()));
        if (!response.Success) throw Fail(builder, $"Failed to create index '{target.Index}'.", response);
    }

    /// <summary>
    /// Map the fields the old index mapped dynamically (source, mediaType, owner, series, ...) in the
    /// new index before any document is written, so they keep their types.
    /// </summary>
    private static async Task CopyDynamicMappingsAsync(MigrationBuilder builder, MigrationTarget target)
    {
        var existing = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"{target.Alias}/_mapping");
        if (!existing.Success) throw Fail(builder, $"Failed to read the mapping of '{target.Alias}'.", existing);
        var created = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"{target.Index}/_mapping");
        if (!created.Success) throw Fail(builder, $"Failed to read the mapping of '{target.Index}'.", created);

        var oldProperties = JsonNode.Parse(existing.Body)!.AsObject().First().Value?["mappings"]?["properties"]?.AsObject();
        var newProperties = JsonNode.Parse(created.Body)!.AsObject().First().Value?["mappings"]?["properties"]?.AsObject();
        if (oldProperties == null) return;
        var missing = new JsonObject();
        foreach (var (name, mapping) in oldProperties)
            if (newProperties == null || !newProperties.ContainsKey(name)) missing[name] = mapping?.DeepClone();
        if (missing.Count == 0) return;

        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{target.Index}/_mapping", new JsonObject { ["properties"] = missing }.ToJsonString());
        if (!response.Success) throw Fail(builder, $"Failed to copy the dynamic mappings of '{target.Alias}' to '{target.Index}'.", response);
        builder.Logger.LogInformation("Copied {count} field mapping(s) from '{alias}' to '{index}': {fields}", missing.Count, target.Alias, target.Index, String.Join(", ", missing.Select(p => p.Key)));
    }

    /// <summary>
    /// The index must exist and have completed the earlier step.
    /// </summary>
    private static async Task RequireStepAsync(MigrationBuilder builder, MigrationTarget target, string key, MigrationStepName step)
    {
        var name = step.ToString().ToLowerInvariant();
        if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, target.Index)).HttpStatusCode == 404)
            throw new InvalidOperationException($"Index '{target.Index}' does not exist. Run step '{name}' first; nothing was changed.");
        var state = await new NativeReindex(builder).ReadStateAsync(target.Index);
        if (state[key]?.GetValue<bool>() != true)
            throw new InvalidOperationException($"Index '{target.Index}' has not completed step '{name}'. Run it first; nothing was changed.");
    }

    /// <summary>
    /// Give the loaded index back its replicas, refreshes and delete retention, then wait until its
    /// replicas are as healthy as the index it replaces.
    /// </summary>
    private static async Task RestoreSettingsAsync(MigrationBuilder builder, MigrationTarget target)
    {
        var state = await new NativeReindex(builder).ReadStateAsync(target.Index);
        var settings = new JsonObject { ["index.refresh_interval"] = null };
        if (state["replicas"] != null)
        {
            settings["index.number_of_replicas"] = state["replicas"]!.DeepClone();
            settings["index.gc_deletes"] = state["gcDeletes"]?.DeepClone();
        }
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{target.Index}/_settings", settings.ToJsonString());
        if (!response.Success) throw Fail(builder, $"Failed to restore the settings of '{target.Index}'; the alias is unchanged.", response);

        var current = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_cluster/health/{target.Alias}");
        if (!current.Success) throw Fail(builder, $"Failed to read the health of '{target.Alias}'.", current);
        var wanted = JsonNode.Parse(current.Body)?["status"]?.GetValue<string>() == "green" ? Elasticsearch.Net.WaitForStatus.Green : Elasticsearch.Net.WaitForStatus.Yellow;
        for (var minute = 1; ; minute++)
        {
            var health = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_cluster/health/{target.Index}",
                parameters: new Elasticsearch.Net.Specification.ClusterApi.ClusterHealthRequestParameters
                {
                    WaitForStatus = wanted,
                    WaitForNoInitializingShards = true,
                    Timeout = TimeSpan.FromMinutes(1),
                });
            var body = String.IsNullOrEmpty(health.Body) ? null : JsonNode.Parse(health.Body);
            if (body?["timed_out"]?.GetValue<bool>() == false) break;
            if (minute >= 60)
                throw new InvalidOperationException($"The replicas of {target.Index} are not {wanted.ToString().ToLowerInvariant()} after an hour; the alias is unchanged. Rerun cutover once the cluster is healthy.");
            builder.Logger.LogInformation("Waiting for the replicas of {index}: {status}, {initializing} initializing, {unassigned} unassigned", target.Index, body?["status"], body?["initializing_shards"], body?["unassigned_shards"]);
        }
        builder.Logger.LogInformation("Restored the settings of {index}", target.Index);
    }

    /// <summary>
    /// Compare the new index with the index behind the alias, and require every status to be the
    /// label. Writers are not paused, so the counts may differ by what arrived during the comparison.
    /// </summary>
    private static async Task VerifyAsync(MigrationBuilder builder, MigrationTarget target)
    {
        await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{target.Index}/_refresh");
        var source = await CountAsync(builder, target.Alias, null);
        var destination = await CountAsync(builder, target.Index, null);
        var allowed = Math.Max(MinCountDifference, source / 1000);
        if (Math.Abs(source - destination) > allowed)
            throw new InvalidOperationException($"Index '{target.Index}' has {destination:N0} document(s) and '{target.Alias}' has {source:N0}, more than the {allowed:N0} allowed to differ. Investigate before rerunning; nothing was changed.");

        var wrong = await CountAsync(builder, target.Index, """{"query":{"bool":{"must_not":[{"term":{"status":"Published"}}]}}}""");
        if (wrong > 0)
            throw new InvalidOperationException($"Index '{target.Index}' has {wrong:N0} document(s) whose status is not 'Published'. Investigate before rerunning; nothing was changed.");
        builder.Logger.LogInformation("Verified '{index}': {destination:N0} document(s), '{alias}' has {source:N0}, every status is 'Published'.", target.Index, destination, target.Alias, source);
    }

    private static async Task<long> CountAsync(MigrationBuilder builder, string index, string? body)
    {
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_count", body);
        if (!response.Success) throw Fail(builder, $"Failed to count '{index}'.", response);
        return JsonNode.Parse(response.Body)!["count"]!.GetValue<long>();
    }

    /// <returns>The indexes the alias pointed to before (kept).</returns>
    private static async Task<List<string>> SwitchAliasAsync(MigrationBuilder builder, MigrationTarget target)
    {
        var alias = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_alias/{target.Alias}");
        if (!alias.Success) throw Fail(builder, $"'{target.Alias}' must be an alias; it is unchanged.", alias);
        var previous = JsonNode.Parse(alias.Body)!.AsObject().Select(p => p.Key).Where(n => n != target.Index).ToList();
        var actions = new JsonArray();
        foreach (var index in previous)
            actions.Add(new JsonObject { ["remove"] = new JsonObject { ["index"] = index, ["alias"] = target.Alias } });
        actions.Add(new JsonObject { ["add"] = new JsonObject { ["index"] = target.Index, ["alias"] = target.Alias } });
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, "_aliases", new JsonObject { ["actions"] = actions }.ToJsonString());
        if (!response.Success) throw Fail(builder, "Failed to move the alias; it is unchanged.", response);
        builder.Logger.LogInformation("Moved the alias: {alias} -> {index}", target.Alias, target.Index);
        return previous;
    }
    #endregion
    #endregion
}
