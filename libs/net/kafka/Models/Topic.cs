namespace TNO.Kafka.Models;

public class Topic
{
    #region Properties
    public string TopicType { get; set; }
    public string Name { get; set; }

    #endregion

    #region Constructors
    public Topic() { }

    public Topic(string name, string topicType)
    {
        this.Name = name ?? "";
        this.TopicType = topicType ?? "";
    }
    #endregion
}
