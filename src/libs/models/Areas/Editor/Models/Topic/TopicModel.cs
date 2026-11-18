using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.Topic;

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

    /// <summary>
    /// get/set - A topic the system relies on (the "Not Applicable" topic).
    /// </summary>
    public bool IsSystem { get; set; }
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
        this.IsSystem = entity.IsSystem;
    }
    #endregion
}
