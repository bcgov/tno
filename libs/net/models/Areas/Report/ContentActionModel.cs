namespace TNO.API.Areas.Report;

/// <summary>
/// ContentActionModel class, provides a model that represents an action.
/// </summary>
public class ContentActionModel
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
        this.Id = entity.ActionId;
        this.Name = entity.Action?.Name ?? "";
        this.ValueLabel = entity.Action?.ValueLabel ?? "";
        this.ValueType = entity.Action?.ValueType ?? Entities.ValueType.Boolean;
        this.Value = entity.Value;
        this.DefaultValue = entity.Action?.DefaultValue ?? "";
    }

    /// <summary>
    /// Creates a new instance of an ContentActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContentActionModel(TNO.API.Areas.Services.Models.Content.ContentActionModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name ?? "";
        this.ValueLabel = model.ValueLabel ?? "";
        this.ValueType = model.ValueType;
        this.Value = model.Value;
        this.DefaultValue = model.DefaultValue ?? "";
    }
    #endregion
}
