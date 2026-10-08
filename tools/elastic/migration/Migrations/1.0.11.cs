using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.DAL;
using TNO.DAL.Services;
using TNO.Entities;

namespace TNO.Elastic.Migration;

/// <summary>
/// Migration_1011 class, provides a way to migration elastic to version 1.0.11.
/// Adds content analysis ('analysis'), 'topics.isSystem' and 'projectionRevision' to the content
/// indexes, creates the report evidence index, and raises 'index.gc_deletes'.
///
/// Elasticsearch copies existing documents server-side with external projection revisions.
/// Database reads hydrate only missing/stale documents and current analyses. Writers must remain
/// paused until verification and atomic alias cutover complete. Destination mapping metadata
/// records copy progress, allowing a replacement Job to reconnect after interruption.
/// Old indexes are retained; an existing unmanaged destination is never deleted automatically.
/// </summary>
[Migration("1.0.11")]
public class Migration_1011 : TNOMigration
{
    #region Variables
    private const string PreviousVersion = "1.0.10";
    private const int BatchSize = 500;
    private const int IdPageSize = 10000;
    private const int MaxRepairs = 100000;
    private readonly TNOContext _context;
    private readonly IContentAnalysisService _analysisService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Migration_1011 object, initializes with specified parameters.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="contentService"></param>
    /// <param name="analysisService"></param>
    /// <param name="context"></param>
    /// <param name="serializerOptions"></param>
    public Migration_1011(
        MigrationBuilder builder,
        IContentService contentService,
        IContentAnalysisService analysisService,
        TNOContext context,
        IOptions<JsonSerializerOptions> serializerOptions) : base(builder, contentService, serializerOptions)
    {
        _context = context;
        _analysisService = analysisService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <inheritdoc />
    protected override Task UpAsync(MigrationBuilder builder) => WithLockAsync(builder, () => NativeUpAsync(builder));

    /// <inheritdoc />
    protected override Task DownAsync(MigrationBuilder builder) => WithLockAsync(builder, () => NativeDownAsync(builder));

    private async Task WithLockAsync(MigrationBuilder builder, Func<Task> action)
    {
        RequireMaintenance(builder);
        await _context.Database.OpenConnectionAsync();
        try
        {
            await using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT pg_try_advisory_lock(1011001011)";
            if (!Equals(await command.ExecuteScalarAsync(), true))
                throw new InvalidOperationException("Another Elasticsearch migration is using this database.");
            try { await action(); }
            finally
            {
                command.CommandText = "SELECT pg_advisory_unlock(1011001011)";
                await command.ExecuteScalarAsync();
            }
        }
        finally { await _context.Database.CloseConnectionAsync(); }
    }

    /// <summary>
    /// Copy the 1.0.11 indexes natively, verify them, and move the aliases to them.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    private async Task NativeUpAsync(MigrationBuilder builder)
    {
        var targets = GetTargets(builder, $"_v{this.Version}");
        await CreateIndexFromFileAsync(builder, targets.Published.Index, Path.Combine("indexes", "index-published.json"), targets.Published.Alias);
        await CreateIndexFromFileAsync(builder, targets.Content.Index, Path.Combine("indexes", "index-content.json"));
        await CreateIndexFromFileAsync(builder, targets.Evidence?.Index, Path.Combine("indexes", "index-evidence.json"));
        await CopyAndSwitchAsync(builder, targets, includeAnalysis: true);
    }

    /// <summary>
    /// Copy indexes into the 1.0.10 shape, verify them, and move the aliases back
    /// to them. The 1.0.11 indexes and the evidence index are kept.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    private async Task NativeDownAsync(MigrationBuilder builder)
    {
        var current = GetTargets(builder, $"_v{this.Version}");
        var targets = GetTargets(builder, $"_v{PreviousVersion}-rollback-from-{this.Version}") with { Evidence = null };
        var rollbackMapping = Path.Combine("rollback", "index-content.json");
        await CreateIndexFromFileAsync(builder, targets.Published.Index, rollbackMapping, targets.Published.Alias);
        await CreateIndexFromFileAsync(builder, targets.Content.Index, rollbackMapping);
        await CopyAndSwitchAsync(builder, targets, includeAnalysis: false);

        // 1.0.10 has no evidence index.
        if (current.Evidence != null)
        {
            await DeleteAliasAsync(builder, current.Evidence.Value.Index, current.Evidence.Value.Alias);
            builder.Logger.LogWarning("Kept index '{index}' (no longer behind an alias).", current.Evidence.Value.Index);
        }
    }

    /// <summary>
    /// Copy dynamic mappings and documents, repair database differences, move aliases, and verify again. Nothing users see changes
    /// until the aliases move.
    /// </summary>
    private async Task CopyAndSwitchAsync(MigrationBuilder builder, MigrationTargets targets, bool includeAnalysis)
    {
        await CopyDynamicMappingsAsync(builder, targets.Published);
        await CopyDynamicMappingsAsync(builder, targets.Content);

        var native = new NativeReindex(builder);
        await native.CopyAsync(targets.Content.Alias, targets.Content.Index, false, includeAnalysis);
        await native.CopyAsync(targets.Published.Alias, targets.Published.Index, true, includeAnalysis);
        if (includeAnalysis) await RebuildAnalysesAsync(builder, targets);
        await VerifyAsync(builder, targets, includeAnalysis);

        var previous = await SwitchAliasesAsync(builder, targets);

        // A restart after cutover must validate again before recording migration completion.
        await VerifyAsync(builder, targets, includeAnalysis);

        foreach (var index in previous.Distinct())
            builder.Logger.LogWarning("Kept index '{index}' (no longer behind an alias). Delete it once the migration is confirmed: DELETE /{index}", index, index);
    }

    #region Helpers
    /// <summary>
    /// An alias and the versioned index behind it.
    /// </summary>
    private record struct IndexTarget(string Alias, string Index);

    /// <summary>
    /// The indexes a rebuild writes; evidence is skipped when no evidence index is configured.
    /// </summary>
    private record MigrationTargets(IndexTarget Published, IndexTarget Content, IndexTarget? Evidence);

    private static MigrationTargets GetTargets(MigrationBuilder builder, string suffix)
    {
        var options = builder.MigrationOptions;
        return new MigrationTargets(
            new IndexTarget(options.PublishedIndex, $"{options.PublishedIndex}{suffix}"),
            new IndexTarget(options.ContentIndex, $"{options.ContentIndex}{suffix}"),
            String.IsNullOrWhiteSpace(options.EvidenceIndex) ? null : new IndexTarget(options.EvidenceIndex, $"{options.EvidenceIndex}{suffix}"));
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
    private async Task CreateIndexFromFileAsync(MigrationBuilder builder, string? index, string file, string? sourceAlias = null)
    {
        if (index == null) return;
        if ((await RequestAsync(builder, Elasticsearch.Net.HttpMethod.HEAD, index)).HttpStatusCode == 200)
        {
            var mapping = await new NativeReindex(builder).ReadAsync($"{index}/_mapping");
            if (mapping[index]?["mappings"]?["_meta"]?["owner"]?.GetValue<string>() != NativeReindex.Owner)
                throw new InvalidOperationException($"Index '{index}' already exists without native migration metadata. Verify it is an unused partial destination and clean it up explicitly before retrying; nothing was deleted.");
            builder.Logger.LogInformation("Resuming retained destination {index}", index);
            return;
        }
        var path = Path.Combine(builder.MigrationOptions.MigrationsPath, this.Version, file);
        var json = ReplaceIndexNames(builder, await File.ReadAllTextAsync(path));
        var step = JsonSerializer.Deserialize<MigrationStep>(json, builder.SerializerOptions) ?? throw new InvalidOperationException($"Failed to deserialize '{path}'");
        var data = JsonNode.Parse(JsonSerializer.Serialize(step.Data, builder.SerializerOptions))!;
        data["mappings"]!["_meta"] = new JsonObject { ["owner"] = NativeReindex.Owner };
        var indexSettings = data["settings"]!["index"]!;
        // Preserve cluster-specific topology instead of hard-coding three replicas on Cloud.
        sourceAlias ??= builder.MigrationOptions.ContentIndex;
        var sourceSettings = await new NativeReindex(builder).ReadAsync($"{sourceAlias}/_settings");
        var original = sourceSettings.AsObject().First().Value!["settings"]!["index"]!;
        indexSettings["number_of_replicas"] = builder.MigrationOptions.NumberOfReplicas ?? Int32.Parse(original["number_of_replicas"]!.GetValue<string>());
        indexSettings["number_of_shards"] = builder.MigrationOptions.NumberOfShards ?? Int32.Parse(original["number_of_shards"]!.GetValue<string>());
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

    private static void RequireMaintenance(MigrationBuilder builder)
    {
        if (!builder.MigrationOptions.WritersPaused)
            throw new InvalidOperationException("Pause database and Elasticsearch writers, then set Elastic__WritersPaused=true. Keep them paused through verification and alias cutover; see the migration runbook.");
        var throttle = builder.MigrationOptions.ReindexRequestsPerSecond;
        if (throttle != -1 && throttle <= 0)
            throw new InvalidOperationException("ReindexRequestsPerSecond must be positive or -1.");
    }

    private async Task RebuildAnalysesAsync(MigrationBuilder builder, MigrationTargets targets)
    {
        long after = 0;
        while (true)
        {
            var ids = await _context.Contents.AsNoTracking()
                .Where(c => c.Id > after && (_context.ContentAnalyses.Any(a => a.ContentId == c.Id && a.IsCurrent)
                    || c.TopicsManyToMany.Any(t => t.Topic!.IsSystem)))
                .OrderBy(c => c.Id).Select(c => c.Id).Take(BatchSize).ToArrayAsync();
            if (ids.Length == 0) break;
            await IndexAsync(builder, targets, ids, includeAnalysis: true);
            after = ids[^1];
            builder.Logger.LogInformation("Rebuilt current analyses through content {id}", after);
        }
    }

    /// <summary>
    /// Index the specified content into the target indexes, versioned by projection revision.
    /// </summary>
    /// <returns>The IDs of the content that exists.</returns>
    private async Task<HashSet<long>> IndexAsync(MigrationBuilder builder, MigrationTargets targets, long[] ids, bool includeAnalysis)
    {
        var contents = await _context.Contents.AsNoTracking().AsSplitQuery()
            .Include(c => c.MediaType)
            .Include(c => c.Source)
            .Include(c => c.Series)
            .Include(c => c.Contributor)
            .Include(c => c.License)
            .Include(c => c.Owner)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.TopicsManyToMany).ThenInclude(ct => ct.Topic)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.Labels)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .Include(c => c.Quotes)
            .Where(c => ids.Contains(c.Id))
            .ToArrayAsync();
        var analyses = includeAnalysis ? _analysisService.FindCurrent(ids) : new Dictionary<long, Entities.ContentAnalysis>();

        var operations = new BulkOperationsCollection();
        foreach (var content in contents)
        {
            var model = new ContentModel(content, _serializerOptions);
            if (analyses.TryGetValue(content.Id, out var analysis))
                model.Analysis = new ContentAnalysisSummaryModel(analysis, _serializerOptions);
            var revision = content.ProjectionRevision;

            operations.Add(new BulkIndexOperation<ContentModel>(model) { Index = targets.Content.Index, Id = content.Id, VersionType = ExternalGte, Version = revision });

            if (content.Status == ContentStatus.Publish || content.Status == ContentStatus.Published)
            {
                // An unapproved transcript is never published.
                var published = model.ToPublishedDocument();
                published.Status = ContentStatus.Published;
                operations.Add(new BulkIndexOperation<ContentModel>(published) { Index = targets.Published.Index, Id = content.Id, VersionType = ExternalGte, Version = revision });
            }
            else
            {
                // Content unpublished while the migration ran.
                operations.Add(new BulkDeleteOperation(content.Id) { Index = targets.Published.Index, VersionType = ExternalGte, Version = revision });
            }

            if (targets.Evidence != null)
            {
                if (model.Analysis != null)
                    operations.Add(new BulkIndexOperation<ContentEvidenceModel>(new ContentEvidenceModel(model, model.Analysis)) { Index = targets.Evidence.Value.Index, Id = content.Id, VersionType = ExternalGte, Version = revision });
                else
                    operations.Add(new BulkDeleteOperation(content.Id) { Index = targets.Evidence.Value.Index, VersionType = ExternalGte, Version = revision });
            }
        }

        await BulkAsync(builder, operations);
        return contents.Select(c => c.Id).ToHashSet();
    }

    private const global::Elastic.Clients.Elasticsearch.VersionType ExternalGte = global::Elastic.Clients.Elasticsearch.VersionType.ExternalGte;

    /// <summary>
    /// Remove content from the target indexes. With a revision the delete is ordered; without one
    /// (content that is gone and whose revision is unknown) it is unconditional.
    /// </summary>
    private static async Task DeleteAsync(MigrationBuilder builder, MigrationTargets targets, IEnumerable<long> contentIds, Func<long, long>? revision = null, bool publishedOnly = false)
    {
        var operations = new BulkOperationsCollection();
        var indexes = publishedOnly
            ? new[] { targets.Published.Index }
            : new[] { targets.Content.Index, targets.Published.Index, targets.Evidence?.Index }.OfType<string>().ToArray();
        foreach (var contentId in contentIds)
            foreach (var index in indexes)
                operations.Add(revision == null
                    ? new BulkDeleteOperation(contentId) { Index = index }
                    : new BulkDeleteOperation(contentId) { Index = index, VersionType = ExternalGte, Version = revision(contentId) });
        await BulkAsync(builder, operations);
    }

    /// <summary>
    /// Send the bulk request, retrying failures. Only deletes of missing documents are success;
    /// revision conflicts during a maintenance window require investigation.
    /// </summary>
    private static async Task BulkAsync(MigrationBuilder builder, BulkOperationsCollection operations)
    {
        if (operations.Count == 0) return;
        var failures = 0;
        while (true)
        {
            var response = await builder.IndexingClient.BulkAsync(new global::Elastic.Clients.Elasticsearch.BulkRequest { Operations = operations });
            var errors = response.ItemsWithErrors.Where(i => !(i.Status == 404 && i.Operation == "delete")).ToArray();
            if (response.ApiCallDetails?.HasSuccessfulStatusCode == true && errors.Length == 0) return;

            foreach (var item in errors.Take(20))
                builder.Logger.LogError("Failed to index document {id} in '{index}': {error}", item.Id, item.Index, item.Error?.Reason);
            if (++failures >= builder.MigrationOptions.ReindexFailureLimit)
                throw new InvalidOperationException($"Failed to index content: {response.DebugInformation}");
            await Task.Delay(builder.MigrationOptions.ReindexDelay);
        }
    }

    /// <summary>
    /// The content IDs in the database with their projection revisions, ascending.
    /// </summary>
    private async IAsyncEnumerable<(long Id, long Revision)> DatabaseIdsAsync(bool publishedOnly, bool evidenceOnly = false)
    {
        long after = 0;
        while (true)
        {
            var query = _context.Contents.AsNoTracking().Where(c => c.Id > after);
            if (evidenceOnly) query = query.Where(c => _context.ContentAnalyses.Any(a => a.ContentId == c.Id && a.IsCurrent));
            if (publishedOnly) query = query.Where(c => c.Status == ContentStatus.Publish || c.Status == ContentStatus.Published);
            var rows = await query.OrderBy(c => c.Id).Select(c => new { c.Id, c.ProjectionRevision }).Take(IdPageSize).ToArrayAsync();
            foreach (var row in rows) yield return (row.Id, row.ProjectionRevision);
            if (rows.Length < IdPageSize) yield break;
            after = rows[^1].Id;
        }
    }

    /// <summary>
    /// The document IDs in an index (or alias) with their projection revisions (null when the
    /// document has none), ascending, read with a point in time.
    /// </summary>
    private static async IAsyncEnumerable<(long Id, long? Revision, long? Version)> IndexIdsAsync(MigrationBuilder builder, string index, string idField = "id", [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var refresh = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_refresh");
        if (!refresh.Success) throw Fail(builder, $"Failed to refresh '{index}'.", refresh);
        var open = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, $"{index}/_pit", null, new OpenPointInTimeRequestParameters() { KeepAlive = "5m" });
        if (!open.Success) throw Fail(builder, $"Failed to open '{index}'.", open);
        var pitId = JsonNode.Parse(open.Body)!["id"]!.GetValue<string>();
        try
        {
            JsonNode? searchAfter = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var body = new JsonObject
                {
                    ["size"] = IdPageSize,
                    ["_source"] = new JsonArray("projectionRevision"),
                    ["track_total_hits"] = false,
                    ["version"] = true,
                    ["pit"] = new JsonObject { ["id"] = pitId, ["keep_alive"] = "5m" },
                    ["sort"] = new JsonArray(new JsonObject { [idField] = "asc" }),
                };
                if (searchAfter != null) body["search_after"] = searchAfter.DeepClone();
                var response = await RequestAsync(builder, Elasticsearch.Net.HttpMethod.POST, "_search", body.ToJsonString());
                if (!response.Success) throw Fail(builder, $"Failed to read the IDs in '{index}'.", response);
                var root = JsonNode.Parse(response.Body)!;
                pitId = root["pit_id"]?.GetValue<string>() ?? pitId;
                var hits = root["hits"]!["hits"]!.AsArray();
                foreach (var hit in hits)
                {
                    searchAfter = hit!["sort"];
                    var revision = hit["_source"]?["projectionRevision"];
                    yield return (searchAfter!.AsArray()[0]!.GetValue<long>(), revision?.GetValue<long>(), hit["_version"]?.GetValue<long>());
                }
                if (hits.Count < IdPageSize) yield break;
            }
        }
        finally
        {
            await RequestAsync(builder, Elasticsearch.Net.HttpMethod.DELETE, "_pit", new JsonObject { ["id"] = pitId }.ToJsonString());
        }
    }

    /// <summary>
    /// What an index lacks (or has at an older revision) and what it has in excess, compared with
    /// the database.
    /// </summary>
    private record IdDifference(long MissingCount, List<long> Missing, long ExtraCount, List<long> Extra, long IndexCount, long DatabaseCount);

    private async Task<IdDifference> CompareAsync(MigrationBuilder builder, string index, bool publishedOnly, int keep, bool compareRevisions = true, bool evidenceOnly = false)
    {
        var missing = new List<long>();
        var extra = new List<long>();
        long missingCount = 0, extraCount = 0, indexCount = 0, databaseCount = 0;
        await using var db = DatabaseIdsAsync(publishedOnly, evidenceOnly).GetAsyncEnumerator();
        await using var es = IndexIdsAsync(builder, index, evidenceOnly ? "contentId" : "id").GetAsyncEnumerator();
        var hasDb = await db.MoveNextAsync();
        var hasEs = await es.MoveNextAsync();
        while (hasDb || hasEs)
        {
            if (hasDb && (!hasEs || db.Current.Id < es.Current.Id))
            {
                missingCount++;
                databaseCount++;
                if (missing.Count < keep) missing.Add(db.Current.Id);
                hasDb = await db.MoveNextAsync();
            }
            else if (hasEs && (!hasDb || es.Current.Id < db.Current.Id))
            {
                extraCount++;
                indexCount++;
                if (extra.Count < keep) extra.Add(es.Current.Id);
                hasEs = await es.MoveNextAsync();
            }
            else
            {
                // A document behind its content's revision is out of date.
                if (compareRevisions && (es.Current.Revision != db.Current.Revision || es.Current.Version != db.Current.Revision))
                {
                    missingCount++;
                    if (missing.Count < keep) missing.Add(db.Current.Id);
                }
                databaseCount++;
                indexCount++;
                hasDb = await db.MoveNextAsync();
                hasEs = await es.MoveNextAsync();
            }
        }
        return new IdDifference(missingCount, missing, extraCount, extra, indexCount, databaseCount);
    }

    /// <summary>
    /// Every content item must be in the new content index at its current revision, and every
    /// published item in the new published index, with nothing else. Repair differences in bounded
    /// batches and fail if a pass makes no progress. Evidence must match current analyses too.
    /// </summary>
    private async Task VerifyAsync(MigrationBuilder builder, MigrationTargets targets, bool includeAnalysis)
    {
        var checks = new List<(IndexTarget Target, bool Published, bool Evidence)> { (targets.Content, false, false), (targets.Published, true, false) };
        if (targets.Evidence != null) checks.Add((targets.Evidence.Value, false, true));
        foreach (var (target, publishedOnly, evidenceOnly) in checks)
        {
            var difference = await CompareAsync(builder, target.Index, publishedOnly, MaxRepairs, evidenceOnly: evidenceOnly);
            while (difference.MissingCount > 0 || difference.ExtraCount > 0)
            {
                var before = difference.MissingCount + difference.ExtraCount;
                builder.Logger.LogWarning("Repairing '{index}': {missing:N0} missing or out of date, {extra:N0} extra", target.Index, difference.MissingCount, difference.ExtraCount);
                foreach (var batch in difference.Missing.Chunk(BatchSize))
                    await IndexAsync(builder, targets, batch, includeAnalysis);
                foreach (var batch in difference.Extra.Chunk(BatchSize))
                {
                    if (evidenceOnly)
                    {
                        var deletes = new BulkOperationsCollection();
                        foreach (var id in batch) deletes.Add(new BulkDeleteOperation(id) { Index = target.Index });
                        await BulkAsync(builder, deletes);
                    }
                    else await DeleteAsync(builder, targets, batch, publishedOnly: publishedOnly);
                }
                difference = await CompareAsync(builder, target.Index, publishedOnly, MaxRepairs, evidenceOnly: evidenceOnly);
                if (difference.MissingCount + difference.ExtraCount >= before)
                    throw new InvalidOperationException($"Repairs to '{target.Index}' made no progress. Keep writers paused and investigate; migration was not marked complete.");
            }
            builder.Logger.LogInformation("Verified '{index}': {count:N0} document(s), matching the database", target.Index, difference.IndexCount);
        }
    }

    /// <summary>
    /// Point each alias at its new index in one atomic request. An alias name held by a concrete
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
        builder.Logger.LogInformation("Moved the aliases: {aliases}", String.Join(", ", new[] { targets.Published, targets.Content, targets.Evidence }.OfType<IndexTarget>().Select(t => $"{t.Alias} -> {t.Index}")));
        return previous;
    }

    private static async Task AddAliasActionsAsync(MigrationBuilder builder, MigrationTargets targets, List<string> previous, JsonArray actions, List<string> blocked)
    {
        foreach (var target in new[] { targets.Published, targets.Content, targets.Evidence }.OfType<IndexTarget>())
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
