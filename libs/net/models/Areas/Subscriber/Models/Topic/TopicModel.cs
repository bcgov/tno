using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.Topic;

/// <summary>
/// TopicModel class, provides a model that represents an topic.
/// </summary>
public class TopicModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of topic (issue, proactive).
    /// </summary>
    public TopicType TopicType { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TopicModel.
    /// </summary>
    public TopicModel() { }

    /// <summary>
    /// Creates a new instance of an TopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TopicModel(Entities.Topic entity) : base(entity)
    {
        this.TopicType = entity.TopicType;
    }
    #endregion
}
