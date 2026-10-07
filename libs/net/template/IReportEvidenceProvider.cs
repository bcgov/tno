using TNO.API.Areas.Services.Models.Content;

namespace TNO.TemplateEngine;

/// <summary>
/// IReportEvidenceProvider interface, provides the analysis evidence (summary, key facts, entities,
/// quotes, and topic) that report synthesis reads in place of a story's full text. The API reads the
/// evidence index; the reporting service reads it through the API.
/// Only evidence subscribers may see is returned: never an unapproved transcript's.
/// </summary>
public interface IReportEvidenceProvider
{
    /// <summary>
    /// Find the evidence for the specified content. Content without evidence is left out.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default);
}

/// <summary>
/// NoReportEvidenceProvider class, for hosts without an evidence index. Stories without analysis in
/// their document are synthesized from their text.
/// </summary>
public class NoReportEvidenceProvider : IReportEvidenceProvider
{
    /// <summary>
    /// Nothing is found.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task<IReadOnlyDictionary<long, ContentEvidenceModel>> FindAsync(IEnumerable<long> contentIds, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<long, ContentEvidenceModel>>(new Dictionary<long, ContentEvidenceModel>());
}
