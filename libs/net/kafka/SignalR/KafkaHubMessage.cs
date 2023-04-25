using Microsoft.AspNetCore.SignalR.Protocol;

namespace TNO.Kafka.SignalR;

/// <summary>
/// KafkaHubMessage class, provides a serializable message for the Kafka hub back-plane.
/// </summary>
public class KafkaHubMessage
{
    #region Properties
    /// <summary>
    /// get/set - The hub event that occurred.
    /// </summary>
    public HubEvent HubEvent { get; set; }

    /// <summary>
    /// get/set - The user Id or group name to send the message to.
    /// </summary>
    public string[]? Identifiers { get; set; }

    /// <summary>
    /// get/set - The message to send.
    /// </summary>
    public KafkaInvocationMessage Message { get; set; } = default!;

    /// <summary>
    /// get/set - Exclude the following connection ids when broadcasting this message.
    /// </summary>
    public string[]? ExcludedConnectionIds { get; set; }
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of a KafkaHubMessage object.
    /// </summary>
    public KafkaHubMessage() { }

    /// <summary>
    /// Creates a new instance of a KafkaHubMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="message"></param>
    /// <param name="excludedConnectionIds"></param>
    public KafkaHubMessage(HubEvent hubEvent, InvocationMessage message, IEnumerable<string>? excludedConnectionIds = null)
    {
        this.HubEvent = hubEvent;
        this.Message = new KafkaInvocationMessage(message);
        this.ExcludedConnectionIds = excludedConnectionIds?.ToArray();
    }

    /// <summary>
    /// Creates a new instance of a KafkaHubMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="identifier"></param>
    /// <param name="message"></param>
    /// <param name="excludedConnectionIds"></param>
    public KafkaHubMessage(HubEvent hubEvent, string identifier, InvocationMessage message, IEnumerable<string>? excludedConnectionIds = null)
        : this(hubEvent, new[] { identifier }, message, excludedConnectionIds)
    {
    }

    /// <summary>
    /// Creates a new instance of a KafkaHubMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="hubEvent"></param>
    /// <param name="identifiers"></param>
    /// <param name="message"></param>
    /// <param name="excludedConnectionIds"></param>
    public KafkaHubMessage(HubEvent hubEvent, IEnumerable<string> identifiers, InvocationMessage message, IEnumerable<string>? excludedConnectionIds = null)
        : this(hubEvent, message, excludedConnectionIds)
    {
        this.Identifiers = identifiers.ToArray();
    }
    #endregion
}
