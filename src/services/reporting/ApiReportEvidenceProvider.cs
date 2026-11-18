using TNO.API.Areas.Services.Models.Content;
using TNO.Services;
using TNO.TemplateEngine;

namespace TNO.Services.Reporting;

/// <summary>
/// ApiReportEvidenceProvider class, reads report synthesis evidence through the API.
/// </summary>
public class ApiReportEvidenceProvider : IReportEvidenceProvider
{
    #region Variables
    private const int BatchSize = 5000;
    private readonly IApiService _api;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ApiReportEvidenceProvider object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    public ApiReportEvidenceProvider(IApiService api)
    {
        _api = api;
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<long, ContentEvidenceModel>();
        foreach (var batch in contentIds.Distinct().Chunk(BatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var evidence in await _api.FindReportEvidenceAsync(batch))
                result[evidence.ContentId] = evidence;
        }
        return result;
    }
    #endregion
}
