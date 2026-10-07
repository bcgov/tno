using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using TNO.DAL.Analysis;
using TNO.Entities;

namespace TNO.DAL;

/// <summary>
/// TNOContext Content-Analysis hooks.
/// - Field ownership: a save records who set a populated editorial value (and that a person
///   cleared one), so analysis never overwrites a person's or automation's choice.
/// - Scheduling: a save that changes an analysis input upserts the content's analysis job in the
///   same transaction, due after the quiet period.
/// </summary>
public partial class TNOContext
{
    #region Variables
    /// <summary>
    /// The query string parameter (or header 'X-Change-Owner') a service sets to say who is saving.
    /// </summary>
    public const string ChangeOwnerParameter = "owner";
    #endregion

    #region Properties
    /// <summary>
    /// get/set - Who the current save is on behalf of. Null resolves from the request (the 'owner'
    /// query parameter or 'X-Change-Owner' header), and otherwise a person.
    /// </summary>
    public FieldOwner? ChangeOwner { get; set; }

    /// <summary>
    /// get - When the analysis jobs scheduled by saves on this context become due, so the API can
    /// wake the Content-Analysis workers.
    /// </summary>
    public List<DateTime> ScheduledAnalysisJobs { get; } = new();
    #endregion

    #region Methods
    /// <summary>
    /// Who the current save is on behalf of.
    /// </summary>
    /// <returns></returns>
    public FieldOwner ResolveChangeOwner()
    {
        if (this.ChangeOwner.HasValue) return this.ChangeOwner.Value;
        var request = _httpContextAccessor?.HttpContext?.Request;
        var value = request?.Query[ChangeOwnerParameter].FirstOrDefault() ?? request?.Headers["X-Change-Owner"].FirstOrDefault();
        return Enum.TryParse<FieldOwner>(value, true, out var owner) && owner != FieldOwner.Analysis ? owner : FieldOwner.Human;
    }

    /// <summary>
    /// Record ownership of editorial values changed by a person or automation. Analysis records its
    /// own ownership when a result is accepted.
    /// </summary>
    private void RecordFieldOwnership()
    {
        var owner = ResolveChangeOwner();
        if (this.ChangeOwner == FieldOwner.Analysis) return;

        var entries = ChangeTracker.Entries().ToArray();
        var touched = new HashSet<(long ContentId, string Field)>();

        foreach (var entry in entries.Where(e => e.State == EntityState.Modified && e.Entity is Content))
        {
            var content = (Content)entry.Entity;
            if (entry.Property(nameof(Content.Summary)).IsModified)
                SetScalarOwner(content.Id, ContentFieldOwnership.SummaryField, owner, String.IsNullOrWhiteSpace(content.Summary), !String.IsNullOrWhiteSpace((string?)entry.Property(nameof(Content.Summary)).OriginalValue));
            if (entry.Property(nameof(Content.ContributorId)).IsModified)
                SetScalarOwner(content.Id, ContentFieldOwnership.ContributorField, owner, !content.ContributorId.HasValue, entry.Property(nameof(Content.ContributorId)).OriginalValue != null);
        }

        foreach (var entry in entries.Where(e => e.Entity is ContentTag && (e.State == EntityState.Added || e.State == EntityState.Deleted)))
        {
            var tag = (ContentTag)entry.Entity;
            if (tag.ContentId == 0) continue;
            SetItemOwner(tag.ContentId, ContentFieldOwnership.TagField, tag.TagId.ToString(), owner, entry.State == EntityState.Deleted);
            touched.Add((tag.ContentId, ContentFieldOwnership.TagField));
        }

        foreach (var entry in entries.Where(e => e.Entity is ContentTopic && (e.State == EntityState.Added || e.State == EntityState.Deleted)))
        {
            var topic = (ContentTopic)entry.Entity;
            if (topic.ContentId == 0) continue;
            SetItemOwner(topic.ContentId, ContentFieldOwnership.TopicField, topic.TopicId.ToString(), owner, entry.State == EntityState.Deleted);
            touched.Add((topic.ContentId, ContentFieldOwnership.TopicField));
        }

        foreach (var entry in entries.Where(e => e.Entity is Quote && e.State != EntityState.Unchanged && e.State != EntityState.Detached))
        {
            var quote = (Quote)entry.Entity;
            if (quote.ContentId == 0) continue;
            if (entry.State == EntityState.Added && quote.Owner == FieldOwner.Analysis) continue;
            if (entry.State == EntityState.Modified && quote.Owner != owner)
                quote.Owner = owner; // A person who edits a generated quote now owns it.
            else if (entry.State == EntityState.Added)
                quote.Owner = owner;
            var statement = entry.State == EntityState.Modified
                ? (string?)entry.Property(nameof(Quote.Statement)).OriginalValue ?? quote.Statement
                : quote.Statement;
            SetItemOwner(quote.ContentId, ContentFieldOwnership.QuoteField, AnalysisInput.QuoteKey(statement), owner, entry.State == EntityState.Deleted);
            touched.Add((quote.ContentId, ContentFieldOwnership.QuoteField));
        }

        // Removing the last value of a collection clears the field, so analysis does not refill it.
        foreach (var (contentId, field) in touched)
        {
            var remaining = field switch
            {
                ContentFieldOwnership.TagField => CountRemaining<ContentTag>(contentId, this.ContentTags.Where(t => t.ContentId == contentId).Select(t => t.TagId.ToString()).ToArray(), e => e.ContentId, e => e.TagId.ToString()),
                ContentFieldOwnership.TopicField => CountRemaining<ContentTopic>(contentId, this.ContentTopics.Where(t => t.ContentId == contentId).Select(t => t.TopicId.ToString()).ToArray(), e => e.ContentId, e => e.TopicId.ToString()),
                _ => CountRemaining<Quote>(contentId, this.Quotes.Where(q => q.ContentId == contentId).Select(q => q.Id.ToString()).ToArray(), e => e.ContentId, e => e.Id.ToString()),
            };
            var anyDeleted = entries.Any(e => e.State == EntityState.Deleted && e.Entity switch
            {
                ContentTag t => t.ContentId == contentId && field == ContentFieldOwnership.TagField,
                ContentTopic t => t.ContentId == contentId && field == ContentFieldOwnership.TopicField,
                Quote q => q.ContentId == contentId && field == ContentFieldOwnership.QuoteField,
                _ => false,
            });
            if (remaining == 0 && anyDeleted) SetScalarOwner(contentId, field, owner, true, true);
            else if (remaining > 0) ClearFieldMarker(contentId, field);
        }
    }

    /// <summary>
    /// The number of items a content item will have after this save.
    /// </summary>
    private int CountRemaining<T>(long contentId, string[] savedKeys, Func<T, long> getContentId, Func<T, string> getKey) where T : class
    {
        var entries = ChangeTracker.Entries<T>().Where(e => getContentId(e.Entity) == contentId).ToArray();
        var deleted = entries.Where(e => e.State == EntityState.Deleted).Select(e => getKey(e.Entity)).ToHashSet();
        var added = entries.Count(e => e.State == EntityState.Added);
        return savedKeys.Count(k => !deleted.Contains(k)) + added;
    }

    /// <summary>
    /// Record who set a scalar field, or that it was cleared.
    /// </summary>
    private void SetScalarOwner(long contentId, string field, FieldOwner owner, bool isEmpty, bool hadValue)
    {
        var record = FindOwnership(contentId, field, "");
        if (isEmpty)
        {
            // A person or automation removing a value clears the field.
            if (!hadValue) return;
            if (record == null) this.ContentFieldOwnerships.Add(new ContentFieldOwnership(contentId, field, "", owner) { IsCleared = true });
            else
            {
                record.Owner = owner;
                record.IsCleared = true;
                record.AnalysisId = null;
            }
            return;
        }

        // A value with no record is human-owned; only record what differs from that default.
        if (record == null)
        {
            if (owner != FieldOwner.Human) this.ContentFieldOwnerships.Add(new ContentFieldOwnership(contentId, field, "", owner));
        }
        else if (owner == FieldOwner.Human) this.ContentFieldOwnerships.Remove(record);
        else
        {
            record.Owner = owner;
            record.IsCleared = false;
            record.AnalysisId = null;
        }
    }

    /// <summary>
    /// Record who added a collection value, or that a person removed it.
    /// </summary>
    private void SetItemOwner(long contentId, string field, string key, FieldOwner owner, bool isRemoved)
    {
        var record = FindOwnership(contentId, field, key);
        if (isRemoved)
        {
            if (record == null) this.ContentFieldOwnerships.Add(new ContentFieldOwnership(contentId, field, key, owner) { IsCleared = true });
            else
            {
                record.Owner = owner;
                record.IsCleared = true;
                record.AnalysisId = null;
            }
            return;
        }
        if (record == null)
        {
            if (owner != FieldOwner.Human) this.ContentFieldOwnerships.Add(new ContentFieldOwnership(contentId, field, key, owner));
        }
        else if (owner == FieldOwner.Human) this.ContentFieldOwnerships.Remove(record);
        else
        {
            record.Owner = owner;
            record.IsCleared = false;
            record.AnalysisId = null;
        }
    }

    private void ClearFieldMarker(long contentId, string field)
    {
        var record = FindOwnership(contentId, field, "");
        if (record?.IsCleared == true) this.ContentFieldOwnerships.Remove(record);
    }

    /// <summary>
    /// The ownership record, tracked or saved.
    /// </summary>
    private ContentFieldOwnership? FindOwnership(long contentId, string field, string key)
    {
        return ChangeTracker.Entries<ContentFieldOwnership>()
                .Where(e => e.State != EntityState.Deleted && e.State != EntityState.Detached)
                .Select(e => e.Entity)
                .FirstOrDefault(o => o.ContentId == contentId && o.Field == field && o.ValueKey == key)
            ?? this.ContentFieldOwnerships.FirstOrDefault(o => o.ContentId == contentId && o.Field == field && o.ValueKey == key);
    }

    /// <summary>
    /// Whether a person owns the content's summary (so it is an analysis input).
    /// </summary>
    public bool IsSummaryHumanOwned(long contentId)
    {
        if (contentId == 0) return true;
        var record = FindOwnership(contentId, ContentFieldOwnership.SummaryField, "");
        return record == null || (record.Owner == FieldOwner.Human && !record.IsCleared);
    }

    /// <summary>
    /// Upsert the analysis job of every content item whose analysis input this save changes.
    /// </summary>
    private void ScheduleAnalysisJobs()
    {
        var now = DateTime.UtcNow;
        var dueOn = now.AddSeconds(Math.Max(0, _analysisOptions.QuietPeriodSeconds));

        foreach (var entry in ChangeTracker.Entries<Content>().ToArray())
        {
            var content = entry.Entity;
            if (entry.State == EntityState.Added)
            {
                // The job is inserted with the content; EF fills in its key.
                var job = new AnalysisJob(0, AnalysisInput.ComputeHash(content, true), AnalysisJobReason.Lifecycle, _analysisOptions.LifecyclePriority, dueOn) { Content = content };
                this.AnalysisJobs.Add(job);
                this.ScheduledAnalysisJobs.Add(dueOn);
                continue;
            }
            if (entry.State != EntityState.Modified) continue;
            if (!AnalysisInput.Properties.Any(p => entry.Property(p).IsModified)
                && !ChangeTracker.Entries<ContentFieldOwnership>().Any(o => o.Entity.ContentId == content.Id && o.Entity.Field == ContentFieldOwnership.SummaryField && o.State != EntityState.Unchanged))
                continue;

            var hash = AnalysisInput.ComputeHash(content, IsSummaryHumanOwned(content.Id));
            ScheduleJob(content.Id, hash, AnalysisJobReason.Lifecycle, _analysisOptions.LifecyclePriority, dueOn, null);
        }
    }

    /// <summary>
    /// Upsert a content item's job for the specified input. Nothing changes when the job is already
    /// for this input (and not failed), unless 'force' asks for reanalysis.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="inputHash"></param>
    /// <param name="reason"></param>
    /// <param name="priority"></param>
    /// <param name="dueOn"></param>
    /// <param name="backfillId"></param>
    /// <param name="force">Queue even when the job's input is current.</param>
    /// <returns>Whether work was queued.</returns>
    public bool ScheduleJob(long contentId, string inputHash, AnalysisJobReason reason, int priority, DateTime dueOn, long? backfillId, bool force = false)
    {
        var job = ChangeTracker.Entries<AnalysisJob>().Select(e => e.Entity).FirstOrDefault(j => j.ContentId == contentId)
            ?? this.AnalysisJobs.FirstOrDefault(j => j.ContentId == contentId);
        if (job == null)
        {
            job = new AnalysisJob(contentId, inputHash, reason, priority, dueOn) { BackfillId = backfillId };
            this.AnalysisJobs.Add(job);
            this.ScheduledAnalysisJobs.Add(dueOn);
            return true;
        }

        var isCurrent = job.InputHash == inputHash;
        if (isCurrent && !force && job.Status != AnalysisJobStatus.Failed && job.Status != AnalysisJobStatus.Skipped) return false;
        // Backfill never displaces lifecycle work that is still to run.
        if (reason == AnalysisJobReason.Backfill && isCurrent && (job.Status == AnalysisJobStatus.Pending || job.Status == AnalysisJobStatus.Claimed)) return false;

        // Lifecycle work takes over a backfill's job; the job keeps its backfill so the backfill can
        // count it as superseded.
        job.InputHash = inputHash;
        job.Reason = reason;
        job.Priority = priority;
        if (reason == AnalysisJobReason.Backfill) job.BackfillId = backfillId;
        job.Status = AnalysisJobStatus.Pending;
        job.Attempts = 0;
        job.DueOn = dueOn;
        job.NextAttemptOn = null;
        job.LastError = null;
        job.CompletedOn = null;
        this.ScheduledAnalysisJobs.Add(dueOn);
        return true;
    }
    #endregion
}
