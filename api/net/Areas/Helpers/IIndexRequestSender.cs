using TNO.DAL;

namespace TNO.API.Helpers;

/// <summary>
/// IIndexRequestSender interface, sends the index requests a context has saved to Kafka.
/// </summary>
public interface IIndexRequestSender
{
    /// <summary>
    /// Send the index requests the context has saved since the last send. Throws when Kafka does
    /// not accept them.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    Task SendAsync(TNOContext context);
}
