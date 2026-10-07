using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.DAL.Analysis;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// IContentAnalysisService interface, the Content-Analysis work queue and the acceptance of results.
/// </summary>
public interface IContentAnalysisService : IBaseService
{
    /// <summary>
    /// The runtime settings.
    /// </summary>
    /// <returns></returns>
    ContentAnalysisSettings GetSettings();

    /// <summary>
    /// Claim due jobs (FOR UPDATE SKIP LOCKED), highest priority then earliest due. Jobs whose lease
    /// lapsed return to the queue. Each claim gets a lease and a new fencing token.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    IEnumerable<AnalysisJob> ClaimJobs(AnalysisClaimRequestModel request);

    /// <summary>
    /// Extend a valid claim's lease.
    /// </summary>
    /// <param name="lease"></param>
    /// <returns>The job, or null when the claim is no longer valid.</returns>
    AnalysisJob? RenewLease(AnalysisLeaseModel lease);

    /// <summary>
    /// The current input of claimed content. Ineligible content ends the job as skipped.
    /// </summary>
    /// <param name="lease"></param>
    /// <returns>The input, or null when the claim is no longer valid.</returns>
    AnalysisInputModel? GetInput(AnalysisLeaseModel lease);

    /// <summary>
    /// Accept an analysis when the content exists and is eligible, the input is current, and the
    /// claim is valid. A duplicate returns the stored result; a stale one is rejected without
    /// populating anything.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    AnalysisSubmitResultModel Submit(AnalysisResultModel result);

    /// <summary>
    /// Record a failed attempt: transient failures retry with backoff until the attempts run out.
    /// </summary>
    /// <param name="failure"></param>
    /// <returns></returns>
    AnalysisJob? Fail(AnalysisFailureModel failure);

    /// <summary>
    /// Queue content for analysis again, even when its analysis is current.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    AnalysisJob RequestReanalysis(long contentId);

    /// <summary>
    /// Queue a failed job again.
    /// </summary>
    /// <param name="jobId"></param>
    /// <returns></returns>
    AnalysisJob Replay(long jobId);

    /// <summary>
    /// The content's job, if any.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    AnalysisJob? FindJob(long contentId);

    /// <summary>
    /// Jobs with the specified status, most recently updated first.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    IEnumerable<AnalysisJob> FindJobs(AnalysisJobStatus status, int qty = 100);

    /// <summary>
    /// Job counts by status and reason.
    /// </summary>
    /// <returns></returns>
    IDictionary<string, int> GetQueueCounts();

    /// <summary>
    /// The content's current analysis.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    ContentAnalysis? FindCurrent(long contentId);

    /// <summary>
    /// The current analyses of the specified content.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <returns></returns>
    IDictionary<long, ContentAnalysis> FindCurrent(IEnumerable<long> contentIds);

    /// <summary>
    /// The ownership records of the content's editorial values.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    IEnumerable<ContentFieldOwnership> FindOwnership(long contentId);
}
