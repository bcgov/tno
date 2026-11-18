using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentTagModel class, provides a model that represents an tag.
/// </summary>
public class ContentTagModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTagModel.
    /// </summary>
    public ContentTagModel()
    {
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="code"></param>
    public ContentTagModel(string code)
    {
        this.Code = code;
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="tagId"></param>
    /// <param name="code"></param>
    /// <param name="name"></param>
    public ContentTagModel(int tagId, string code, string name)
    {
        this.Id = tagId;
        this.Code = code;
        this.Name = name;
    }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTagModel(Entities.ContentTag entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TagId;
        this.Code = entity.Tag?.Code ?? "";
        this.Name = entity.Tag?.Name ?? "";
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentTag object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentTag ToEntity(long contentId)
    {
        var entity = (Entities.ContentTag)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentTag(ContentTagModel model)
    {
        return new Entities.ContentTag(model.ContentId, model.Id)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
