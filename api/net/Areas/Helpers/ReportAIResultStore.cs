using TNO.API.Areas.Services.Models.ReportAIResult;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.TemplateEngine;

namespace TNO.API.Helpers;

/// <summary>
/// ReportAIResultStore class, stores generated AI section results in the database, so previews and
/// views in the API and sends in the reporting service share one result per manifest.
/// </summary>
public class ReportAIResultStore : IReportAIResultStore
{
    #region Variables
    private readonly IReportAIResultService _service;
    private readonly IHttpContextAccessor _httpContextAccessor;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportAIResultStore object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="httpContextAccessor"></param>
    public ReportAIResultStore(IReportAIResultService service, IHttpContextAccessor httpContextAccessor)
    {
        _service = service;
        _httpContextAccessor = httpContextAccessor;
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public Task<ReportAIResultModel?> FindAsync(string hash, CancellationToken cancellationToken = default)
    {
        var result = _service.FindByHash(hash);
        return Task.FromResult(result == null ? null : new ReportAIResultModel(result));
    }

    /// <inheritdoc/>
    public Task<ReportAIResultModel?> TryClaimAsync(ReportAIResultClaimModel claim, CancellationToken cancellationToken = default)
    {
        var username = _httpContextAccessor.HttpContext?.User.GetUsername() ?? "api";
        var result = _service.TryClaim(claim, username);
        return Task.FromResult(result == null ? null : new ReportAIResultModel(result));
    }

    /// <inheritdoc/>
    public Task CompleteAsync(long id, ReportAIResultCompletionModel completion, CancellationToken cancellationToken = default)
    {
        _service.Complete(id, completion);
        return Task.CompletedTask;
    }
    #endregion
}
