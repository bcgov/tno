using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AutomationRunResponseService class, provides persistence for the LLM prompt/response records
/// captured during an automation run. Kept separate from the run so large text stays out of the
/// run summary.
/// </summary>
public class AutomationRunResponseService : BaseService<AutomationRunResponse, long>, IAutomationRunResponseService
{
    #region Constructors
    public AutomationRunResponseService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<AutomationRunResponseService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find the responses captured for the specified run, ordered as captured. Capped at 'qty'
    /// rows — responses can be up to 20k characters each, so an unbounded read of a large run
    /// would materialize tens of MB in one request.
    /// </summary>
    /// <param name="runId"></param>
    /// <param name="qty"></param>
    /// <returns></returns>
    public IEnumerable<AutomationRunResponse> FindByRun(long runId, int qty = 500)
    {
        qty = Math.Clamp(qty, 1, 2000);
        return this.Context.AutomationRunResponses.AsNoTracking()
            .Where(r => r.AutomationRunId == runId)
            .OrderBy(r => r.Id)
            .Take(qty)
            .ToArray();
    }

    /// <summary>
    /// Insert a batch of run responses.
    /// </summary>
    /// <param name="responses"></param>
    /// <returns>The number of responses added.</returns>
    public int AddRange(IEnumerable<AutomationRunResponse> responses)
    {
        var items = responses.ToArray();
        if (items.Length == 0) return 0;
        this.Context.AutomationRunResponses.AddRange(items);
        this.Context.CommitTransaction();
        return items.Length;
    }
    #endregion
}
