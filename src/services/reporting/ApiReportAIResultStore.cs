using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Services;
using TNO.TemplateEngine;

namespace TNO.Services.Reporting;

/// <summary>
/// ApiReportAIResultStore class, stores generated AI section results through the API, so a send
/// reuses the result a preview already generated, and a retry or resend never generates again.
/// </summary>
public class ApiReportAIResultStore : IReportAIResultStore
{
    #region Variables
    private readonly IApiService _api;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ApiReportAIResultStore object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    public ApiReportAIResultStore(IApiService api)
    {
        _api = api;
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default)
        => _api.FindReportAIResultAsync(hash);

    /// <inheritdoc/>
    public Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default)
        => _api.ClaimReportAIResultAsync(claim);

    /// <inheritdoc/>
    public async Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default)
        => await _api.CompleteReportAIResultAsync(id, completion);
    #endregion
}
