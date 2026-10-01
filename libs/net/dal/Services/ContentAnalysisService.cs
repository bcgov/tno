using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.Core.Exceptions;
using TNO.DAL.Analysis;
using TNO.DAL.Config;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ContentAnalysisService class, the Content-Analysis work queue and the acceptance of results.
/// </summary>
public class ContentAnalysisService : BaseService, IContentAnalysisService
{
    #region Variables
    /// <summary>The submission was stored and applied.</summary>
    public const string Accepted = "Accepted";
    /// <summary>The same input was already analyzed; the stored result is returned.</summary>
    public const string Duplicate = "Duplicate";
    /// <summary>The submission is out of date and was rejected without populating anything.</summary>
    public const string Stale = "Stale";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ContentAnalysisOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentAnalysisService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentAnalysisService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IOptions<ContentAnalysisOptions> options,
        ILogger<ContentAnalysisService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _options = options.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// The runtime settings.
    /// </summary>
    /// <returns></returns>
    public ContentAnalysisSettings GetSettings() => ContentAnalysisSettings.Read(this.Context);

    /// <summary>
    /// Claim due jobs.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public IEnumerable<AnalysisJob> ClaimJobs(AnalysisClaimRequestModel request)
    {
        var quantity = Math.Clamp(request.Quantity, 1, 100);
        var worker = String.IsNullOrWhiteSpace(request.WorkerId) ? "worker" : request.WorkerId[..Math.Min(250, request.WorkerId.Length)];

        // A job whose lease lapsed too often fails rather than returning to the queue again.
        this.Context.Database.ExecuteSqlRaw(@"
UPDATE public.analysis_job
SET status = 3, last_error = 'The worker''s claim lapsed too many times.', lease_expires_on = NULL, updated_on = CURRENT_TIMESTAMP, version = version + 1
WHERE status = 1 AND lease_expires_on < CURRENT_TIMESTAMP AND attempts + 1 >= {0}", Math.Max(1, _options.MaxAttempts));

        var ids = Claim(false, quantity, worker).ToList();
        var backfill = Math.Min(Math.Max(0, request.MaxBackfill), quantity - ids.Count);
        if (backfill > 0) ids.AddRange(Claim(true, backfill, worker));
        if (ids.Count == 0) return Array.Empty<AnalysisJob>();
        return this.Context.AnalysisJobs.AsNoTracking().Where(j => ids.Contains(j.Id)).OrderByDescending(j => j.Priority).ThenBy(j => j.DueOn).ToArray();
    }

    private long[] Claim(bool backfill, int quantity, string worker)
    {
        // SKIP LOCKED lets any number of workers claim concurrently without taking the same job.
        const string sql = @"
WITH due AS (
    SELECT id FROM public.analysis_job
    WHERE ((status = 0 AND due_on <= CURRENT_TIMESTAMP AND (next_attempt_on IS NULL OR next_attempt_on <= CURRENT_TIMESTAMP))
        OR (status = 1 AND lease_expires_on < CURRENT_TIMESTAMP))
        AND ((@backfill AND reason = 2) OR (NOT @backfill AND reason <> 2))
    ORDER BY priority DESC, due_on
    LIMIT @quantity
    FOR UPDATE SKIP LOCKED
)
UPDATE public.analysis_job j
SET status = 1,
    attempts = CASE WHEN j.status = 1 THEN j.attempts + 1 ELSE j.attempts END,
    fencing_token = j.fencing_token + 1,
    lease_expires_on = CURRENT_TIMESTAMP + make_interval(secs => @lease),
    claimed_by = @worker,
    updated_by = @worker,
    updated_on = CURRENT_TIMESTAMP,
    version = j.version + 1
FROM due
WHERE j.id = due.id
RETURNING j.id AS ""Value""";
        return this.Context.Database.SqlQueryRaw<long>(sql,
                new Npgsql.NpgsqlParameter("backfill", backfill),
                new Npgsql.NpgsqlParameter("quantity", quantity),
                new Npgsql.NpgsqlParameter("lease", (double)Math.Max(30, _options.LeaseSeconds)),
                new Npgsql.NpgsqlParameter("worker", worker))
            .ToArray();
    }

    /// <summary>
    /// The job for a valid claim, tracked.
    /// </summary>
    private AnalysisJob? FindClaimed(AnalysisLeaseModel lease)
    {
        var job = this.Context.AnalysisJobs.FirstOrDefault(j => j.Id == lease.JobId);
        if (job == null || job.Status != AnalysisJobStatus.Claimed || job.FencingToken != lease.FencingToken) return null;
        return job;
    }

    /// <summary>
    /// Extend a valid claim's lease.
    /// </summary>
    /// <param name="lease"></param>
    /// <returns></returns>
    public AnalysisJob? RenewLease(AnalysisLeaseModel lease)
    {
        var job = FindClaimed(lease);
        if (job == null) return null;
        job.LeaseExpiresOn = DateTime.UtcNow.AddSeconds(Math.Max(30, _options.LeaseSeconds));
        this.Context.CommitTransaction();
        return job;
    }

    /// <summary>
    /// The current input of claimed content.
    /// </summary>
    /// <param name="lease"></param>
    /// <returns></returns>
    public AnalysisInputModel? GetInput(AnalysisLeaseModel lease)
    {
        var job = FindClaimed(lease);
        if (job == null) return null;
        var content = this.Context.Contents.AsNoTracking()
            .Include(c => c.Source)
            .Include(c => c.MediaType)
            .Include(c => c.Series)
            .FirstOrDefault(c => c.Id == job.ContentId);
        if (content == null) return null;

        var settings = GetSettings();
        var ineligible = settings.GetIneligibleReason(content);
        var isSummaryHumanOwned = this.Context.IsSummaryHumanOwned(content.Id);
        var input = new AnalysisInputModel()
        {
            JobId = job.Id,
            ContentId = content.Id,
            InputHash = AnalysisInput.ComputeHash(content, isSummaryHumanOwned),
            IsEligible = ineligible == null,
            IneligibleReason = ineligible,
            ContentType = content.ContentType,
            Headline = content.Headline,
            Byline = content.Byline,
            Body = content.Body,
            Summary = isSummaryHumanOwned ? content.Summary : "",
            Source = content.Source?.Name ?? content.OtherSource,
            SourceId = content.SourceId,
            MediaType = content.MediaType?.Name ?? "",
            MediaTypeId = content.MediaTypeId,
            Series = content.Series?.Name ?? "",
            PublishedOn = content.PublishedOn,
            IsApproved = content.IsApproved,
            LLMId = settings.LLMId,
        };

        if (ineligible != null)
        {
            // Ineligible content waits for its input to change (e.g. transcript approval).
            job.Status = AnalysisJobStatus.Skipped;
            job.LastError = ineligible;
            job.LeaseExpiresOn = null;
            this.Context.CommitTransaction();
        }
        return input;
    }

    /// <summary>
    /// Accept an analysis.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public AnalysisSubmitResultModel Submit(AnalysisResultModel result)
    {
        var job = this.Context.AnalysisJobs.FirstOrDefault(j => j.Id == result.JobId);
        if (job == null) return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The job no longer exists." };

        var duplicate = this.Context.ContentAnalyses.AsNoTracking().FirstOrDefault(a => a.ContentId == job.ContentId && a.InputHash == result.InputHash);
        if (duplicate != null)
        {
            if (job.Status == AnalysisJobStatus.Claimed && job.FencingToken == result.FencingToken && job.InputHash == result.InputHash)
            {
                job.Status = AnalysisJobStatus.Completed;
                job.CompletedOn = DateTime.UtcNow;
                job.LeaseExpiresOn = null;
                this.Context.CommitTransaction();
            }
            return new AnalysisSubmitResultModel() { Status = Duplicate, AnalysisId = duplicate.Id };
        }

        if (job.Status != AnalysisJobStatus.Claimed || job.FencingToken != result.FencingToken)
            return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The claim is no longer valid." };
        if (job.InputHash != result.InputHash)
            return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The content changed after it was claimed." };

        var content = this.Context.Contents
            .Include(c => c.TagsManyToMany)
            .Include(c => c.TopicsManyToMany)
            .Include(c => c.Quotes)
            .FirstOrDefault(c => c.Id == job.ContentId);
        if (content == null) return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The content no longer exists." };

        var settings = GetSettings();
        var ineligible = settings.GetIneligibleReason(content);
        if (ineligible != null) return new AnalysisSubmitResultModel() { Status = Stale, Reason = ineligible };
        if (AnalysisInput.ComputeHash(content, this.Context.IsSummaryHumanOwned(content.Id)) != result.InputHash)
            return new AnalysisSubmitResultModel() { Status = Stale, Reason = "The content changed after it was claimed." };

        using var transaction = this.Context.Database.BeginTransaction();
        this.Context.ChangeOwner = FieldOwner.Analysis;
        try
        {
            foreach (var previous in this.Context.ContentAnalyses.Where(a => a.ContentId == content.Id && a.IsCurrent))
                previous.IsCurrent = false;

            var registry = ResolveRegistry(result.PrimaryTopic);
            var analysis = ToEntity(result, content.Id, registry);
            this.Context.ContentAnalyses.Add(analysis);
            this.Context.SaveChanges();

            // Only the fields of the processes the worker ran are applied.
            var populator = new AnalysisPopulator(this.Context, settings, content, analysis);
            populator.Populate(result);
            var populated = populator.PopulatedFields.ToArray();
            // Populating any field changes the content's version, so an open editor form merges it.
            if (populated.Length > 0) this.Context.Entry(content).Property(c => c.UpdatedOn).IsModified = true;

            job.Status = AnalysisJobStatus.Completed;
            job.CompletedOn = DateTime.UtcNow;
            job.LeaseExpiresOn = null;
            job.LastError = null;

            // The analysis is part of the indexed document, so the content is always re-indexed.
            this.Context.RequestIndex(content, TNOContext.GetIndexAction(content.Status), TNOContext.IndexReasonAnalysis);
            this.Context.SaveChanges();
            transaction.Commit();

            return new AnalysisSubmitResultModel()
            {
                Status = Accepted,
                AnalysisId = analysis.Id,
                PopulatedFields = populated,
                ContentChanged = populated.Length > 0,
                ContentStatus = content.Status,
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
        finally
        {
            this.Context.ChangeOwner = null;
        }
    }

    /// <summary>
    /// The registry entry for a topic label: matched by its deterministic key or an alias, otherwise
    /// added, linked to the staff topic of the same name when there is one.
    /// </summary>
    private AnalysisTopic? ResolveRegistry(string? label)
    {
        var key = AnalysisPopulator.Normalize(label);
        if (key.Length == 0) return null;
        var entry = this.Context.AnalysisTopics.FirstOrDefault(t => t.Key == key || t.Aliases.Contains(key));
        if (entry != null) return entry;

        var staffTopic = this.Context.Topics.AsNoTracking().Where(t => !t.IsSystem).Select(t => new { t.Id, t.Name }).ToArray()
            .FirstOrDefault(t => AnalysisPopulator.Normalize(t.Name) == key);
        entry = new AnalysisTopic(label!.Trim(), key) { TopicId = staffTopic?.Id };
        this.Context.AnalysisTopics.Add(entry);
        this.Context.SaveChanges();
        return entry;
    }

    private static ContentAnalysis ToEntity(AnalysisResultModel result, long contentId, AnalysisTopic? registry)
    {
        JsonDocument Json(object value) => JsonSerializer.SerializeToDocument(value, _jsonOptions);
        return new ContentAnalysis(contentId, result.InputHash)
        {
            IsCurrent = true,
            IsMetadataOnly = result.IsMetadataOnly,
            NormalizationVersion = result.NormalizationVersion,
            SchemaVersion = result.SchemaVersion,
            PromptVersion = result.PromptVersion,
            LLMId = result.LLMId,
            Model = result.Model,
            Summary = result.Summary,
            KeyFacts = Json(result.KeyFacts),
            Entities = Json(result.Entities),
            Places = Json(result.Places),
            Topics = Json(result.Topics),
            PrimaryTopic = result.PrimaryTopic,
            AnalysisTopicId = registry?.Id,
            SuggestedTags = Json(result.SuggestedTags),
            SuggestedContributor = result.SuggestedContributor,
            Events = Json(result.Events),
            Quotes = Json(result.Quotes),
            // The processes the worker ran are recorded with the analysis.
            Validation = Json(new Dictionary<string, object?>(result.Validation) { ["processes"] = result.Processes.ToString() }),
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
        };
    }

    /// <summary>
    /// Record a failed attempt.
    /// </summary>
    /// <param name="failure"></param>
    /// <returns></returns>
    public AnalysisJob? Fail(AnalysisFailureModel failure)
    {
        var job = FindClaimed(failure);
        if (job == null) return null;
        job.Attempts++;
        job.LastError = failure.Error.Length > 4000 ? failure.Error[..4000] : failure.Error;
        job.LeaseExpiresOn = null;
        if (!failure.IsTransient || job.Attempts >= Math.Max(1, _options.MaxAttempts))
        {
            job.Status = AnalysisJobStatus.Failed;
        }
        else
        {
            // Exponential backoff with jitter, capped at an hour.
            var delay = Math.Min(3600, 30 * Math.Pow(2, job.Attempts - 1)) + Random.Shared.Next(0, 30);
            job.Status = AnalysisJobStatus.Pending;
            job.NextAttemptOn = DateTime.UtcNow.AddSeconds(delay);
        }
        this.Context.CommitTransaction();
        return job;
    }

    /// <summary>
    /// Queue content for analysis again.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public AnalysisJob RequestReanalysis(long contentId)
    {
        var content = this.Context.Contents.AsNoTracking().FirstOrDefault(c => c.Id == contentId) ?? throw new NoContentException("Content does not exist");
        var hash = AnalysisInput.ComputeHash(content, this.Context.IsSummaryHumanOwned(contentId));
        this.Context.ScheduleJob(contentId, hash, AnalysisJobReason.Reanalysis, _options.LifecyclePriority, DateTime.UtcNow, null, true);
        this.Context.CommitTransaction();
        return this.Context.AnalysisJobs.AsNoTracking().First(j => j.ContentId == contentId);
    }

    /// <summary>
    /// Queue a failed job again.
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    public AnalysisJob Replay(long jobId)
    {
        var job = this.Context.AnalysisJobs.FirstOrDefault(j => j.Id == jobId) ?? throw new NoContentException("Analysis job does not exist");
        if (job.Status != AnalysisJobStatus.Failed && job.Status != AnalysisJobStatus.Skipped)
            throw new InvalidOperationException("Only failed or skipped jobs can be replayed.");
        job.Status = AnalysisJobStatus.Pending;
        job.Attempts = 0;
        job.DueOn = DateTime.UtcNow;
        job.NextAttemptOn = null;
        job.LastError = null;
        this.Context.ScheduledAnalysisJobs.Add(job.DueOn);
        this.Context.CommitTransaction();
        return job;
    }

    /// <summary>
    /// The content's job.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public AnalysisJob? FindJob(long contentId) => this.Context.AnalysisJobs.AsNoTracking().FirstOrDefault(j => j.ContentId == contentId);

    /// <summary>
    /// Jobs with the specified status.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    public IEnumerable<AnalysisJob> FindJobs(AnalysisJobStatus status, int qty = 100)
    {
        return this.Context.AnalysisJobs.AsNoTracking()
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.UpdatedOn)
            .Take(Math.Clamp(qty, 1, 1000))
            .ToArray();
    }

    /// <summary>
    /// Job counts by status and reason.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, int> GetQueueCounts()
    {
        var now = DateTime.UtcNow;
        var counts = this.Context.AnalysisJobs.AsNoTracking()
            .GroupBy(j => new { j.Status, j.Reason })
            .Select(g => new { g.Key.Status, g.Key.Reason, Count = g.Count() })
            .ToArray();
        var result = counts.ToDictionary(c => $"{c.Status}:{c.Reason}", c => c.Count);
        result["Due"] = this.Context.AnalysisJobs.AsNoTracking().Count(j => j.Status == AnalysisJobStatus.Pending && j.DueOn <= now);
        return result;
    }

    /// <summary>
    /// The content's current analysis.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public ContentAnalysis? FindCurrent(long contentId)
        => this.Context.ContentAnalyses.AsNoTracking().Include(a => a.AnalysisTopic).FirstOrDefault(a => a.ContentId == contentId && a.IsCurrent);

    /// <summary>
    /// The current analyses of the specified content.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <returns></returns>
    public IDictionary<long, ContentAnalysis> FindCurrent(IEnumerable<long> contentIds)
    {
        var ids = contentIds.Distinct().ToArray();
        var result = new Dictionary<long, ContentAnalysis>();
        foreach (var chunk in ids.Chunk(500))
        {
            foreach (var analysis in this.Context.ContentAnalyses.AsNoTracking().Include(a => a.AnalysisTopic).ThenInclude(t => t!.Topic)
                .Where(a => chunk.Contains(a.ContentId) && a.IsCurrent))
                result[analysis.ContentId] = analysis;
        }
        return result;
    }

    /// <summary>
    /// The ownership records of the content's editorial values.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<ContentFieldOwnership> FindOwnership(long contentId)
        => this.Context.ContentFieldOwnerships.AsNoTracking().Where(o => o.ContentId == contentId).ToArray();

    #endregion
}
