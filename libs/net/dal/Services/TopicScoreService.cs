using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Config;
using TNO.DAL.Models;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// TopicScoreService class, calculates topic scores from the topic score rules.
/// </summary>
public class TopicScoreService : BaseService, ITopicScoreService
{
    #region Variables
    private const int BatchSize = 200;
    #endregion

    #region Properties
    /// <summary>
    /// get - The time zone rule times are expressed in.
    /// </summary>
    public TimeZoneInfo TimeZone { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TopicScoreService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IOptions<TopicScoreOptions> options,
        ILogger<TopicScoreService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        this.TimeZone = DateTimeExtensions.ResolveTimeZone(options.Value.TimeZone);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Calculate the score for the specified input using its source's rules and default score.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="evaluateAll"></param>
    /// <returns></returns>
    public TopicScoreResult Calculate(TopicScoreInput input, bool evaluateAll = false)
    {
        if (!input.SourceId.HasValue) return new TopicScoreResult(0, null, false, Array.Empty<TopicScoreRuleEvaluation>());
        var rules = this.Context.TopicScoreRules.AsNoTracking().Where(r => r.SourceId == input.SourceId).ToArray();
        var defaultScore = this.Context.Sources.AsNoTracking().Where(s => s.Id == input.SourceId).Select(s => s.TopicDefaultScore).FirstOrDefault();
        return TopicScoreCalculator.Calculate(rules, defaultScore, input, this.TimeZone, evaluateAll);
    }

    /// <summary>
    /// Build the scoring input for saved content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public TopicScoreInput? GetInput(long contentId)
    {
        var content = this.Context.Contents.AsNoTracking()
            .Where(c => c.Id == contentId)
            .Select(c => new
            {
                c.SourceId,
                c.SeriesId,
                c.Section,
                c.Page,
                c.Body,
                c.PublishedOn,
                HasImageFile = c.FileReferences.Any(f => f.ContentType.StartsWith("image/")),
            })
            .FirstOrDefault();
        if (content == null) return null;
        return TopicScoreInput.From(content.SourceId, content.SeriesId, content.Section, content.Page, content.Body, content.PublishedOn, content.HasImageFile);
    }

    /// <summary>
    /// Whether saved content is scored.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public bool IsEligible(long contentId)
    {
        var content = this.Context.Contents.AsNoTracking()
            .Where(c => c.Id == contentId)
            .Select(c => new
            {
                c.ContentType,
                SourceUseInTopics = c.Source != null && c.Source.UseInTopics,
                SeriesUseInTopics = c.Series != null && c.Series.UseInTopics,
            })
            .FirstOrDefault();
        return content != null && TopicScoreCalculator.IsEligible(content.ContentType, content.SourceUseInTopics, content.SeriesUseInTopics);
    }

    /// <summary>
    /// Whether content with these values is scored.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="sourceId"></param>
    /// <param name="seriesId"></param>
    /// <returns></returns>
    public bool IsEligible(ContentType contentType, int? sourceId, int? seriesId)
    {
        var sourceUseInTopics = sourceId.HasValue && this.Context.Sources.AsNoTracking().Any(s => s.Id == sourceId && s.UseInTopics);
        var seriesUseInTopics = seriesId.HasValue && this.Context.Series.AsNoTracking().Any(s => s.Id == seriesId && s.UseInTopics);
        return TopicScoreCalculator.IsEligible(contentType, sourceUseInTopics, seriesUseInTopics);
    }

    /// <summary>
    /// Prepare the topics of content that is about to be added.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="incomingScoresAreOverrides"></param>
    public void PrepareNewContentTopics(Content content, bool incomingScoresAreOverrides)
    {
        foreach (var topic in content.TopicsManyToMany)
        {
            topic.IsScoreOverridden = incomingScoresAreOverrides && topic.Score > 0;
            if (topic.IsScoreOverridden) topic.ScoreRuleId = null;
        }

        if (content.TopicsManyToMany.Count > 0 || !IsEligible(content.ContentType, content.SourceId, content.SeriesId)) return;

        // Only content a rule or source default scores is given the placeholder topic, as before.
        var input = TopicScoreInput.From(content.SourceId, content.SeriesId, content.Section, content.Page, content.Body, content.PublishedOn,
            content.FileReferences.Any(f => TNOContext.IsImageFile(f.ContentType)));
        var result = Calculate(input);
        if (!result.RuleId.HasValue && !result.IsSourceDefault) return;

        var systemTopic = this.Context.Topics.AsNoTracking().FirstOrDefault(t => t.IsSystem);
        if (systemTopic == null)
        {
            this.Logger.LogWarning("The system topic does not exist, so content is not given a topic score.");
            return;
        }
        content.TopicsManyToMany.Add(new ContentTopic(content.Id, systemTopic.Id, result.Score) { ScoreRuleId = result.RuleId });
    }

    /// <summary>
    /// Calculate the score of each of the specified content items. Ineligible content scores 0.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <returns></returns>
    public IDictionary<long, int> CalculateScores(IEnumerable<long> contentIds)
    {
        var ids = contentIds.Distinct().ToArray();
        var result = new Dictionary<long, int>();
        if (ids.Length == 0) return result;

        var scorer = new BatchScorer(this.Context, this.TimeZone);
        foreach (var chunk in ids.Chunk(BatchSize))
        {
            foreach (var item in LoadScoringItems(this.Context.Contents.AsNoTracking().Where(c => chunk.Contains(c.Id))))
                result[item.Id] = scorer.Calculate(item)?.Score ?? 0;
        }
        return result;
    }

    /// <summary>
    /// Sources that use topics, with their rule count and default score.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TopicScoreSourceSummary> FindSourceSummaries()
    {
        return this.Context.Sources.AsNoTracking()
            .Where(s => s.UseInTopics || this.Context.Series.Any(ss => ss.SourceId == s.Id && ss.UseInTopics) || this.Context.TopicScoreRules.Any(r => r.SourceId == s.Id))
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name)
            .Select(s => new TopicScoreSourceSummary(
                s.Id,
                s.Code,
                s.Name,
                s.UseInTopics,
                this.Context.Series.Any(ss => ss.SourceId == s.Id && ss.UseInTopics),
                this.Context.TopicScoreRules.Count(r => r.SourceId == s.Id),
                s.TopicDefaultScore))
            .ToArray();
    }

    /// <summary>
    /// Sections known for a source.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    public IEnumerable<string> FindKnownSections(int sourceId)
    {
        var since = DateTime.UtcNow.AddYears(-1);
        var ruleSections = this.Context.TopicScoreRules.AsNoTracking()
            .Where(r => r.SourceId == sourceId && r.Section != null && r.Section != "")
            .Select(r => r.Section!)
            .Distinct()
            .ToArray();
        var contentSections = this.Context.Contents.AsNoTracking()
            .Where(c => c.SourceId == sourceId && c.Section != "" && c.PublishedOn >= since)
            .Select(c => c.Section)
            .Distinct()
            .Take(500)
            .ToArray();
        return ruleSections.Concat(contentSections)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(s => s, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Set the score used when none of a source's rules match.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="score"></param>
    /// <returns></returns>
    public TopicScoreSourceSummary UpdateSourceDefaultScore(int sourceId, int? score)
    {
        if (score < 0) throw new ArgumentException("The default score must be a whole number of 0 or more.", nameof(score));
        var source = this.Context.Sources.FirstOrDefault(s => s.Id == sourceId) ?? throw new NoContentException("Source does not exist");
        source.TopicDefaultScore = score;
        this.Context.CommitTransaction();
        return FindSourceSummaries().FirstOrDefault(s => s.Id == sourceId)
            ?? new TopicScoreSourceSummary(source.Id, source.Code, source.Name, source.UseInTopics, false, 0, source.TopicDefaultScore);
    }

    /// <summary>
    /// Clear the override on a content topic so its score is recalculated.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="topicId"></param>
    /// <returns></returns>
    public ContentTopic ResetOverride(long contentId, int topicId)
    {
        var topic = this.Context.ContentTopics.FirstOrDefault(t => t.ContentId == contentId && t.TopicId == topicId)
            ?? throw new NoContentException("Content topic does not exist");
        if (topic.IsScoreOverridden)
        {
            // The save hook recalculates the score when the override is cleared.
            topic.IsScoreOverridden = false;
            this.Context.CommitTransaction();
        }
        return topic;
    }

    /// <summary>
    /// Count the content in the range whose calculated score would change.
    /// </summary>
    /// <param name="startOn"></param>
    /// <param name="endOn"></param>
    /// <param name="sourceIds"></param>
    /// <returns></returns>
    public TopicRescorePreview PreviewRescore(DateTime startOn, DateTime endOn, IEnumerable<int>? sourceIds)
    {
        var sources = sourceIds?.Distinct().ToArray() ?? Array.Empty<int>();
        var scorer = new BatchScorer(this.Context, this.TimeZone);
        var total = 0;
        var changed = 0;
        long after = 0;
        while (true)
        {
            var ids = RescoreQuery(startOn, endOn, sources, after).Take(BatchSize).ToArray();
            if (ids.Length == 0) break;
            after = ids[^1];
            total += ids.Length;

            var items = LoadScoringItems(this.Context.Contents.AsNoTracking().Where(c => ids.Contains(c.Id)));
            var topics = this.Context.ContentTopics.AsNoTracking()
                .Where(t => ids.Contains(t.ContentId) && !t.IsScoreOverridden)
                .ToArray()
                .ToLookup(t => t.ContentId);
            foreach (var item in items)
            {
                var result = scorer.Calculate(item);
                if (result != null && topics[item.Id].Any(t => t.Score != result.Score || t.ScoreRuleId != result.RuleId)) changed++;
            }
        }
        return new TopicRescorePreview(total, changed);
    }

    /// <summary>
    /// Record a new rescore job.
    /// </summary>
    /// <param name="startOn"></param>
    /// <param name="endOn"></param>
    /// <param name="sourceIds"></param>
    /// <returns></returns>
    public TopicRescoreJob AddRescoreJob(DateTime startOn, DateTime endOn, IEnumerable<int>? sourceIds)
    {
        if (endOn <= startOn) throw new ArgumentException("The end date must be after the start date.");
        var job = new TopicRescoreJob(startOn, endOn, sourceIds);
        this.Context.TopicRescoreJobs.Add(job);
        this.Context.CommitTransaction();
        return job;
    }

    /// <summary>
    /// Find a rescore job.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public TopicRescoreJob? FindRescoreJob(long id)
    {
        return this.Context.TopicRescoreJobs.AsNoTracking().FirstOrDefault(j => j.Id == id);
    }

    /// <summary>
    /// The most recent rescore jobs.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    public IEnumerable<TopicRescoreJob> FindRescoreJobs(int qty = 10)
    {
        return this.Context.TopicRescoreJobs.AsNoTracking().OrderByDescending(j => j.Id).Take(qty).ToArray();
    }

    /// <summary>
    /// Run a rescore job, recording progress on the job.
    /// </summary>
    /// <param name="jobId"></param>
    /// <param name="onBatchSaved">Called after each batch commits (to send its index requests).</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task RunRescoreJobAsync(long jobId, Func<Task> onBatchSaved, CancellationToken cancellationToken = default)
    {
        var job = this.Context.TopicRescoreJobs.FirstOrDefault(j => j.Id == jobId) ?? throw new NoContentException("Rescore job does not exist");
        if (job.Status != BackgroundJobStatus.Pending) return;

        job.Status = BackgroundJobStatus.Running;
        job.StartedOn = DateTime.UtcNow;
        job.Total = RescoreQuery(job.StartOn, job.EndOn, job.SourceIds, 0).Count();
        this.Context.CommitTransaction();

        var scorer = new BatchScorer(this.Context, this.TimeZone);
        long after = 0;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var ids = RescoreQuery(job.StartOn, job.EndOn, job.SourceIds, after).Take(BatchSize).ToArray();
                if (ids.Length == 0) break;
                after = ids[^1];

                var changedIds = new List<long>();
                var items = LoadScoringItems(this.Context.Contents.AsNoTracking().Where(c => ids.Contains(c.Id)));
                var topics = this.Context.ContentTopics
                    .Where(t => ids.Contains(t.ContentId) && !t.IsScoreOverridden)
                    .ToArray()
                    .ToLookup(t => t.ContentId);
                foreach (var item in items)
                {
                    try
                    {
                        var result = scorer.Calculate(item);
                        if (result == null) continue;
                        var changed = false;
                        foreach (var topic in topics[item.Id].Where(t => t.Score != result.Score || t.ScoreRuleId != result.RuleId))
                        {
                            topic.Score = result.Score;
                            topic.ScoreRuleId = result.RuleId;
                            changed = true;
                        }
                        if (changed) changedIds.Add(item.Id);
                    }
                    catch (Exception ex)
                    {
                        job.Failed++;
                        job.Error = $"Content {item.Id}: {ex.Message}";
                        this.Logger.LogError(ex, "Topic rescore failed for content {contentId}", item.Id);
                    }
                }

                job.Processed += ids.Length;
                job.Changed += changedIds.Count;
                // Content whose score changed is re-indexed once the scores commit.
                foreach (var id in changedIds) this.Context.RequestIndex(id);
                this.Context.CommitTransaction();
                await onBatchSaved();
                this.Context.ChangeTracker.Clear();
                job = this.Context.TopicRescoreJobs.First(j => j.Id == jobId);
            }

            job.Status = cancellationToken.IsCancellationRequested ? BackgroundJobStatus.Cancelled : BackgroundJobStatus.Completed;
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Topic rescore job {jobId} failed", jobId);
            this.Context.ChangeTracker.Clear();
            job = this.Context.TopicRescoreJobs.First(j => j.Id == jobId);
            job.Status = BackgroundJobStatus.Failed;
            job.Error = ex.Message;
        }
        job.CompletedOn = DateTime.UtcNow;
        this.Context.CommitTransaction();
    }

    /// <summary>
    /// Content in the rescore range that has at least one calculated topic score.
    /// </summary>
    /// <param name="startOn"></param>
    /// <param name="endOn"></param>
    /// <param name="sourceIds"></param>
    /// <param name="after">Keyset position.</param>
    /// <returns></returns>
    private IQueryable<long> RescoreQuery(DateTime startOn, DateTime endOn, int[] sourceIds, long after)
    {
        var query = this.Context.Contents.AsNoTracking()
            .Where(c => c.PublishedOn >= startOn && c.PublishedOn < endOn && c.Id > after
                && c.TopicsManyToMany.Any(t => !t.IsScoreOverridden));
        if (sourceIds.Length > 0) query = query.Where(c => c.SourceId.HasValue && sourceIds.Contains(c.SourceId.Value));
        return query.OrderBy(c => c.Id).Select(c => c.Id);
    }

    /// <summary>
    /// Load the values scoring needs for each content item.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    private static ScoringItem[] LoadScoringItems(IQueryable<Content> query)
    {
        return query
            .Select(c => new ScoringItem(
                c.Id,
                c.ContentType,
                c.SourceId,
                c.SeriesId,
                c.Section,
                c.Page,
                c.Body,
                c.PublishedOn,
                c.FileReferences.Any(f => f.ContentType.StartsWith("image/"))))
            .ToArray();
    }
    #endregion

    #region Classes
    /// <summary>
    /// The values scoring reads from a content item.
    /// </summary>
    private record ScoringItem(long Id, ContentType ContentType, int? SourceId, int? SeriesId, string Section, string Page, string Body, DateTime? PublishedOn, bool HasImageFile);

    /// <summary>
    /// Scores many content items, loading each source's rules and eligibility once.
    /// </summary>
    private class BatchScorer
    {
        private readonly TNOContext _context;
        private readonly TimeZoneInfo _timeZone;
        private readonly Dictionary<int, (bool UseInTopics, int? DefaultScore, TopicScoreRule[] Rules)> _sources = new();
        private readonly HashSet<int> _topicSeries;

        public BatchScorer(TNOContext context, TimeZoneInfo timeZone)
        {
            _context = context;
            _timeZone = timeZone;
            _topicSeries = context.Series.AsNoTracking().Where(s => s.UseInTopics).Select(s => s.Id).ToHashSet();
        }

        /// <summary>
        /// Calculate the score, or null when the content is not scored.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public TopicScoreResult? Calculate(ScoringItem item)
        {
            if (!item.SourceId.HasValue) return null;
            if (!_sources.TryGetValue(item.SourceId.Value, out var source))
            {
                var values = _context.Sources.AsNoTracking().Where(s => s.Id == item.SourceId).Select(s => new { s.UseInTopics, s.TopicDefaultScore }).FirstOrDefault();
                source = (values?.UseInTopics ?? false, values?.TopicDefaultScore, _context.TopicScoreRules.AsNoTracking().Where(r => r.SourceId == item.SourceId).ToArray());
                _sources[item.SourceId.Value] = source;
            }
            var seriesUseInTopics = item.SeriesId.HasValue && _topicSeries.Contains(item.SeriesId.Value);
            if (!TopicScoreCalculator.IsEligible(item.ContentType, source.UseInTopics, seriesUseInTopics)) return null;

            var input = TopicScoreInput.From(item.SourceId, item.SeriesId, item.Section, item.Page, item.Body, item.PublishedOn, item.HasImageFile);
            return TopicScoreCalculator.Calculate(source.Rules, source.DefaultScore, input, _timeZone);
        }
    }
    #endregion
}
