using TNO.Entities;

namespace TNO.API.Helpers;

/// <summary>
/// IWorkOrderRequestSender interface, asks the Event Handler to run a work order.
/// </summary>
public interface IWorkOrderRequestSender
{
    /// <summary>
    /// Send the work order to the Kafka work order topic. Throws when Kafka does not accept it.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="requestor"></param>
    /// <returns></returns>
    Task SendAsync(WorkOrder workOrder, string requestor);
}
