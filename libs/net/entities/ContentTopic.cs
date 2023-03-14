using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentTopic class, provides an entity model to link (many-to-many) content with topics.
/// </summary>
[Table("content_topic")]
public class ContentTopic : AuditColumns, IEquatable<ContentTopic>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to topic.
    /// </summary>
    [Column("topic_id")]
    public int TopicId { get; set; }

    /// <summary>
    /// get/set - The topic.
    /// </summary>
    public virtual Topic? Topic { get; set; }

    /// <summary>
    /// get/set - The score of the topic.
    /// </summary>
    [Column("score")]
    public int Score { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentTopic object.
    /// </summary>
    protected ContentTopic() { }

    /// <summary>
    /// Creates a new instance of a ContentTopic object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="topicId"></param>
    /// <param name="score"></param>
    public ContentTopic(long contentId, int topicId, int score)
    {
        this.ContentId = contentId;
        this.TopicId = topicId;
        this.Score = score;
    }

    /// <summary>
    /// Creates a new instance of a ContentTopic object, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="topic"></param>
    /// <param name="score"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ContentTopic(Content content, Topic topic, int score)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.TopicId = topic?.Id ?? throw new ArgumentNullException(nameof(topic));
        this.Topic = topic;
        this.Score = score;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determine if equal based on primary keys.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(ContentTopic? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.TopicId == other.TopicId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentTopic);
    public override int GetHashCode() => (this.ContentId, this.TopicId).GetHashCode();
    #endregion
}
