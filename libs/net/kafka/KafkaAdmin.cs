using Confluent.Kafka;
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
    #endregion
}
