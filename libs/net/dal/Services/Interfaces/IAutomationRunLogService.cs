using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAutomationRunLogService : IBaseService<AutomationRunLog, long>
{
    /// <summary>
    /// Find log entries for the specified run in execution order, with optional filters and paging.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="step">Only entries for this step name.</param>
    /// <param name="action">Only entries for this action name.</param>
    /// <param name="outcome">Only entries with this outcome.</param>
    /// <param name="contentId">Only entries for this content item.</param>
    /// <param name="search">Case-insensitive text match against prompt and response.</param>
    /// <param name="page">1-based page number.</param>
    /// <param name="qty">Page size.</param>
    /// <returns>The page of entries and the total matching count.</returns>
    (IEnumerable<AutomationRunLog> Items, int Total) FindByRun(long runId, string? step = null, string? action = null, string? outcome = null, long? contentId = null, string? search = null, int page = 1, int qty = 100, bool descending = false);

    /// <summary>
    /// Insert a batch of run log entries.
    /// </summary>
    /// <param name="logs"></param>
    /// <returns>The number of entries added.</returns>
    int AddRange(IEnumerable<AutomationRunLog> logs);

    /// <summary>
    /// Delete log entries created before the specified cutoff (UTC). Supports the same-day
    /// retention policy - runs keep their history, only the verbose log is pruned.
    /// </summary>
    /// <param name="cutoffUtc"></param>
    /// <returns>The number of entries deleted.</returns>
    int Prune(DateTime cutoffUtc);
}
