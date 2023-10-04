using TNO.Core.Exceptions;

namespace TNO.API.Helpers;

/// <summary>
/// IWorkOrderHelper interface, provides methods to help with work orders.
/// </summary>
public interface IWorkOrderHelper
{
    /// <summary>
    /// Determine if the content should be auto transcribed.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public bool ShouldAutoTranscribe(long contentId);

    /// <summary>
    /// Request a transcript for the specified 'contentId'.
    /// Only allow one active transcript request.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    Task<Entities.WorkOrder> RequestTranscriptionAsync(long contentId, bool force = false);

    /// <summary>
    /// Request a natural language processing for the specified 'contentId'.
    /// Only allow one active nlp request.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="force">Whether to force a request regardless of the prior requests state</param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="NotAuthorizedException"></exception>
    Task<Entities.WorkOrder> RequestNLPAsync(long contentId, bool force = false);
}
