using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentActionModel class, provides a model that represents an action.
/// </summary>
public class ContentActionModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The value of the action.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// get/set - The default value.
    /// </summary>
    public string DefaultValue { get; set; } = "";

    /// <summary>
    /// get/set - The value label.
    /// </summary>
    public string ValueLabel { get; set; } = "";

    /// <summary>
    /// get/set - The value type of this action.
    /// </summary>
    public Entities.ValueType ValueType { get; set; } = Entities.ValueType.Boolean;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentActionModel.
    /// </summary>
    public ContentActionModel() { }

    /// <summary>
    /// Creates a new instance of an ContentActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentActionModel(Entities.ContentAction entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.ActionId;
        this.Name = entity.Action?.Name ?? "";
        this.ValueLabel = entity.Action?.ValueLabel ?? "";
        this.ValueType = entity.Action?.ValueType ?? Entities.ValueType.Boolean;
        this.Value = entity.Value;
        this.DefaultValue = entity.Action?.DefaultValue ?? "";
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
    /// Creates a new instance of a ContentAction object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentAction ToEntity(long contentId)
    {
        var entity = (Entities.ContentAction)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentAction(ContentActionModel model)
    {
        return new Entities.ContentAction(model.ContentId, model.Id, model.Value ?? "")
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
