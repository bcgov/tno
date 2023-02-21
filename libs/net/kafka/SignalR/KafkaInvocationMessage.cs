using Microsoft.AspNetCore.SignalR.Protocol;

namespace TNO.Kafka.SignalR;

/// <summary>
/// KafkaInvocationMessage class, provides a serializable model to pass invocation messages to the Kafka back-plane.
/// </summary>
public class KafkaInvocationMessage
{
    #region Properties
    /// <summary>
    /// get/set - The invocation Id.
    /// </summary>
    public string? InvocationId { get; set; }

    /// <summary>
    /// get/set - The target (or method name).
    /// </summary>
    public string Target { get; set; } = "";

    /// <summary>
    /// get/set - Array of arguments sent with the message.
    /// </summary>
    public object?[] Arguments { get; set; } = Array.Empty<object>();

    /// <summary>
    /// get/set - An array of stream Ids.
    /// </summary>
    public string[]? StreamIds { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a KafkaInvocationMessage object.
    /// </summary>
    public KafkaInvocationMessage() { }

    /// <summary>
    /// Creates a new instance of a KafkaInvocationMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    public KafkaInvocationMessage(InvocationMessage message)
    {
        this.InvocationId = message.InvocationId;
        this.Target = message.Target;
        this.Arguments = message.Arguments;
        this.StreamIds = message.StreamIds;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Convert serialized object to invocation message.
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator InvocationMessage(KafkaInvocationMessage obj)
    {
        return new InvocationMessage(obj.InvocationId, obj.Target, obj.Arguments, obj.StreamIds);
    }
    #endregion
}
