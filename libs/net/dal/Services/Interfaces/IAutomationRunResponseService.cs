using TNO.Entities;

namespace TNO.DAL.Services;

public interface IAutomationRunResponseService : IBaseService<AutomationRunResponse, long>
{
    /// <summary>
    /// Find the responses captured for the specified run, ordered as captured.
    /// </summary>
    /// <param name="runId"></param>
    /// <returns></returns>
    IEnumerable<AutomationRunResponse> FindByRun(long runId);

    /// <summary>
    /// Insert a batch of run responses.
    /// </summary>
    /// <param name="responses"></param>
    /// <returns>The number of responses added.</returns>
    int AddRange(IEnumerable<AutomationRunResponse> responses);
}
