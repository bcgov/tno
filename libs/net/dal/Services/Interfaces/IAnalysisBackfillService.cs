using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AnalysisBackfillPreview record, what a backfill would cover.
/// </summary>
/// <param name="Matching">Eligible content in the range.</param>
/// <param name="Excluded">Content in the range excluded by media type or source.</param>
/// <param name="MissingPublicationDate">Content created in the range with no publication date (excluded when the range is by publication date).</param>
/// <param name="AlreadyCurrent">Matching content whose analysis is current (skipped unless forced).</param>
public record AnalysisBackfillPreview(int Matching, int Excluded, int MissingPublicationDate, int AlreadyCurrent);

/// <summary>
/// AnalysisBackfillProgress record, where a backfill stands.
/// </summary>
/// <param name="Backfill">The backfill.</param>
/// <param name="Analyzed">Content the backfill's jobs analyzed.</param>
/// <param name="Superseded">Content whose backfill job was taken over by lifecycle work.</param>
/// <param name="Deleted">Content deleted after it was scheduled.</param>
/// <param name="Failed">Jobs that failed (shown for replay).</param>
/// <param name="Remaining">Jobs still queued or running.</param>
/// <param name="Indexed">Analyzed content whose index request has been dispatched.</param>
/// <param name="IsComplete">Every target is resolved and successful results are searchable.</param>
public record AnalysisBackfillProgress(AnalysisBackfill Backfill, int Analyzed, int Superseded, int Deleted, int Failed, int Remaining, int Indexed, bool IsComplete);

/// <summary>
/// IAnalysisBackfillService interface, administrator-requested analysis of existing content in a
/// date range, through the same job queue as lifecycle work at a lower priority.
/// </summary>
public interface IAnalysisBackfillService : IBaseService
{
    /// <summary>
    /// Count what a backfill would cover.
    /// </summary>
    /// <param name="startOn">Inclusive (UTC).</param>
    /// <param name="endOn">Exclusive (UTC).</param>
    /// <param name="dateField"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    AnalysisBackfillPreview Preview(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, AnalysisBackfillMode mode);

    /// <summary>
    /// Record a new backfill with its criteria and creation high-water mark.
    /// </summary>
    /// <param name="backfill"></param>
    /// <returns></returns>
    AnalysisBackfill Add(AnalysisBackfill backfill);

    /// <summary>
    /// Schedule the backfill's jobs from its checkpoint, in keyset order, checkpointing each batch.
    /// Stops when the backfill is cancelled.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop scheduling and withdraw the backfill's unclaimed jobs; claimed work may finish.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    AnalysisBackfill Cancel(long id);

    /// <summary>
    /// Mark a cancelled or failed backfill to continue from its checkpoint.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    AnalysisBackfill Resume(long id);

    /// <summary>
    /// The most recent backfills.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    IEnumerable<AnalysisBackfillProgress> FindRecent(int qty = 20);

    /// <summary>
    /// A backfill and its progress.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    AnalysisBackfillProgress? FindProgress(long id);
}
