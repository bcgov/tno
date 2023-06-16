using TNO.Entities;

namespace TNO.Kafka.Models;

public class Topic
{
    #region Properties
    public TopicType TopicType { get; set; }
    public string Name { get; set; } = "";

    #endregion

    #region Constructors
    public Topic() { }

    public Topic(string name, TopicType topicType)
    {
        this.Name = name ?? "";
        this.TopicType = topicType;
    }
    #endregion
}
