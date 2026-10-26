using System.Text.Json;
using Elasticsearch.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.DAL;
using TNO.Entities;

namespace TNO.API.Helpers;

public record StartOrphanScan(string Side = "elasticsearch", string Index = "content", DateOnly? StartPostedOn = null, DateOnly? EndPostedOn = null);
public record OrphanScanStatus(Guid Id, string Side, string Index, string Status, DateOnly? StartPostedOn, DateOnly? EndPostedOn,
    long Scanned, long Pending, long Found, bool CancelRequested, DateTime CreatedOn, DateTime UpdatedOn,
    DateTime? CompletedOn, string? Error);
public record OrphanScanResults(OrphanedContentItem[] Items, long NextAfter, bool HasMore);

/// <summary>
/// Durable, single-batch reconciliation. No full content documents or unbounded result collections
/// are loaded. A transaction advisory lock coordinates all replicas; every batch uses a fresh scope.
/// </summary>
public class OrphanScanService(TNOContext context, OrphanedContentService content,
    TNO.Elastic.ITNOElasticClient elastic, IOptions<OrphanScanOptions> options, ILogger<OrphanScanService> logger)
{
    private const long WorkerLock = -73129481002;
    private readonly OrphanScanOptions _options = options.Value;
    private static OrphanScanStatus Status(OrphanScan job) => new(job.Id, job.Side, job.Index, job.Status,
        job.StartPostedOn, job.EndPostedOn, job.Scanned, job.Pending, job.Found, job.CancelRequested,
        job.CreatedOn, job.UpdatedOn, job.CompletedOn, job.Error);

    public async Task<OrphanScanStatus?> LatestAsync(CancellationToken ct)
    {
        var job = await context.OrphanScans.AsNoTracking().OrderByDescending(x => x.CreatedOn).FirstOrDefaultAsync(ct);
        return job == null ? null : Status(job);
    }

    public async Task<OrphanScanStatus> StartAsync(StartOrphanScan request, CancellationToken ct)
    {
        OrphanedContentService.ValidateSide(request.Side);
        _ = content.Index(request.Index);
        if (request.EndPostedOn < request.StartPostedOn)
            throw new BadRequestException("End posted date must be on or after start posted date.");
        if (request.EndPostedOn == DateOnly.MaxValue)
            throw new BadRequestException("End posted date must be before 9999-12-31.");
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({WorkerLock})", ct);
        if (await context.OrphanScans.AnyAsync(x => x.CompletedOn == null, ct))
            throw new BadRequestException("An orphan scan is already active. Wait for it or cancel it first.");
        var job = new OrphanScan { Side = request.Side, Index = request.Index, StartPostedOn = request.StartPostedOn, EndPostedOn = request.EndPostedOn };
        context.OrphanScans.Add(job);
        await context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Status(job);
    }

    public async Task CancelAsync(Guid id, CancellationToken ct)
    {
        await context.OrphanScans.Where(x => x.Id == id && x.CompletedOn == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.CancelRequested, true), ct);
    }

    public async Task<OrphanScanResults> ResultsAsync(Guid id, long after, int limit, CancellationToken ct)
    {
        if (after < 0 || limit < 1 || limit > 200) throw new BadRequestException("Use a non-negative cursor and a page size from 1 to 200.");
        var rows = await context.OrphanScanResults.AsNoTracking()
            .Where(x => x.ScanId == id && x.Confirmed && x.ContentId > after).OrderBy(x => x.ContentId)
            .Select(x => new OrphanedContentItem(x.ContentId, x.Headline, x.Status, x.Resolution)).Take(limit + 1).ToArrayAsync(ct);
        var page = rows.Take(limit).ToArray();
        return new(page, page.LastOrDefault()?.Id ?? after, rows.Length > limit);
    }

    public Task MarkResolvedAsync(Guid? scanId, long contentId, string resolution, CancellationToken ct)
        => context.OrphanScanResults.Where(x => x.ScanId == scanId && x.ContentId == contentId && x.Confirmed)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Resolution, resolution), ct);

    /// <summary>Return after one bounded batch, including when another replica owns the lock.</summary>
    public async Task ProcessBatchAsync(CancellationToken stoppingToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(_options.BatchTimeoutSeconds));
        var ct = timeout.Token;
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        var acquired = await context.Database.SqlQuery<bool>($"SELECT pg_try_advisory_xact_lock({WorkerLock}) AS \"Value\"").SingleAsync(ct);
        if (!acquired) return;
        var job = await context.OrphanScans.SingleOrDefaultAsync(x => x.CompletedOn == null, ct);
        if (job == null || (!job.CancelRequested && job.NextBatchOn > DateTime.UtcNow)) return;
        var checkpoint = job.UpdatedOn;
        try
        {
            if (job.CancelRequested)
            {
                await ClosePitAsync(job, ct);
                Finish(job, "Cancelled");
            }
            else if (job.Status == "Scanning") await DiscoverAsync(job, ct);
            else await ConfirmAsync(job, ct);
            job.UpdatedOn = DateTime.UtcNow;
            job.NextBatchOn = job.UpdatedOn.AddMilliseconds(_options.BatchDelayMilliseconds);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
        {
            // A failed DB command may abort the transaction. Roll back the whole batch, then mark
            // this exact checkpoint failed under the same cross-replica lock in a new transaction.
            await transaction.RollbackAsync(stoppingToken);
            await transaction.DisposeAsync();
            context.ChangeTracker.Clear();
            await using var failure = await context.Database.BeginTransactionAsync(stoppingToken);
            await context.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({WorkerLock})", stoppingToken);
            var current = await context.OrphanScans.SingleAsync(x => x.Id == job.Id, stoppingToken);
            if (current.CompletedOn == null && current.UpdatedOn == checkpoint)
            {
                // PIT expires automatically if explicit closure fails.
                using var cleanup = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                cleanup.CancelAfter(TimeSpan.FromSeconds(5));
                await ClosePitAsync(job, cleanup.Token);
                current.PitId = null;
                current.Error = "The scan could not finish. Results are incomplete. Start a new scan; an interrupted Elasticsearch snapshot may have expired.";
                Finish(current, "Failed");
                current.UpdatedOn = DateTime.UtcNow;
                await context.SaveChangesAsync(stoppingToken);
            }
            await failure.CommitAsync(stoppingToken);
            logger.LogError(ex, "Orphan scan {ScanId} failed at a persisted checkpoint.", job.Id);
        }
    }

    private static void Finish(OrphanScan job, string status)
    {
        job.Status = status;
        job.CompletedOn = DateTime.UtcNow;
    }

    private async Task DiscoverAsync(OrphanScan job, CancellationToken ct)
    {
        long[] ids;
        bool complete;
        if (job.Side == "postgres")
        {
            // Freeze the upper bound once; ongoing ingest cannot keep this run alive forever.
            job.MaxId ??= await content.Eligible(job.Index).Select(x => (long?)x.Id).MaxAsync(ct) ?? 0;
            var cutoff = job.CreatedOn.AddSeconds(-_options.GracePeriodSeconds);
            if (job.StartPostedOn.HasValue || job.EndPostedOn.HasValue)
            {
                var rows = PostedContent(job).Where(x => x.Id >= job.MinId && x.Id <= job.MaxId && x.UpdatedOn <= cutoff);
                if (job.AfterPostedOn.HasValue)
                    rows = rows.Where(x => x.PostedOn > job.AfterPostedOn || x.PostedOn == job.AfterPostedOn && x.Id > job.After);
                var page = await rows.OrderBy(x => x.PostedOn).ThenBy(x => x.Id)
                    .Select(x => new { x.Id, x.PostedOn }).Take(_options.BatchSize).ToArrayAsync(ct);
                ids = page.Select(x => x.Id).ToArray();
                if (page.Length > 0) job.AfterPostedOn = page[^1].PostedOn;
            }
            else
            {
                ids = await content.Eligible(job.Index).Where(x => x.Id > job.After && x.Id <= job.MaxId && x.UpdatedOn <= cutoff)
                    .OrderBy(x => x.Id).Select(x => x.Id).Take(_options.BatchSize).ToArrayAsync(ct);
            }
            complete = ids.Length < _options.BatchSize;
            if (ids.Length > 0) job.After = ids[^1];
        }
        else
        {
            if (job.PitId == null)
            {
                var opened = await elastic.LowLevel.OpenPointInTimeAsync<StringResponse>(content.Index(job.Index),
                    new OpenPointInTimeRequestParameters { KeepAlive = "2m" }, ct);
                using var openedResponse = Read(opened);
                job.PitId = openedResponse.RootElement.GetProperty("id").GetString() ?? throw new InvalidOperationException("Missing snapshot ID.");
            }
            var query = new Dictionary<string, object>
            {
                ["size"] = _options.BatchSize,
                ["_source"] = false,
                ["track_total_hits"] = false,
                ["sort"] = new[] { "_shard_doc" },
                ["pit"] = new { id = job.PitId, keep_alive = "2m" },
                ["query"] = new
                {
                    @bool = new
                    {
                        filter = new object[] {
                    new { range = new { id = new { gte = job.MinId, lte = job.MaxId ?? long.MaxValue } } }, PostedOnFilter(job) }
                    }
                },
            };
            if (job.SearchAfter != null) query["search_after"] = JsonSerializer.Deserialize<JsonElement>(job.SearchAfter);
            using var response = Read(await elastic.LowLevel.SearchAsync<StringResponse>(PostData.String(JsonSerializer.Serialize(query)), ctx: ct));
            var root = response.RootElement;
            if (root.TryGetProperty("pit_id", out var pit)) job.PitId = pit.GetString();
            var hits = root.GetProperty("hits").GetProperty("hits").EnumerateArray().ToArray();
            ids = hits.Select(Id).ToArray();
            if (hits.Length > 0) job.SearchAfter = hits[^1].GetProperty("sort").GetRawText();
            complete = ids.Length < _options.BatchSize;
        }
        job.Scanned += ids.Length;
        var present = job.Side == "postgres"
            ? (await SearchIdsAsync(job.Index, ids, false, ct)).Select(Id).ToHashSet()
            : (await context.Contents.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(ct)).ToHashSet();
        foreach (var id in ids.Where(id => !present.Contains(id)))
        {
            context.OrphanScanResults.Add(new OrphanScanResult { ScanId = job.Id, ContentId = id, DetectedOn = DateTime.UtcNow });
            job.Pending++;
        }
        if (complete)
        {
            await ClosePitAsync(job, ct);
            job.Status = "Confirming";
            if (job.Pending == 0) Finish(job, "Completed");
        }
    }

    private async Task ConfirmAsync(OrphanScan job, CancellationToken ct)
    {
        var cutoff = DateTime.UtcNow.AddSeconds(-_options.GracePeriodSeconds);
        var candidates = await context.OrphanScanResults.Where(x => x.ScanId == job.Id && !x.Confirmed && x.DetectedOn <= cutoff)
            .OrderBy(x => x.ContentId).Take(_options.BatchSize).ToArrayAsync(ct);
        if (candidates.Length == 0)
        {
            if (job.Pending == 0) Finish(job, "Completed");
            return;
        }
        var ids = candidates.Select(x => x.ContentId).ToArray();
        // Recheck both stores live after the grace period, including continued existence in the
        // source. Only now fetch the short display fields, never transcripts or child collections.
        var hits = await SearchIdsAsync(job.Index, ids, job.Side == "elasticsearch", ct, job.Side == "elasticsearch" ? job : null);
        var indexed = hits.ToDictionary(Id);
        var database = job.Side == "postgres"
            ? await PostedContent(job).Where(x => ids.Contains(x.Id) && x.UpdatedOn <= cutoff)
                .Select(x => new OrphanedContentItem(x.Id, x.Headline, x.Status.ToString(), "")).ToArrayAsync(ct)
            : await context.Contents.AsNoTracking().Where(x => ids.Contains(x.Id))
                .Select(x => new OrphanedContentItem(x.Id, "", "", "")).ToArrayAsync(ct);
        var existing = database.ToDictionary(x => x.Id);
        foreach (var candidate in candidates)
        {
            var orphan = job.Side == "postgres"
                ? existing.ContainsKey(candidate.ContentId) && !indexed.ContainsKey(candidate.ContentId)
                : !existing.ContainsKey(candidate.ContentId) && indexed.ContainsKey(candidate.ContentId);
            if (orphan)
            {
                var item = job.Side == "postgres" ? existing[candidate.ContentId] : Display(indexed[candidate.ContentId]);
                candidate.Confirmed = true;
                candidate.Headline = item.Headline;
                candidate.Status = item.Status;
                job.Found++;
            }
            else context.OrphanScanResults.Remove(candidate);
            job.Pending--;
        }
        if (job.Pending == 0) Finish(job, "Completed");
    }

    // Date inputs are Pacific calendar days. An exclusive next-midnight bound includes the
    // entire end date and correctly covers 23/25-hour days when daylight saving time changes.
    private static DateTime MidnightUtc(DateOnly date)
        => TimeZoneInfo.ConvertTimeToUtc(date.ToDateTime(TimeOnly.MinValue), TimeZoneInfo.FindSystemTimeZoneById("US/Pacific"));

    private IQueryable<Content> PostedContent(OrphanScan job)
    {
        var query = content.Eligible(job.Index);
        if (job.StartPostedOn.HasValue)
        {
            var start = MidnightUtc(job.StartPostedOn.Value);
            query = query.Where(x => x.PostedOn >= start);
        }
        if (job.EndPostedOn.HasValue)
        {
            var end = MidnightUtc(job.EndPostedOn.Value.AddDays(1));
            query = query.Where(x => x.PostedOn < end);
        }
        return query;
    }

    private static object PostedOnFilter(OrphanScan job)
    {
        var bounds = new Dictionary<string, DateTime>();
        if (job.StartPostedOn.HasValue) bounds["gte"] = MidnightUtc(job.StartPostedOn.Value);
        if (job.EndPostedOn.HasValue) bounds["lt"] = MidnightUtc(job.EndPostedOn.Value.AddDays(1));
        return bounds.Count == 0 ? (object)new { match_all = new { } } : new { range = new { postedOn = bounds } };
    }

    private async Task<JsonElement[]> SearchIdsAsync(string index, long[] ids, bool display, CancellationToken ct, OrphanScan? scope = null)
    {
        if (ids.Length == 0) return [];
        var query = new
        {
            size = ids.Length,
            track_total_hits = false,
            _source = display ? (object)new[] { "id", "headline", "status" } : false,
            query = new
            {
                @bool = new
                {
                    filter = new object[] {
                new { ids = new { values = ids.Select(x => x.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray() } },
                scope == null ? new { match_all = new { } } : PostedOnFilter(scope) }
                }
            }
        };
        using var response = Read(await elastic.LowLevel.SearchAsync<StringResponse>(content.Index(index), PostData.String(JsonSerializer.Serialize(query)), ctx: ct));
        return response.RootElement.GetProperty("hits").GetProperty("hits").EnumerateArray().Select(x => x.Clone()).ToArray();
    }

    private static long Id(JsonElement hit)
        => long.TryParse(hit.GetProperty("_id").GetString(), out var id) && id > 0 ? id : throw new InvalidOperationException("Invalid Elasticsearch content ID.");

    private static OrphanedContentItem Display(JsonElement hit)
    {
        var source = hit.GetProperty("_source");
        var id = Id(hit);
        if (source.GetProperty("id").GetInt64() != id) throw new InvalidOperationException("Elasticsearch content ID does not match its document ID.");
        var status = source.GetProperty("status");
        return new(id, source.GetProperty("headline").GetString() ?? "", status.ValueKind == JsonValueKind.Number
            ? ((ContentStatus)status.GetInt32()).ToString() : status.GetString() ?? "");
    }

    private static JsonDocument Read(StringResponse response)
    {
        if (!response.Success) throw new InvalidOperationException("Elasticsearch reconciliation request failed.", response.OriginalException);
        var document = JsonDocument.Parse(response.Body);
        var root = document.RootElement;
        if ((root.TryGetProperty("timed_out", out var timeout) && timeout.GetBoolean()) ||
            (root.TryGetProperty("_shards", out var shards) && shards.GetProperty("failed").GetInt32() > 0))
        {
            document.Dispose();
            throw new InvalidOperationException("Elasticsearch returned incomplete results.");
        }
        return document;
    }

    private async Task ClosePitAsync(OrphanScan job, CancellationToken ct)
    {
        if (job.PitId == null) return;
        try
        {
            var response = await elastic.LowLevel.ClosePointInTimeAsync<StringResponse>(PostData.String(JsonSerializer.Serialize(new { id = job.PitId })), ctx: ct);
            if (!response.Success) logger.LogWarning("Could not close orphan scan snapshot {ScanId}; it will expire automatically.", job.Id);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Could not close orphan scan snapshot {ScanId}; it will expire automatically.", job.Id); }
        job.PitId = null;
    }
}
