using TNO.API.Areas.Services.Models.Content;
using TNO.DAL.Services;
using TNO.TemplateEngine;

namespace TNO.API.Helpers;

/// <summary>
/// ReportEvidenceProvider class, reads report synthesis evidence from the evidence index for
/// previews and views generated in the API.
/// </summary>
public class ReportEvidenceProvider : IReportEvidenceProvider
{
    #region Variables
    private readonly IReportEvidenceService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportEvidenceProvider object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public ReportEvidenceProvider(IReportEvidenceService service)
    {
        _service = service;
    }
    #endregion

    #region Methods
    /// <inheritdoc/>
    public Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default)
        => _service.FindAsync(contentIds, cancellationToken);
    #endregion
}
