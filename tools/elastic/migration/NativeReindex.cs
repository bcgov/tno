using System.Text.Json.Nodes;
using Elasticsearch.Net;
using Microsoft.Extensions.Logging;

namespace TNO.Elastic.Migration;

/// <summary>Coordinates server-side copies; progress lives in the destination mapping's _meta.</summary>
public sealed class NativeReindex(MigrationBuilder builder)
{
    internal const string Owner = "tno-native-1.0.11";

    internal async Task<JsonNode> ReadAsync(string path, IRequestParameters? parameters = null)
    {
        var response = await RequestAsync(Elasticsearch.Net.HttpMethod.GET, path, parameters: parameters);
        return JsonNode.Parse(response.Body)!;
    }

    private async Task<StringResponse> RequestAsync(Elasticsearch.Net.HttpMethod method, string path,
        JsonNode? body = null, IRequestParameters? parameters = null)
    {
        parameters ??= new SearchRequestParameters();
        parameters.RequestConfiguration = new RequestConfiguration
        {
            ThrowExceptions = false,
            OpaqueId = parameters.RequestConfiguration?.OpaqueId,
        };
        var response = await builder.Client.LowLevel.DoRequestAsync<StringResponse>(method, path,
            CancellationToken.None, body == null ? null : PostData.String(body.ToJsonString()), parameters);
        if (!response.Success) throw new InvalidOperationException($"Elasticsearch {method} {path} failed: {response.Body}", response.OriginalException);
        return response;
    }

    private Task SaveAsync(string destination, JsonObject state) => RequestAsync(Elasticsearch.Net.HttpMethod.PUT,
        $"{destination}/_mapping", new JsonObject { ["_meta"] = state.DeepClone() });

    /// <summary>Copy once, reconnect to an existing task, or safely replay an interrupted copy.</summary>
    public async Task CopyAsync(string sourceAlias, string destination, bool published, bool includeAnalysis)
    {
        var mapping = await ReadAsync($"{destination}/_mapping");
        var state = mapping[destination]?["mappings"]?["_meta"]?.DeepClone().AsObject()
            ?? throw new InvalidOperationException($"'{destination}' has no native migration ownership marker.");
        if (state["owner"]?.GetValue<string>() != Owner)
            throw new InvalidOperationException($"'{destination}' belongs to another migration.");
        // The alias may already point to the destination after a crash immediately after cutover.
        if (state["complete"]?.GetValue<bool>() == true)
        {
            var current = (await ReadAsync($"{sourceAlias}/_settings")).AsObject();
            if (current.Count != 1 || (current.First().Key != destination &&
                current.First().Value?["settings"]?["index"]?["uuid"]?.GetValue<string>() != state["sourceUuid"]?.GetValue<string>()))
                throw new InvalidOperationException($"Source for completed copy '{destination}' changed; refusing stale reuse.");
            return;
        }
        if (state["source"] == null)
        {
            var sources = (await ReadAsync($"{sourceAlias}/_settings")).AsObject();
            if (sources.Count != 1 || sources.First().Key == destination)
                throw new InvalidOperationException($"'{sourceAlias}' must resolve to one source index distinct from '{destination}'.");
            state["source"] = sources.First().Key;
            state["sourceUuid"] = sources.First().Value!["settings"]!["index"]!["uuid"]!.DeepClone();
            await SaveAsync(destination, state);
        }
        var source = state["source"]!.GetValue<string>();
        var settings = await ReadAsync($"{source}/_settings");
        if (settings[source]?["settings"]?["index"]?["uuid"]?.GetValue<string>() != state["sourceUuid"]!.GetValue<string>())
            throw new InvalidOperationException($"Source index '{source}' was replaced; refusing to resume.");

        var opaqueId = $"{Owner}:{destination}";
        var taskId = state["task"]?.GetValue<string>();
        if (taskId == null)
        {
            // Recover the narrow crash window between launching a task and saving its ID.
            var tasks = await ReadAsync("_tasks", new Elasticsearch.Net.Specification.TasksApi.ListTasksRequestParameters { Actions = new[] { "*reindex" }, Detailed = true });
            var matches = tasks["nodes"]?.AsObject().SelectMany(n => n.Value?["tasks"]?.AsObject() ?? new JsonObject())
                .Where(t => t.Value?["headers"]?["X-Opaque-Id"]?.GetValue<string>() == opaqueId)
                .Select(t => t.Key).ToArray() ?? [];
            if (matches.Length > 1) throw new InvalidOperationException($"Multiple migration tasks for '{destination}'; investigate before retrying.");
            taskId = matches.SingleOrDefault();
            if (taskId == null)
            {
                // Replays are safe: external versions stop older revisions replacing newer writes.
                var body = CreateRequest(source, destination, published, includeAnalysis);
                var response = await RequestAsync(Elasticsearch.Net.HttpMethod.POST, "_reindex", body,
                    new ReindexOnServerRequestParameters
                    {
                        WaitForCompletion = false,
                        RequestsPerSecond = builder.MigrationOptions.ReindexRequestsPerSecond,
                        RequestConfiguration = new RequestConfiguration { OpaqueId = opaqueId },
                    });
                taskId = JsonNode.Parse(response.Body)!["task"]!.GetValue<string>();
            }
            state["task"] = taskId;
            await SaveAsync(destination, state);
        }
        builder.Logger.LogInformation("Following native reindex task {task}: {source} -> {destination}", taskId, source, destination);
        while (true)
        {
            var response = await builder.Client.LowLevel.DoRequestAsync<StringResponse>(Elasticsearch.Net.HttpMethod.GET,
                $"_tasks/{taskId}", CancellationToken.None, null, new SearchRequestParameters
                { RequestConfiguration = new RequestConfiguration { ThrowExceptions = false } });
            if (response.HttpStatusCode == 404)
            {
                state.Remove("task");
                await SaveAsync(destination, state);
                throw new InvalidOperationException($"Reindex task '{taskId}' is missing (possibly a node restart). Rerun to replay the copy into the retained destination.");
            }
            if (!response.Success) throw new InvalidOperationException($"Cannot read task '{taskId}'; rerun to reconnect. {response.Body}");
            var result = JsonNode.Parse(response.Body)!;
            if (result["completed"]?.GetValue<bool>() == true)
            {
                if (result["error"] != null || result["response"] == null ||
                    result["response"]?["timed_out"]?.GetValue<bool>() == true ||
                    result["response"]?["failures"]?.AsArray().Count > 0)
                {
                    state.Remove("task");
                    await SaveAsync(destination, state);
                    throw new InvalidOperationException($"Reindex task '{taskId}' failed; destinations retained, aliases unchanged. {result}");
                }
                state["complete"] = true;
                await SaveAsync(destination, state);
                builder.Logger.LogInformation("Native copy complete for {destination}: {result}", destination, result["response"]);
                return;
            }
            builder.Logger.LogInformation("Native reindex {task}: {status}", taskId, result["task"]?["status"]);
            await Task.Delay(Math.Max(1000, builder.MigrationOptions.ReindexDelay));
        }
    }

    /// <summary>Build the ES 7/8 compatible transformation, including external revision zero for legacy data.</summary>
    public static JsonObject CreateRequest(string source, string destination, bool published, bool includeAnalysis) => new()
    {
        ["source"] = new JsonObject { ["index"] = source },
        ["dest"] = new JsonObject { ["index"] = destination, ["version_type"] = "external_gte" },
        ["conflicts"] = "proceed",
        ["script"] = new JsonObject
        {
            ["lang"] = "painless",
            ["source"] = """
                long revision = ctx._source.projectionRevision == null ? 0L : (long)ctx._source.projectionRevision;
                ctx._source.projectionRevision = revision;
                ctx._version = revision;
                ctx._source.remove('analysis');
                if (ctx._source.topics != null) {
                    for (def topic : ctx._source.topics) {
                        if (params.includeAnalysis) {
                            if (topic.isSystem == null) { topic.isSystem = false; }
                        } else { topic.remove('isSystem'); }
                    }
                }
                if (params.published) {
                    ctx._source.status = 2;
                    if ((ctx._source.contentType == null || ctx._source.contentType == 0 || ctx._source.contentType == 'AudioVideo') && ctx._source.isApproved != true) {
                        ctx._source.body = '';
                    }
                }
                """,
            ["params"] = new JsonObject { ["published"] = published, ["includeAnalysis"] = includeAnalysis },
        },
    };
}
