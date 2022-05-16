using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentTagModel class, provides a model that represents an tag.
/// </summary>
public class ContentTagModel : BaseTypeWithAuditColumnsModel<string>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTagModel.
    /// </summary>
    public ContentTagModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTagModel(Entities.ContentTag entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TagId;
        this.Name = entity.Tag?.Name ?? "";
        this.CreatedBy = entity.CreatedBy;
        this.CreatedById = entity.CreatedById;
        this.CreatedOn = entity.CreatedOn;
        this.UpdatedBy = entity.UpdatedBy;
        this.UpdatedById = entity.UpdatedById;
        this.UpdatedOn = entity.UpdatedOn;
        this.Version = entity.Version;
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
