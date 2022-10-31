using Confluent.Kafka;

namespace TNO.API.Areas.Kafka.Models;

/// <summary>
/// DeliveryResultModel class, provides a serializable model for Kafka delivery result message.
/// </summary>
public class DeliveryResultModel<T>
{
    #region Properties
    /// <summary>
    /// The topic associated with the message.
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// The partition associated with the message.
    /// </summary>
    public int Partition { get; set; }

    /// <summary>
    /// The partition offset associated with the message.
    /// </summary>
    public long Offset { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DeliveryResultModel object.
    /// </summary>
    public DeliveryResultModel()
    {

    }

    /// <summary>
    /// Creates a new instance of a DeliveryResultModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="result"></param>
    public DeliveryResultModel(DeliveryResult<string, T> result)
    {
        this.Topic = result.Topic;
        this.Partition = result.Partition;
        this.Offset = result.Offset;
    }
    #endregion
}
