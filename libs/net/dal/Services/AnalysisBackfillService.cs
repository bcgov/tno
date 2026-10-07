using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.DAL.Analysis;
using TNO.DAL.Config;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AnalysisBackfillService class, administrator-requested analysis of existing content in a date
/// range. Backfill jobs go through the same queue as lifecycle work, at a lower priority; reports
/// never depend on, wait for, or create them.
/// </summary>
public class AnalysisBackfillService : BaseService, IAnalysisBackfillService
{
    #region Variables
    private const int BatchSize = 500;
    private readonly ContentAnalysisOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisBackfillService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public AnalysisBackfillService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IOptions<ContentAnalysisOptions> options,
        ILogger<AnalysisBackfillService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _options = options.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Content in the range, created no later than the high-water mark.
    /// </summary>
    private IQueryable<Content> InRange(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, DateTime? highWaterMark)
    {
        var query = this.Context.Contents.AsNoTracking();
        query = dateField == AnalysisBackfillDateField.PublishedOn
            ? query.Where(c => c.PublishedOn >= startOn && c.PublishedOn < endOn)
            : query.Where(c => c.CreatedOn >= startOn && c.CreatedOn < endOn);
        if (highWaterMark.HasValue) query = query.Where(c => c.CreatedOn <= highWaterMark);
        return query;
    }

    private static IQueryable<Content> Eligible(IQueryable<Content> query, ContentAnalysisSettings settings)
    {
        var mediaTypes = settings.ExcludedMediaTypeIds.ToArray();
        var sources = settings.ExcludedSourceIds.ToArray();
        return query.Where(c => !mediaTypes.Contains(c.MediaTypeId) && (c.SourceId == null || !sources.Contains(c.SourceId.Value)));
    }

    /// <summary>
    /// Count what a backfill would cover.
    /// </summary>
    public AnalysisBackfillPreview Preview(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, AnalysisBackfillMode mode)
    {
        if (endOn <= startOn) throw new ArgumentException("The end must be after the start.");
        var settings = ContentAnalysisSettings.Read(this.Context);
        var range = InRange(startOn, endOn, dateField, null);
        var eligible = Eligible(range, settings);
        var matching = eligible.Count();
        var excluded = range.Count() - matching;
        var missingPublicationDate = dateField == AnalysisBackfillDateField.PublishedOn
            ? this.Context.Contents.AsNoTracking().Count(c => c.PublishedOn == null && c.CreatedOn >= startOn && c.CreatedOn < endOn)
            : 0;

        // Current analysis whose input still matches is skipped unless the backfill is forced.
        var alreadyCurrent = 0;
        if (mode == AnalysisBackfillMode.MissingOrStale)
        {
            long after = 0;
            while (true)
            {
                var batch = eligible.Where(c => c.Id > after).OrderBy(c => c.Id).Take(BatchSize).ToArray();
                if (batch.Length == 0) break;
                after = batch[^1].Id;
                var hashes = CurrentHashes(batch);
                alreadyCurrent += batch.Count(c => hashes.TryGetValue(c.Id, out var hash) && hash.Analysis == hash.Input);
            }
        }
        return new AnalysisBackfillPreview(matching, excluded, missingPublicationDate, alreadyCurrent);
    }

    /// <summary>
    /// The current input hash and current analysis hash of each content item.
    /// </summary>
    private Dictionary<long, (string Input, string? Analysis)> CurrentHashes(IReadOnlyList<Content> contents)
    {
        var ids = contents.Select(c => c.Id).ToArray();
        var ownedSummaries = this.Context.ContentFieldOwnerships.AsNoTracking()
            .Where(o => ids.Contains(o.ContentId) && o.Field == ContentFieldOwnership.SummaryField && o.ValueKey == ""
                && (o.Owner != FieldOwner.Human || o.IsCleared))
            .Select(o => o.ContentId)
            .ToHashSet();
        var analyses = this.Context.ContentAnalyses.AsNoTracking()
            .Where(a => ids.Contains(a.ContentId) && a.IsCurrent)
            .Select(a => new { a.ContentId, a.InputHash })
            .ToDictionary(a => a.ContentId, a => a.InputHash);
        return contents.ToDictionary(
            c => c.Id,
            c => (AnalysisInput.ComputeHash(c, !ownedSummaries.Contains(c.Id)), analyses.TryGetValue(c.Id, out var hash) ? hash : null));
    }

    /// <summary>
    /// Record a new backfill.
    /// </summary>
    public AnalysisBackfill Add(AnalysisBackfill backfill)
    {
        if (backfill.EndOn <= backfill.StartOn) throw new ArgumentException("The end must be after the start.");
        backfill.Status = BackgroundJobStatus.Pending;
        backfill.HighWaterMark = DateTime.UtcNow;
        backfill.CheckpointContentId = 0;
        var settings = ContentAnalysisSettings.Read(this.Context);
        backfill.Total = Eligible(InRange(backfill.StartOn, backfill.EndOn, backfill.DateField, backfill.HighWaterMark), settings).Count();
        this.Context.AnalysisBackfills.Add(backfill);
        this.Context.CommitTransaction();
        return backfill;
    }

    /// <summary>
    /// Schedule the backfill's jobs from its checkpoint.
    /// </summary>
    public async Task RunAsync(long id, CancellationToken cancellationToken = default)
    {
        var backfill = this.Context.AnalysisBackfills.FirstOrDefault(b => b.Id == id) ?? throw new NoContentException("Backfill does not exist");
        if (backfill.Status == BackgroundJobStatus.Cancelled || backfill.Status == BackgroundJobStatus.Completed) return;
        backfill.Status = BackgroundJobStatus.Running;
        backfill.Error = null;
        this.Context.CommitTransaction();

        try
        {
            var settings = ContentAnalysisSettings.Read(this.Context);
            while (!cancellationToken.IsCancellationRequested)
            {
                // A cancellation from another request stops further scheduling.
                var status = this.Context.AnalysisBackfills.AsNoTracking().Where(b => b.Id == id).Select(b => b.Status).First();
                if (status == BackgroundJobStatus.Cancelled) return;

                var batch = Eligible(InRange(backfill.StartOn, backfill.EndOn, backfill.DateField, backfill.HighWaterMark), settings)
                    .Where(c => c.Id > backfill.CheckpointContentId)
                    .OrderBy(c => c.Id)
                    .Take(BatchSize)
                    .ToArray();
                if (batch.Length == 0) break;

                var hashes = CurrentHashes(batch);
                foreach (var content in batch)
                {
                    var (input, analysis) = hashes[content.Id];
                    if (backfill.Mode == AnalysisBackfillMode.MissingOrStale && analysis == input)
                    {
                        backfill.AlreadyCurrent++;
                        continue;
                    }
                    if (this.Context.ScheduleJob(content.Id, input, AnalysisJobReason.Backfill, _options.BackfillPriority, DateTime.UtcNow, backfill.Id, backfill.Mode == AnalysisBackfillMode.Force))
                        backfill.Scheduled++;
                    else
                        backfill.AlreadyCurrent++; // Lifecycle work for the same input is already queued.
                }
                backfill.CheckpointContentId = batch[^1].Id;
                this.Context.CommitTransaction();
                this.Context.ChangeTracker.Clear();
                backfill = this.Context.AnalysisBackfills.First(b => b.Id == id);
                await Task.Yield();
            }

            backfill.CompletedOn = DateTime.UtcNow;
            this.Context.CommitTransaction();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Analysis backfill {id} failed", id);
            this.Context.ChangeTracker.Clear();
            backfill = this.Context.AnalysisBackfills.First(b => b.Id == id);
            backfill.Status = BackgroundJobStatus.Failed;
            backfill.Error = ex.Message;
            this.Context.CommitTransaction();
        }
    }

    /// <summary>
    /// Stop scheduling and withdraw unclaimed jobs.
    /// </summary>
    public AnalysisBackfill Cancel(long id)
    {
        var backfill = this.Context.AnalysisBackfills.FirstOrDefault(b => b.Id == id) ?? throw new NoContentException("Backfill does not exist");
        backfill.Status = BackgroundJobStatus.Cancelled;
        this.Context.CommitTransaction();
        this.Context.AnalysisJobs
            .Where(j => j.BackfillId == id && j.Reason == AnalysisJobReason.Backfill && j.Status == AnalysisJobStatus.Pending)
            .ExecuteDelete();
        return backfill;
    }

    /// <summary>
    /// Continue a cancelled or failed backfill from its checkpoint.
    /// </summary>
    public AnalysisBackfill Resume(long id)
    {
        var backfill = this.Context.AnalysisBackfills.FirstOrDefault(b => b.Id == id) ?? throw new NoContentException("Backfill does not exist");
        if (backfill.Status != BackgroundJobStatus.Cancelled && backfill.Status != BackgroundJobStatus.Failed && backfill.Status != BackgroundJobStatus.Running)
            throw new InvalidOperationException("Only a cancelled, failed, or interrupted backfill can be resumed.");
        backfill.Status = BackgroundJobStatus.Pending;
        backfill.CompletedOn = null;
        this.Context.CommitTransaction();
        return backfill;
    }

    /// <summary>
    /// The most recent backfills with their progress.
    /// </summary>
    public IEnumerable<AnalysisBackfillProgress> FindRecent(int qty = 20)
    {
        return this.Context.AnalysisBackfills.AsNoTracking()
            .OrderByDescending(b => b.Id)
            .Take(Math.Clamp(qty, 1, 100))
            .ToArray()
            .Select(Progress)
            .ToArray();
    }

    /// <summary>
    /// A backfill with its progress.
    /// </summary>
    public AnalysisBackfillProgress? FindProgress(long id)
    {
        var backfill = this.Context.AnalysisBackfills.AsNoTracking().FirstOrDefault(b => b.Id == id);
        return backfill == null ? null : Progress(backfill);
    }

    private AnalysisBackfillProgress Progress(AnalysisBackfill backfill)
    {
        var jobs = this.Context.AnalysisJobs.AsNoTracking()
            .Where(j => j.BackfillId == backfill.Id)
            .GroupBy(j => new { j.Status, IsBackfill = j.Reason == AnalysisJobReason.Backfill })
            .Select(g => new { g.Key.Status, g.Key.IsBackfill, Count = g.Count() })
            .ToArray();
        int Count(Func<AnalysisJobStatus, bool> status, bool? isBackfill = null)
            => jobs.Where(j => status(j.Status) && (isBackfill == null || j.IsBackfill == isBackfill)).Sum(j => j.Count);

        var analyzed = Count(s => s == AnalysisJobStatus.Completed, true);
        var superseded = Count(_ => true, false);
        var failed = Count(s => s == AnalysisJobStatus.Failed, true);
        var remaining = Count(s => s == AnalysisJobStatus.Pending || s == AnalysisJobStatus.Claimed, true);
        var deleted = Math.Max(0, backfill.Scheduled - jobs.Sum(j => j.Count));

        // Accepting an analysis sends its index request before the submission succeeds.
        var indexed = analyzed;

        var isComplete = backfill.CompletedOn.HasValue && backfill.Status != BackgroundJobStatus.Cancelled && remaining == 0;
        if (isComplete && backfill.Status != BackgroundJobStatus.Completed)
        {
            backfill.Status = BackgroundJobStatus.Completed;
            this.Context.AnalysisBackfills.Where(b => b.Id == backfill.Id && b.Status == BackgroundJobStatus.Running)
                .ExecuteUpdate(setters => setters.SetProperty(b => b.Status, BackgroundJobStatus.Completed));
        }
        return new AnalysisBackfillProgress(backfill, analyzed, superseded, deleted, failed, remaining, indexed, isComplete);
    }
    #endregion
}
