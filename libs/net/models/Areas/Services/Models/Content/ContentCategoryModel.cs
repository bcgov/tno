using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentCategoryModel class, provides a model that represents an category.
/// </summary>
public class ContentCategoryModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The value of the category.
    /// </summary>
    public int Score { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentCategoryModel.
    /// </summary>
    public ContentCategoryModel() { }

    /// <summary>
    /// Creates a new instance of an ContentCategoryModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentCategoryModel(Entities.ContentCategory entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.CategoryId;
        this.Name = entity.Category?.Name ?? "";
        this.Score = entity.Score;
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
    /// Creates a new instance of a ContentCategory object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentCategory ToEntity(long contentId)
    {
        var entity = (ContentCategory)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentCategory(ContentCategoryModel model)
    {
        return new Entities.ContentCategory(model.ContentId, model.Id, model.Score)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
