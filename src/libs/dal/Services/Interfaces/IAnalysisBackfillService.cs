using TNO.API.Areas.Services.Models.ContentAnalysis;
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
/// IAnalysisBackfillService interface, administrator-requested analysis of existing content in a
/// date range, run as a work order by the Event Handler.
/// </summary>
public interface IAnalysisBackfillService : IBaseService
{
    /// <summary>
    /// Count what a backfill would cover.
    /// </summary>
    AnalysisBackfillPreview Preview(DateTime startOn, DateTime endOn, AnalysisBackfillDateField dateField, AnalysisBackfillMode mode);

    /// <summary>
    /// Record a new backfill work order, counting the eligible content in its range now.
    /// </summary>
    WorkOrder Add(AnalysisBackfillConfigurationModel configuration, int? requestorId);

    /// <summary>
    /// The backfill work order.
    /// </summary>
    WorkOrder? FindById(long id);

    /// <summary>
    /// The most recent backfill work orders.
    /// </summary>
    IEnumerable<WorkOrder> FindRecent(int qty = 20);

    /// <summary>
    /// Stop the backfill.
    /// </summary>
    WorkOrder Cancel(long id);

    /// <summary>
    /// Continue a cancelled or failed backfill from its checkpoint.
    /// </summary>
    WorkOrder Resume(long id);

    /// <summary>
    /// The next page of a backfill after the specified content ID.
    /// </summary>
    AnalysisBackfillPageModel FindPage(AnalysisBackfillConfigurationModel configuration, long afterContentId, int quantity = 500);
}
