using System.Text.Json;
using System.Text.Json.Nodes;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_1012 class, provides a way to migration elastic to version 1.0.12.
/// Adds the 'byline.keyword' and 'otherSeries.keyword' sub-fields so charts can aggregate on them.
/// Adding a sub-field only applies to documents indexed afterwards, and a chart over a multi-year
/// range needs the value on every document, so the indexes are copied.
/// Also stops indexing 'versions': it is keyed by user ID, so every user who edits a story added
/// fields towards the index's field limit, and nothing searches it. Documents still return it.
///
/// Runs as four steps (Elastic__Step), or all of them in order:
/// - prepare: create the new indexes, ready for loading, so a second indexing service can write to
///   them while the existing one keeps writing to the live indexes;
/// - copy: Elasticsearch copies existing documents server-side with external projection revisions;
/// - verify: compare document counts with the live indexes and check the keyword values exist;
/// - cutover: restore the index settings, move the aliases atomically, and verify again.
/// Old indexes are retained; an existing unmanaged destination is never deleted. The evidence
/// index is not part of this migration.
/// </summary>
[Migration("1.0.12")]
public class Migration_1012 : TNOMigration
{
    #region Variables
    private const string PreviousVersion = "1.0.11";
    // Deletes are remembered this long while the indexes load, so the copy cannot bring back
    // content the second indexing service deleted (cutover restores the index's own value).
    private const string LoadingGcDeletes = "48h";
    // The PostgreSQL advisory lock that keeps a second 1.0.12 run out while one is running.
    private const long LockKey = 1012001012;
    // The fewest documents the copies may differ by while writers keep running.
    private const long MinCountDifference = 1000;
    private readonly TNOContext _context;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_1012 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="context"></param>
    /// <param name="serializerOptions"></param>
    public Migration_1012(
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
        ValidateThrottle(builder);
        return DatabaseSession.RunLockedAsync(_context, LockKey, builder.Logger, action);
    }

    /// <summary>
    /// Run one step, or every step in order. Upgrading copies into the 1.0.12 indexes; rolling back
    /// copies into indexes of the 1.0.11 shape and keeps the 1.0.12 indexes.
    /// Nothing users see changes until cutover moves the aliases.
    /// </summary>
    private async Task RunStepsAsync(MigrationBuilder builder, MigrationStepName step, bool rollback)
    {
        var targets = await GetCycleTargetsAsync(builder, rollback);
        if (step == MigrationStepName.All || step == MigrationStepName.Prepare) await PrepareAsync(builder, targets, rollback, step == MigrationStepName.Prepare);
        if (step == MigrationStepName.All || step == MigrationStepName.Copy) await CopyAsync(builder, targets);
        if (step == MigrationStepName.All || step == MigrationStepName.Verify) await VerifyStepAsync(builder, targets);
        if (step == MigrationStepName.All || step == MigrationStepName.Cutover) await CutoverAsync(builder, targets);
    }

    /// <summary>
    /// Create the new indexes with the mappings the old indexes gained dynamically, ready for loading.
    /// </summary>
    private async Task PrepareAsync(MigrationBuilder builder, MigrationTargets targets, bool rollback, bool announce)
    {
        var folder = rollback ? "rollback" : "indexes";
        await CreateIndexFromFileAsync(builder, targets.Published.Index, Path.Combine(folder, "index-published.json"), targets, targets.Published.Alias);
        await CreateIndexFromFileAsync(builder, targets.Content.Index, Path.Combine(folder, "index-content.json"), targets);
        await CopyDynamicMappingsAsync(builder, targets.Published);
        await CopyDynamicMappingsAsync(builder, targets.Content);

        var native = new NativeReindex(builder);
        foreach (var target in targets.All)
            await native.UpdateStateAsync(target.Index, state => state["prepared"] = true);

        if (announce)
            builder.Logger.LogWarning("Prepared the new indexes. Point the second indexing service at them, then run step 'copy':{newline}  CONTENT_INDEX (Elastic__ContentIndex)={content}{newline}  PUBLISHED_INDEX (Elastic__PublishedIndex)={published}",
                Environment.NewLine, targets.Content.Index, Environment.NewLine, targets.Published.Index);
    }

    /// <summary>
    /// Copy the existing documents into the prepared indexes, analyses included.
    /// </summary>
    private static async Task CopyAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        await RequireStepAsync(builder, targets.All, "prepared", MigrationStepName.Prepare);
        var native = new NativeReindex(builder);
        await native.CopyAsync(targets.Content.Alias, targets.Content.Index, false, true, keepAnalysis: true);
        await native.CopyAsync(targets.Published.Alias, targets.Published.Index, true, true, keepAnalysis: true);
    }

    /// <summary>
    /// Compare every new index with the live index behind its alias.
    /// </summary>
    private static async Task VerifyStepAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        await RequireStepAsync(builder, targets.All, "complete", MigrationStepName.Copy);
        await VerifyAsync(builder, targets);
        var native = new NativeReindex(builder);
        foreach (var target in targets.All)
            await native.UpdateStateAsync(target.Index, state => state["verified"] = true);
    }

    /// <summary>
    /// Restore the index settings, move the aliases, and verify again: a restart after cutover must
    /// validate again before recording migration completion.
    /// </summary>
    private static async Task CutoverAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        await RequireStepAsync(builder, targets.All, "verified", MigrationStepName.Verify);
        await RestoreSettingsAsync(builder, targets);
        var previous = await SwitchAliasesAsync(builder, targets);
        await VerifyAsync(builder, targets);

        foreach (var index in previous.Distinct())
            builder.Logger.LogWarning("Kept index '{index}' (no longer behind an alias). Delete it once the migration is confirmed: DELETE /{index}", index, index);
    }

    #region Helpers
    /// <summary>
    /// An alias and the versioned index behind it.
    /// </summary>
    private record struct IndexTarget(string Alias, string Index);

    /// <summary>
    /// The indexes a rebuild writes.
    /// </summary>
    private record MigrationTargets(IndexTarget Published, IndexTarget Content)
    {
        public string Generation { get; init; } = "";
        public string Direction { get; init; } = "";
        public IndexTarget[] All => new[] { Published, Content };
    }

    private static MigrationTargets GetTargets(MigrationBuilder builder, string suffix)
    {
        var options = builder.MigrationOptions;
        return new MigrationTargets(
            new IndexTarget(options.PublishedIndex, $"{options.PublishedIndex}{suffix}"),
            new IndexTarget(options.ContentIndex, $"{options.ContentIndex}{suffix}"));
    }

    // Source UUIDs identify a cycle. Retries before cutover select the same destinations;
    // retries after atomic cutover recover the generation from both live primary indexes.
    private async Task<MigrationTargets> GetCycleTargetsAsync(MigrationBuilder builder, bool rollback)
    {
        var native = new NativeReindex(builder);
        var direction = rollback ? "down" : "up";
        var aliases = new[] { builder.MigrationOptions.ContentIndex, builder.MigrationOptions.PublishedIndex };
        var uuids = new List<string>();
        var generations = new List<string?>();
        foreach (var alias in aliases)
        {
            var settings = (await native.ReadAsync($"{alias}/_settings")).AsObject();
            if (settings.Count != 1) throw new InvalidOperationException($"Expected one source index for '{alias}'.");
            var source = settings.Single();
            uuids.Add(source.Value!["settings"]!["index"]!["uuid"]!.GetValue<string>());
            var mapping = await native.ReadAsync($"{source.Key}/_mapping");
            var meta = mapping[source.Key]?["mappings"]?["_meta"];
            generations.Add(meta?["owner"]?.GetValue<string>() == NativeReindex.Owner &&
                meta?["direction"]?.GetValue<string>() == direction ? meta?["generation"]?.GetValue<string>() : null);
        }
        if (generations.Any(g => g != null) &&
            (generations[0] == null || generations[0] != generations[1]))
            throw new InvalidOperationException("Primary aliases have inconsistent migration generations; inspect them before retrying.");
        var generation = generations[0] ?? Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(String.Join("|", uuids.Append(direction))))).ToLowerInvariant()[..24];
        var suffix = rollback ? $"_v{PreviousVersion}-rollback-from-{this.Version}-{generation}" : $"_v{this.Version}-{generation}";
        return GetTargets(builder, suffix) with { Generation = generation, Direction = direction };
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
    /// Create an index from a step file in this migration's folder (not run as a step). An owned destination left by a failed run is retained for recovery.
    /// </summary>
    private async Task CreateIndexFromFileAsync(MigrationBuilder builder, string? index, string file, MigrationTargets targets, string? sourceAlias = null)
    {
        if (index == null) return;
        if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, index)).HttpStatusCode == 200)
        {
            var mapping = await new NativeReindex(builder).ReadAsync($"{index}/_mapping");
            if (mapping[index]?["mappings"]?["_meta"]?["owner"]?.GetValue<string>() != NativeReindex.Owner)
                throw new InvalidOperationException($"Index '{index}' already exists without native migration metadata. Verify it is an unused partial destination and clean it up explicitly before retrying; nothing was deleted.");
            var meta = mapping[index]?["mappings"]?["_meta"];
            if (meta?["generation"]?.GetValue<string>() != targets.Generation ||
                meta?["direction"]?.GetValue<string>() != targets.Direction)
                throw new InvalidOperationException($"Index '{index}' belongs to a different migration cycle; nothing was deleted.");
            builder.Logger.LogInformation("Resuming retained destination {index}", index);
            return;
        }
        var path = Path.Combine(builder.MigrationOptions.MigrationsPath, this.Version, file);
        var json = ReplaceIndexNames(builder, await File.ReadAllTextAsync(path));
        var step = JsonSerializer.Deserialize<MigrationStep>(json, builder.SerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize '{path}'");
        var data = JsonNode.Parse(JsonSerializer.Serialize(step.Data, builder.SerializerOptions))!;
        data["mappings"]!["_meta"] = new JsonObject { ["owner"] = NativeReindex.Owner, ["generation"] = targets.Generation, ["direction"] = targets.Direction };
        var indexSettings = data["settings"]!["index"]!;
        // Preserve cluster-specific topology instead of hard-coding three replicas on Cloud.
        sourceAlias ??= builder.MigrationOptions.ContentIndex;
        var sourceSettings = await new NativeReindex(builder).ReadAsync($"{sourceAlias}/_settings");
        var original = sourceSettings.AsObject().First().Value!["settings"]!["index"]!;
        var state = data["mappings"]!["_meta"]!.AsObject();
        state["replicas"] = builder.MigrationOptions.NumberOfReplicas ?? Int32.Parse(original["number_of_replicas"]!.GetValue<string>());
        if (indexSettings["gc_deletes"] != null) state["gcDeletes"] = indexSettings["gc_deletes"]!.DeepClone();
        indexSettings["number_of_shards"] = builder.MigrationOptions.NumberOfShards ?? Int32.Parse(original["number_of_shards"]!.GetValue<string>());
        // Load without replicas or refreshes; cutover restores them before the aliases move.
        indexSettings["number_of_replicas"] = 0;
        indexSettings["refresh_interval"] = "-1";
        indexSettings["gc_deletes"] = LoadingGcDeletes;
        var response = await builder.Client.LowLevel.Indices.CreateAsync<StringResponse>(index, PostData.String(data.ToJsonString()));
        if (!response.Success) throw Fail(builder, $"Failed to create index '{index}'.", response);
    }

    /// <summary>
    /// Map the fields the old index mapped dynamically (source, mediaType, owner, series, ...) in the
    /// new index before any document is written, so they keep their types.
    /// </summary>
    private static async Task CopyDynamicMappingsAsync(MigrationBuilder builder, IndexTarget target)
    {
        var existing = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"{target.Alias}/_mapping");
        if (existing.HttpStatusCode == 404) return;
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
    /// Every index must exist and have completed the earlier step.
    /// </summary>
    private static async Task RequireStepAsync(MigrationBuilder builder, IEnumerable<IndexTarget> targets, string key, MigrationStepName step)
    {
        var name = step.ToString().ToLowerInvariant();
        var native = new NativeReindex(builder);
        foreach (var target in targets)
        {
            if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, target.Index)).HttpStatusCode == 404)
                throw new InvalidOperationException($"Index '{target.Index}' does not exist. Run step '{name}' first; nothing was changed.");
            var state = await native.ReadStateAsync(target.Index);
            if (state[key]?.GetValue<bool>() != true)
                throw new InvalidOperationException($"Index '{target.Index}' has not completed step '{name}'. Run it first; nothing was changed.");
        }
    }

    /// <summary>
    /// Give the loaded indexes back their replicas, refreshes and delete retention, then wait until
    /// their replicas are as healthy as the indexes they replace.
    /// </summary>
    private static async Task RestoreSettingsAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        var native = new NativeReindex(builder);
        foreach (var target in targets.All)
        {
            var state = await native.ReadStateAsync(target.Index);
            var settings = new JsonObject { ["index.refresh_interval"] = null };
            if (state["replicas"] != null)
            {
                settings["index.number_of_replicas"] = state["replicas"]!.DeepClone();
                settings["index.gc_deletes"] = state["gcDeletes"]?.DeepClone();
            }
            var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{target.Index}/_settings", settings.ToJsonString());
            if (!response.Success) throw Fail(builder, $"Failed to restore the settings of '{target.Index}'; the aliases are unchanged.", response);
        }

        var current = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_cluster/health/{targets.Content.Alias}");
        if (!current.Success) throw Fail(builder, $"Failed to read the health of '{targets.Content.Alias}'.", current);
        var wanted = JsonNode.Parse(current.Body)?["status"]?.GetValue<string>() == "green" ? Elasticsearch.Net.WaitForStatus.Green : Elasticsearch.Net.WaitForStatus.Yellow;
        var indexes = String.Join(",", targets.All.Select(t => t.Index));
        for (var minute = 1; ; minute++)
        {
            var health = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_cluster/health/{indexes}",
                parameters: new Elasticsearch.Net.Specification.ClusterApi.ClusterHealthRequestParameters
                {
                    WaitForStatus = wanted,
                    WaitForNoInitializingShards = true,
                    Timeout = TimeSpan.FromMinutes(1),
                });
            var body = String.IsNullOrEmpty(health.Body) ? null : JsonNode.Parse(health.Body);
            if (body?["timed_out"]?.GetValue<bool>() == false) break;
            if (minute >= 60)
                throw new InvalidOperationException($"The replicas of {indexes} are not {wanted.ToString().ToLowerInvariant()} after an hour; the aliases are unchanged. Rerun cutover once the cluster is healthy.");
            builder.Logger.LogInformation("Waiting for the replicas of {indexes}: {status}, {initializing} initializing, {unassigned} unassigned", indexes, body?["status"], body?["initializing_shards"], body?["unassigned_shards"]);
        }
        builder.Logger.LogInformation("Restored the settings of {indexes}", indexes);
    }

    private static void ValidateThrottle(MigrationBuilder builder)
    {
        var throttle = builder.MigrationOptions.ReindexRequestsPerSecond;
        if (throttle != -1 && throttle <= 0)
            throw new InvalidOperationException("ReindexRequestsPerSecond must be positive or -1.");
    }

    /// <summary>
    /// Compare each new index with the index behind its alias. Writers are not paused, so the
    /// counts may differ by what arrived during the comparison, but no more than that.
    /// </summary>
    private static async Task VerifyAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        foreach (var target in targets.All)
        {
            await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{target.Index}/_refresh");
            var source = await CountAsync(builder, target.Alias, null);
            var destination = await CountAsync(builder, target.Index, null);
            var allowed = Math.Max(MinCountDifference, source / 1000);
            if (Math.Abs(source - destination) > allowed)
                throw new InvalidOperationException($"Index '{target.Index}' has {destination:N0} document(s) and '{target.Alias}' has {source:N0}, more than the {allowed:N0} allowed to differ. Investigate before rerunning; nothing was changed.");

            // A document with a byline must have the keyword value, otherwise charts would lose it.
            var bylines = await CountAsync(builder, target.Index, "byline");
            var keywords = await CountAsync(builder, target.Index, "byline.keyword");
            var missing = await SampleMissingKeywordAsync(builder, target.Index);
            if (bylines > 0 && keywords == 0)
                throw new InvalidOperationException($"Index '{target.Index}' has {bylines:N0} document(s) with a byline, and none with 'byline.keyword'.");
            builder.Logger.LogInformation("Verified '{index}': {destination:N0} document(s), '{alias}' has {source:N0}. byline:{bylines:N0} byline.keyword:{keywords:N0} (longer than the keyword limit:{missing})", target.Index, destination, target.Alias, source, bylines, keywords, missing);
        }
    }

    private static async Task<long> CountAsync(MigrationBuilder builder, string index, string? field)
    {
        var body = field == null ? null : new JsonObject { ["query"] = new JsonObject { ["exists"] = new JsonObject { ["field"] = field } } }.ToJsonString();
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_count", body);
        if (!response.Success) throw Fail(builder, $"Failed to count '{index}'.", response);
        return JsonNode.Parse(response.Body)!["count"]!.GetValue<long>();
    }

    /// <summary>
    /// Count the documents with a byline and no keyword value. They are expected only past 'ignore_above'.
    /// </summary>
    private static async Task<long> SampleMissingKeywordAsync(MigrationBuilder builder, string index)
    {
        var body = JsonNode.Parse("""{"query":{"bool":{"must":[{"exists":{"field":"byline"}}],"must_not":[{"exists":{"field":"byline.keyword"}}]}}}""")!.ToJsonString();
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_count", body);
        if (!response.Success) throw Fail(builder, $"Failed to count '{index}'.", response);
        return JsonNode.Parse(response.Body)!["count"]!.GetValue<long>();
    }

    /// index (a cluster that never used versioned indexes) is first cloned to a backup index, then
    /// replaced by the alias.
    /// </summary>
    /// <returns>The indexes the aliases pointed to before (kept).</returns>
    private static async Task<List<string>> SwitchAliasesAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        var previous = new List<string>();
        var actions = new JsonArray();
        var blocked = new List<string>();
        try
        {
            await AddAliasActionsAsync(builder, targets, previous, actions, blocked);
            var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, "_aliases", new JsonObject { ["actions"] = actions }.ToJsonString());
            if (!response.Success) throw Fail(builder, "Failed to move the aliases; they are unchanged.", response);
        }
        catch
        {
            // Indexes blocked for their backup take writes again.
            foreach (var index in blocked)
                await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{index}/_settings", new JsonObject { ["index.blocks.write"] = null }.ToJsonString());
            throw;
        }
        builder.Logger.LogInformation("Moved the aliases: {aliases}", String.Join(", ", targets.All.Select(t => $"{t.Alias} -> {t.Index}")));
        return previous;
    }

    private static async Task AddAliasActionsAsync(MigrationBuilder builder, MigrationTargets targets, List<string> previous, JsonArray actions, List<string> blocked)
    {
        foreach (var target in targets.All)
        {
            var aliasResponse = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET, $"_alias/{target.Alias}");
            if (aliasResponse.Success && aliasResponse.HttpStatusCode == 200)
            {
                foreach (var index in JsonNode.Parse(aliasResponse.Body)!.AsObject().Select(p => p.Key).Where(n => n != target.Index))
                {
                    previous.Add(index);
                    actions.Add(new JsonObject { ["remove"] = new JsonObject { ["index"] = index, ["alias"] = target.Alias } });
                }
            }
            else if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, target.Alias)).HttpStatusCode == 200)
            {
                blocked.Add(target.Alias);
                previous.Add(await BackupConcreteIndexAsync(builder, target.Alias));
                actions.Add(new JsonObject { ["remove_index"] = new JsonObject { ["index"] = target.Alias } });
            }
            actions.Add(new JsonObject { ["add"] = new JsonObject { ["index"] = target.Index, ["alias"] = target.Alias } });
        }
    }

    /// <summary>
    /// Clone a concrete index to '{name}-backup-{time}' (writes are blocked while it is cloned).
    /// </summary>
    /// <returns>The backup index.</returns>
    private static async Task<string> BackupConcreteIndexAsync(MigrationBuilder builder, string index)
    {
        var backup = $"{index}-backup-{DateTime.UtcNow:yyyyMMddHHmmss}";
        var settings = await new NativeReindex(builder).ReadAsync($"{index}/_settings");
        var sourceSettings = settings[index]!["settings"]!["index"]!;
        var cloneSettings = new JsonObject
        {
            ["index.number_of_replicas"] = sourceSettings["number_of_replicas"]!.DeepClone(),
        };
        if (sourceSettings["auto_expand_replicas"] != null)
            cloneSettings["index.auto_expand_replicas"] = sourceSettings["auto_expand_replicas"]!.DeepClone();
        // The dedicated block API waits for in-flight writes to finish before cloning.
        var block = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{index}/_block/write");
        if (!block.Success) throw Fail(builder, $"Failed to block writes to '{index}' for its backup.", block);
        var clone = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_clone/{backup}",
            new JsonObject { ["settings"] = cloneSettings }.ToJsonString());
        if (!clone.Success) throw Fail(builder, $"Failed to back up '{index}'; nothing was changed.", clone);
        var health = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.GET,
            $"_cluster/health/{backup}", parameters: new Elasticsearch.Net.Specification.ClusterApi.ClusterHealthRequestParameters
            {
                WaitForStatus = Elasticsearch.Net.WaitForStatus.Yellow,
                WaitForNoInitializingShards = true,
                Timeout = TimeSpan.FromMinutes(5),
            });
        if (!health.Success || JsonNode.Parse(health.Body)?["timed_out"]?.GetValue<bool>() != false)
            throw new InvalidOperationException($"Backup '{backup}' is not ready; source '{index}' was not removed.");
        var unblock = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.PUT, $"{backup}/_settings",
            new JsonObject { ["index.blocks.write"] = null }.ToJsonString());
        if (!unblock.Success) throw Fail(builder, $"Failed to unblock backup '{backup}'.", unblock);
        builder.Logger.LogWarning("'{index}' is an index, not an alias; backed it up to '{backup}' before replacing it with an alias.", index, backup);
        return backup;
    }

    private static async Task DeleteAliasAsync(MigrationBuilder builder, string index, string alias)
    {
        var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.DELETE, $"{index}/_alias/{alias}");
        if (!response.Success && response.HttpStatusCode != 404)
            builder.Logger.LogWarning("Failed to delete alias '{alias}' from '{index}'.  Error: {error}", alias, index, response.Body);
    }
    #endregion
    #endregion
}
