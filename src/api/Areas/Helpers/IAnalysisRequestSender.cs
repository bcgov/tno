using TNO.DAL;
using TNO.Kafka.Models;

namespace TNO.API.Helpers;

/// <summary>
/// IAnalysisRequestSender interface, sends analysis requests to the Kafka analysis topics, keyed by
/// content ID.
/// </summary>
public interface IAnalysisRequestSender
{
    /// <summary>
    /// Send the analysis requests the context has made since the last send to the analysis topic.
    /// Throws when Kafka does not accept them.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SendAsync(TNOContext context);

    /// <summary>
    /// Send the requests to the specified analysis topic. Throws when the topic is not one of the
    /// configured analysis topics, or Kafka does not accept them.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="requests"></param>
    /// <returns></returns>
    Task SendAsync(string topic, IEnumerable<AnalysisRequestModel> requests);
}
