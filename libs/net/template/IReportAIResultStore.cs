using TNO.API.Areas.Services.Models.ReportAIResult;

namespace TNO.TemplateEngine;

/// <summary>
/// IReportAIResultStore interface, stores generated AI section results so an unchanged manifest is
/// generated once. The API stores them through the DAL; the reporting service through the API.
/// </summary>
public interface IReportAIResultStore
{
    /// <summary>
    /// Find the result for the specified manifest hash.
    /// </summary>
    /// <param name="hash"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Claim the right to generate a result.
    /// </summary>
    /// <param name="claim"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>The claimed result, or null when another generator holds it or it is complete.</returns>
    Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record the outcome of a claimed result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="completion"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default);
}

/// <summary>
/// NoReportAIResultStore class, a store that keeps nothing, for hosts without result storage.
/// Every request generates.
/// </summary>
public class NoReportAIResultStore : IReportAIResultStore
{
    private long _id;

    /// <inheritdoc/>
    public Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default) => Task.FromResult<ReportAIResultModel?>(null);

    /// <inheritdoc/>
    public Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default)
        => Task.FromResult<ReportAIResultModel?>(new ReportAIResultModel() { Id = Interlocked.Increment(ref _id), Hash = claim.Hash });

    /// <inheritdoc/>
    public Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
