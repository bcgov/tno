using TNO.API.Areas.Services.Models.Content;

namespace TNO.DAL.Services;

/// <summary>
/// IReportEvidenceService interface, reads report synthesis evidence from the evidence index.
/// </summary>
public interface IReportEvidenceService
{
    /// <summary>
    /// Find the approved evidence for the specified content. Content without evidence is left out.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default);
}
