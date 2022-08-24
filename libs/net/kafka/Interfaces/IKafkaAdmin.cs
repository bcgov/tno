using Confluent.Kafka;

namespace TNO.Kafka;

/// <summary>
/// IKafkaAdmin interface, provides an kafka admin client.
/// </summary>
public interface IKafkaAdmin
{
    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    IAdminClient AdminClient { get; }

    /// <summary>
    /// Fetch the list of topics from the configured brokers.
    /// </summary>
    /// <returns></returns>
    public string[] ListTopics();

    /// <summary>
    /// Determine if all the specified topics exists.
    /// </summary>
    /// <param name="topics"></param>
    /// <returns></returns>
    public bool TopicExists(params string[] topics);
}
