using Microsoft.AspNetCore.SignalR.Protocol;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

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
    public MessageTarget Target { get; set; }

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
        if (!Enum.TryParse<MessageTarget>(message.Target, out MessageTarget target)) throw new ConfigurationException($"Invocation message target '{message.Target}' does not exist");
        this.InvocationId = message.InvocationId;
        this.Target = target;
        this.Arguments = message.Arguments;
        this.StreamIds = message.StreamIds;
    }

    /// <summary>
    /// Creates a new instance of a KafkaInvocationMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="arguments"></param>
    public KafkaInvocationMessage(MessageTarget target, object?[] arguments)
    {
        this.Target = target;
        this.Arguments = arguments;
    }

    /// <summary>
    /// Creates a new instance of a KafkaInvocationMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="invocationId"></param>
    /// <param name="target"></param>
    /// <param name="arguments"></param>
    public KafkaInvocationMessage(string? invocationId, MessageTarget target, object?[] arguments)
    {
        this.InvocationId = invocationId;
        this.Target = target;
        this.Arguments = arguments;
    }

    /// <summary>
    /// Creates a new instance of a KafkaInvocationMessage object, initializes with specified parameters.
    /// </summary>
    /// <param name="invocationId"></param>
    /// <param name="target"></param>
    /// <param name="arguments"></param>
    /// <param name="streamIds"></param>
    public KafkaInvocationMessage(string? invocationId, MessageTarget target, object?[] arguments, string[]? streamIds)
        : this(invocationId, target, arguments)
    {
        this.StreamIds = streamIds;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Convert serialized object to invocation message.
    /// </summary>
    /// <param name="obj"></param>
    public static implicit operator InvocationMessage(KafkaInvocationMessage obj)
    {
        var target = obj.Target.GetName() ?? throw new ConfigurationException($"Invocation message target '{obj.Target}' does not exist");
        return new InvocationMessage(obj.InvocationId, target, obj.Arguments, obj.StreamIds);
    }
    #endregion
}
