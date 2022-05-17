using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentTonePoolModel class, provides a model that represents an tone pool.
/// </summary>
public class ContentTonePoolModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The user Id who owns this tone pool.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - The value of the tone pool.
    /// </summary>
    public int Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel.
    /// </summary>
    public ContentTonePoolModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTonePoolModel(Entities.ContentTonePool entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TonePoolId;
        this.Name = entity.TonePool?.Name ?? "";
        this.OwnerId = entity.TonePool?.OwnerId ?? 0;
        this.Value = entity.Value;
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
    /// Creates a new instance of a ContentTonePool object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentTonePool ToEntity(long contentId)
    {
        var entity = (Entities.ContentTonePool)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentTonePool(ContentTonePoolModel model)
    {
        return new Entities.ContentTonePool(model.ContentId, model.Id, model.Value)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
