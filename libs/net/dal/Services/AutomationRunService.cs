using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AutomationRunService class, provides persistence for automation run history with retention pruning.
/// </summary>
public class AutomationRunService : BaseService<AutomationRun, long>, IAutomationRunService
{
    #region Constructors
    public AutomationRunService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<AutomationRunService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find automation runs, optionally filtered by profile, ordered by most recent first.
    /// </summary>
    /// <param name="profileId"></param>
    /// <returns></returns>
    public IEnumerable<AutomationRun> Find(int? profileId)
    {
        var query = this.Context.AutomationRuns.AsNoTracking().AsQueryable();
        if (profileId.HasValue) query = query.Where(r => r.AutomationProfileId == profileId.Value);
        // Select without the Summary column. Summaries can be many MB each and the run list (which
        // the UI polls) never needs them - it would otherwise read and serialize tens of MB per poll.
        // The full summary is available through the run detail and diff endpoints. Project to an
        // anonymous type in SQL (so only these columns are read), then map to the entity in memory.
        return query
            .OrderByDescending(r => r.StartedOn)
            .Select(r => new
            {
                r.Id,
                r.AutomationProfileId,
                r.Status,
                r.Trigger,
                r.Note,
                r.StartedOn,
                r.CompletedOn,
                // Activity heartbeat: when this run last posted a response record.
                LastResponseOn = this.Context.AutomationRunResponses
                    .Where(x => x.AutomationRunId == r.Id)
                    .Max(x => (DateTime?)x.CreatedOn),
            })
            .ToArray()
            .Select(r => new AutomationRun
            {
                Id = r.Id,
                AutomationProfileId = r.AutomationProfileId,
                Status = r.Status,
                Trigger = r.Trigger,
                Note = r.Note,
                StartedOn = r.StartedOn,
                CompletedOn = r.CompletedOn,
                LastResponseOn = r.LastResponseOn,
            })
            .ToArray();
    }

    /// <summary>
    /// Atomically claim a queued run by transitioning it from Draft to Running.
    /// Only one caller can win the claim; used so horizontally scaled automation service
    /// instances never execute the same run.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns>Whether this caller claimed the run.</returns>
    public bool TryClaim(long runId)
    {
        var affected = this.Context.AutomationRuns
            .Where(r => r.Id == runId && r.Status == AutomationRunStatus.Draft)
            .ExecuteUpdate(setters => setters
                .SetProperty(r => r.Status, AutomationRunStatus.Running)
                .SetProperty(r => r.UpdatedOn, DateTime.UtcNow)
                .SetProperty(r => r.Version, r => r.Version + 1));
        return affected == 1;
    }

    /// <summary>
    /// Delete automation runs that started more than the specified number of days ago.
    /// </summary>
    /// <param name="retentionDays"></param>
    /// <returns>The number of runs deleted.</returns>
    public int DeleteOlderThan(int retentionDays)
    {
        if (retentionDays <= 0) return 0;

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
        return this.Context.AutomationRuns
            .Where(r => r.StartedOn < cutoff)
            .ExecuteDelete();
    }
    #endregion
}
