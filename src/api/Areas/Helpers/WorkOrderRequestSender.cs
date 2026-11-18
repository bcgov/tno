using Microsoft.Extensions.Options;
using TNO.API.Config;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;

namespace TNO.API.Helpers;

/// <summary>
/// WorkOrderRequestSender class, asks the Event Handler to run a work order. The message carries the
/// work order's version, so only the newest message for a work order is acted on.
/// </summary>
public class WorkOrderRequestSender : IWorkOrderRequestSender
{
    #region Variables
    private readonly IKafkaMessenger _kafka;
    private readonly KafkaOptions _options;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a WorkOrderRequestSender object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafka"></param>
    /// <param name="options"></param>
    public WorkOrderRequestSender(IKafkaMessenger kafka, IOptions<KafkaOptions> options)
    {
        _kafka = kafka;
        _options = options.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Send the work order to the Kafka work order topic.
    /// </summary>
    /// <param name="workOrder"></param>
    /// <param name="requestor"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException">The work order topic is not configured.</exception>
    public async Task SendAsync(WorkOrder workOrder, string requestor)
    {
        if (String.IsNullOrWhiteSpace(_options.WorkOrderTopic)) throw new ConfigurationException("Kafka work order topic not configured.");
        _ = await _kafka.SendMessageAsync(_options.WorkOrderTopic, $"{workOrder.Id}",
                new WorkOrderRequestModel(workOrder.Id, workOrder.WorkType, requestor, DateTime.UtcNow) { RequestorId = workOrder.RequestorId, Version = workOrder.Version })
            ?? throw new InvalidOperationException("An unknown error occurred when publishing message to Kafka");
    }
    #endregion
}
