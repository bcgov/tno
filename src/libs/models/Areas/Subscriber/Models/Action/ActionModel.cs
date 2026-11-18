using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Action;

/// <summary>
/// ActionModel class, provides a model that represents an action.
/// </summary>
public class ActionModel : BaseTypeModel<int>
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
        this.DefaultValue = entity.DefaultValue;
    }
    #endregion
}
