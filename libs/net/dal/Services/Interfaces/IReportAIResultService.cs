using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// IReportAIResultService interface, stores generated AI section results so an unchanged manifest
/// is generated once.
/// </summary>
public interface IReportAIResultService : IBaseService
{
    /// <summary>
    /// Find the result for the specified manifest hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <returns></returns>
    ReportAIResult? FindByHash(string hash);

    /// <summary>
    /// Atomically claim the right to generate a result. Succeeds when no result exists for the hash,
    /// the last attempt failed, or a previous claim lapsed.
    /// </summary>
    /// <param name="claim"></param>
    /// <param name="username">Who is claiming.</param>
    /// <returns>The claimed (pending) result, or null when another generator holds it or it is complete.</returns>
    ReportAIResult? TryClaim(ReportAIResultClaimModel claim, string username);

    /// <summary>
    /// Record the outcome of a claimed result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completion"></param>
    /// <returns></returns>
    ReportAIResult Complete(long id, ReportAIResultCompletionModel completion);
}
