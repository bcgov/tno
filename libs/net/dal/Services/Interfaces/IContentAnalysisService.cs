using TNO.API.Areas.Services.Models.ContentAnalysis;
using TNO.DAL.Analysis;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// IContentAnalysisService interface, the Content-Analysis inputs, the acceptance of results, and the
/// record of each content item's recent analysis runs. The work itself arrives through Kafka.
/// </summary>
public interface IContentAnalysisService : IBaseService
{
    /// <summary>
    /// The runtime settings (LLM, exclusions).
    /// </summary>
    /// <returns></returns>
    ContentAnalysisSettings GetSettings();

    /// <summary>
    /// The content's current analysis input, or null when the content does not exist.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    AnalysisInputModel? GetInput(long contentId);

    /// <summary>
    /// Accept an analysis when its input is still the content's current input, populate the empty
    /// fields of the processes run, and record the request's run.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    AnalysisSubmitResultModel Submit(AnalysisResultModel result);

    /// <summary>
    /// Record the outcome of an analysis request in the content's metadata. Returns null when the
    /// content does not exist.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="run"></param>
    /// <returns></returns>
    AnalysisMetadata? RecordRun(long contentId, AnalysisRun run);

    /// <summary>
    /// The content's recent analysis runs.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    AnalysisMetadata FindRuns(long contentId);

    /// <summary>
    /// Content whose newest analysis request failed, most recent first.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    IEnumerable<(long ContentId, string Headline, AnalysisRun Run)> FindFailures(int qty = 100);

    /// <summary>
    /// The number of content items whose newest analysis request failed, optionally only those sent
    /// by the specified backfill work order.
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <returns></returns>
    int CountFailures(long? workOrderId = null);

    /// <summary>
    /// Request analysis of the content's current input, even when its analysis is current; the API
    /// sends the request once the action completes.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    SavedAnalysisRequest RequestAnalysis(long contentId, AnalysisRequestReason reason);

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
