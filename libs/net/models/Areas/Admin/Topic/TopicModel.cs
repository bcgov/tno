using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Topic;

/// <summary>
/// TopicModel class, provides a model that represents an topic.
/// </summary>
public class TopicModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }

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
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
        this.TopicType = entity.TopicType;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Topic(TopicModel model)
    {
        var entity = new Entities.Topic(model.Name, model.TopicType)
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
