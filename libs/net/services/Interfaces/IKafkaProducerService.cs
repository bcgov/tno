using Confluent.Kafka;
using TNO.Models.Kafka;

namespace TNO.Services;

/// <summary>
/// IKafkaProducerService interface, provides a way to publish messages to Kafka.
/// </summary>
public interface IKafkaProducerService
{
    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="topic"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task<DeliveryResult<TKey, TValue>?> SendMessageAsync<TKey, TValue>(string topic, TKey key, TValue value);

    /// <summary>
    /// Send a message to to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, SourceContent>?> SendMessageAsync(string topic, SourceContent content);
}
