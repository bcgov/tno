using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAutomationRunService : IBaseService<AutomationRun, long>
{
    /// <summary>
    /// Find automation runs, optionally filtered by profile, ordered by most recent first.
    /// Capped at 'qty' rows so the (polled) run list stays bounded as history grows.
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    IEnumerable<AutomationRun> Find(int? profileId, int qty = 100);

    /// <summary>
    /// Atomically claim a queued run by transitioning it from Draft to Running.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns>Whether this caller claimed the run.</returns>
    bool TryClaim(long runId);

    /// <summary>
    /// Delete automation runs that started more than the specified number of days ago.
    /// </summary>
    /// <param name="retentionDays"></param>
    /// <returns>The number of runs deleted.</returns>
    int DeleteOlderThan(int retentionDays);
}
