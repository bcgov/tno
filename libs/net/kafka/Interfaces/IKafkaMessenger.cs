using Confluent.Kafka;
using TNO.Kafka.Models;
using TNO.Kafka.SignalR;

namespace TNO.Kafka;

/// <summary>
/// IKafkaMessenger interface, provides a way to publish messages to Kafka.
/// </summary>
public interface IKafkaMessenger
{
    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="topic"></param>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public Task<DeliveryResult<TKey, TValue>?> SendMessageAsync<TKey, TValue>(string topic, TKey key, TValue value);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, SourceContent>?> SendMessageAsync(string topic, SourceContent content);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, TranscriptRequestModel>?> SendMessageAsync(string topic, TranscriptRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, ClipRequestModel>?> SendMessageAsync(string topic, ClipRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, IndexRequestModel>?> SendMessageAsync(string topic, IndexRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, NlpRequestModel>?> SendMessageAsync(string topic, NlpRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, FileRequestModel>?> SendMessageAsync(string topic, FileRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, KafkaHubMessage>?> SendMessageAsync(string topic, KafkaHubMessage request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, ReportRequestModel>?> SendMessageAsync(string topic, ReportRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, NotificationRequestModel>?> SendMessageAsync(string topic, NotificationRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, EventScheduleRequestModel>?> SendMessageAsync(string topic, EventScheduleRequestModel request);

    /// <summary>
    /// Send a message to Kafka.
    /// </summary>
    /// <param name="topic"></param>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<DeliveryResult<string, FFmpegRequestModel>?> SendMessageAsync(string topic, FFmpegRequestModel request);
}
