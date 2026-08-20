using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AutomationRunLogService class, provides persistence for the engine decision log captured
/// during a v2 automation run. Every decision is recorded - LLM analyses, condition gates,
/// exclusions, skips, flushes - so entries are pruned on a same-day retention rather than the
/// run-history retention.
/// </summary>
public class AutomationRunLogService : BaseService<AutomationRunLog, long>, IAutomationRunLogService
{
    #region Constructors
    public AutomationRunLogService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<AutomationRunLogService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find log entries for the specified run in execution order, with optional filters and paging.
    /// Paged because a run over a full day of content produces thousands of entries, each holding
    /// prompt text - an unbounded read would materialize tens of MB in one request.
    /// </summary>
    public (IEnumerable<AutomationRunLog> Items, int Total) FindByRun(long runId, string? step = null, string? action = null, string? outcome = null, long? contentId = null, string? search = null, int page = 1, int qty = 100, bool descending = false)
    {
        page = Math.Max(1, page);
        qty = Math.Clamp(qty, 1, 500);

        var query = this.Context.AutomationRunLogs.AsNoTracking()
            .Where(l => l.AutomationRunId == runId);
        if (!string.IsNullOrWhiteSpace(step)) query = query.Where(l => l.StepName == step);
        if (!string.IsNullOrWhiteSpace(action)) query = query.Where(l => l.ActionName == action || l.AnalysisName == action);
        if (!string.IsNullOrWhiteSpace(outcome)) query = query.Where(l => l.Outcome == outcome);
        if (contentId.HasValue) query = query.Where(l => l.ContentId == contentId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(l => (l.Prompt != null && EF.Functions.ILike(l.Prompt, term))
                || (l.Response != null && EF.Functions.ILike(l.Response, term)));
        }

        var total = query.Count();
        var items = (descending ? query.OrderByDescending(l => l.Id) : query.OrderBy(l => l.Id))
            .Skip((page - 1) * qty)
            .Take(qty)
            .ToArray();
        return (items, total);
    }

    /// <summary>
    /// Insert a batch of run log entries.
    /// </summary>
    public int AddRange(IEnumerable<AutomationRunLog> logs)
    {
        var items = logs.ToArray();
        if (items.Length == 0) return 0;
        this.Context.AutomationRunLogs.AddRange(items);
        this.Context.CommitTransaction();
        return items.Length;
    }

    /// <summary>
    /// Delete log entries created before the specified cutoff (UTC) without loading them.
    /// </summary>
    public int Prune(DateTime cutoffUtc)
    {
        return this.Context.AutomationRunLogs
            .Where(l => l.CreatedOn < cutoffUtc)
            .ExecuteDelete();
    }
    #endregion
}
