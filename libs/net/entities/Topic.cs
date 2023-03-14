using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Topic class, provides an entity model to group related content.
/// </summary>
[Cache("topics", "lookups")]
[Table("topic")]
public class Topic : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of topic (issue, proactive).
    /// </summary>
    [Column("topic_type")]
    public TopicType TopicType { get; set; }

    /// <summary>
    /// get - List of content linked to this topic.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of many-to-many content linked to this topic.
    /// </summary>
    public virtual List<ContentTopic> ContentsManyToMany { get; } = new List<ContentTopic>();
    #endregion

    #region Constructors
    protected Topic() { }

    public Topic(string name, TopicType type = TopicType.Issues) : base(name)
    {
        this.TopicType = type;
    }
    #endregion
}
