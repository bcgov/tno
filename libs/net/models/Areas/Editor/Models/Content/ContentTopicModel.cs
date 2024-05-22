using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.Content;

/// <summary>
/// ContentTopicModel class, provides a model that represents an topic.
/// </summary>
public class ContentTopicModel : AuditColumnsModel
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
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The value of the topic.
    /// This will never be null from the API, but can be null from the client.
    /// </summary>
    public int? Score { get; set; }

    /// <summary>
    /// get/set - The type of topic (issue, proactive).
    /// </summary>
    public TopicType TopicType { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTopicModel.
    /// </summary>
    public ContentTopicModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTopicModel(Entities.ContentTopic entity) : base(entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TopicId;
        this.Name = entity.Topic?.Name ?? "";
        this.Score = entity.Score;
        this.TopicType = entity.Topic?.TopicType ?? TopicType.Issues;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentTopic object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentTopic ToEntity(long contentId)
    {
        var entity = (ContentTopic)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentTopic(ContentTopicModel model)
    {
        return new Entities.ContentTopic(model.ContentId, model.Id, model.Score ?? 0)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
