using TNO.Entities;

namespace TNO.TemplateEngine.Models;

/// <summary>
/// ContentTopicModel class, provides a model that represents an topic.
/// </summary>
public class ContentTopicModel
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
    /// </summary>
    public int Score { get; set; }

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
    public ContentTopicModel(Entities.ContentTopic entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TopicId;
        this.Name = entity.Topic?.Name ?? "";
        this.TopicType = entity.Topic?.TopicType ?? TopicType.Issues;
        this.Score = entity.Score;
    }

    /// <summary>
    /// Creates a new instance of an ContentTopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTopicModel(TNO.API.Areas.Editor.Models.Content.ContentTopicModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Name = model.Name ?? "";
        this.TopicType = model.TopicType;
        this.Score = model.Score ?? 0;
    }

    /// <summary>
    /// Creates a new instance of an ContentTopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTopicModel(TNO.API.Areas.Services.Models.Content.ContentTopicModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Name = model.Name ?? "";
        this.TopicType = model.TopicType;
        this.Score = model.Score;
    }

    /// <summary>
    /// Creates a new instance of an ContentTopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTopicModel(TNO.API.Areas.Services.Models.Report.ContentTopicModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Name = model.Name ?? "";
        this.TopicType = model.TopicType;
        this.Score = model.Score;
    }

    /// <summary>
    /// Creates a new instance of an ContentTopicModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentTopicModel(TNO.API.Areas.Services.Models.ReportInstance.ContentTopicModel model)
    {
        this.ContentId = model.ContentId;
        this.Id = model.Id;
        this.Name = model.Name ?? "";
        this.TopicType = model.TopicType;
        this.Score = model.Score;
    }
    #endregion
}
