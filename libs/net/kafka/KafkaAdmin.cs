using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Options;

namespace TNO.Kafka;

/// <summary>
/// KafkaAdmin class, provides a kafka admin client.
/// </summary>
public class KafkaAdmin : IKafkaAdmin
{
    #region Variable
    private readonly AdminClientConfig _config;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    public IAdminClient AdminClient { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates new instance of a KafkaMessenger object, initializes with specified parameters.
    /// </summary>
    /// <param name="adminConfigOptions"></param>
    public KafkaAdmin(IOptions<AdminClientConfig> adminConfigOptions)
    {
        _config = adminConfigOptions.Value;

        this.AdminClient = new AdminClientBuilder(_config).Build();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Fetch the list of topics from the configured brokers.
    /// </summary>
    /// <returns></returns>
    public string[] ListTopics()
    {
        return this.AdminClient.GetMetadata(TimeSpan.FromSeconds(30)).Topics.Select(t => t.Topic).ToArray();
    }

    /// <summary>
    /// Determine if all the specified topics exists.
    /// </summary>
    /// <param name="topics"></param>
    /// <returns></returns>
    public bool TopicExists(params string[] topics)
    {
        return this.AdminClient.GetMetadata(TimeSpan.FromSeconds(30)).Topics.Select(t => t.Topic).All(t => topics.Contains(t));
    }

    /// <summary>
    /// The number of messages in each topic the consumer group has not yet committed (its lag).
    /// A partition the group has never committed counts every message it holds. Topics that do not
    /// exist are left out.
    /// </summary>
    /// <param name="groupId"></param>
    /// <param name="topics"></param>
    /// <returns></returns>
    public async Task<IDictionary<string, long>> GetConsumerLagAsync(string groupId, params string[] topics)
    {
        var metadata = this.AdminClient.GetMetadata(TimeSpan.FromSeconds(30));
        var partitions = metadata.Topics
            .Where(t => topics.Contains(t.Topic) && !t.Error.IsError)
            .SelectMany(t => t.Partitions.Select(p => new TopicPartition(t.Topic, p.PartitionId)))
            .ToArray();
        var result = topics.Where(t => partitions.Any(p => p.Topic == t)).ToDictionary(t => t, _ => 0L);
        if (partitions.Length == 0) return result;

        var committed = (await this.AdminClient.ListConsumerGroupOffsetsAsync(new[] { new ConsumerGroupTopicPartitions(groupId, partitions.ToList()) }))
            .SelectMany(g => g.Partitions)
            .ToDictionary(p => p.TopicPartition, p => p.Offset);
        var latest = await ListOffsetsAsync(partitions, OffsetSpec.Latest());
        var earliest = await ListOffsetsAsync(partitions, OffsetSpec.Earliest());

        foreach (var partition in partitions)
        {
            var end = latest.TryGetValue(partition, out var l) ? l : 0;
            var start = committed.TryGetValue(partition, out var c) && c.Value >= 0
                ? c.Value
                : earliest.TryGetValue(partition, out var e) ? e : 0;
            result[partition.Topic] += Math.Max(0, end - start);
        }
        return result;
    }

    /// <summary>
    /// The offsets of the partitions at the specified position.
    /// </summary>
    private async Task<Dictionary<TopicPartition, long>> ListOffsetsAsync(IEnumerable<TopicPartition> partitions, OffsetSpec spec)
    {
        var offsets = await this.AdminClient.ListOffsetsAsync(partitions.Select(p => new TopicPartitionOffsetSpec() { TopicPartition = p, OffsetSpec = spec }));
        return offsets.ResultInfos.ToDictionary(r => r.TopicPartitionOffsetError.TopicPartition, r => r.TopicPartitionOffsetError.Offset.Value);
    }
    #endregion
}
