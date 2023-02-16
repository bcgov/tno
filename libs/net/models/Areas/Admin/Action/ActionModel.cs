using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Action;

/// <summary>
/// ActionModel class, provides a model that represents an action.
/// </summary>
public class ActionModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The value label.
    /// </summary>
    public string ValueLabel { get; set; } = "";

    /// <summary>
    /// get/set - The value type of this action.
    /// </summary>
    public Entities.ValueType ValueType { get; set; } = Entities.ValueType.Boolean;

    /// <summary>
    /// get/set - The default value.
    /// </summary>
    public string DefaultValue { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ActionModel.
    /// </summary>
    public ActionModel() { }

    /// <summary>
    /// Creates a new instance of an ActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ActionModel(Entities.Action entity) : base(entity)
    {
        this.ValueLabel = entity.ValueLabel;
        this.ValueType = entity.ValueType;
        this.IsEnabled = entity.IsEnabled;
        this.DefaultValue = entity.DefaultValue;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Action object.
    /// </summary>
    /// <returns></returns>
    public Entities.Action ToEntity()
    {
        var entity = (Entities.Action)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Action(ActionModel model)
    {
        var entity = new Entities.Action(model.Name, model.ValueType, model.ValueLabel)
        {
            Id = model.Id,
            DefaultValue = model.DefaultValue,
            IsEnabled = model.IsEnabled,
            Description = model.Description,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
