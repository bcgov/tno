using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Setting;

/// <summary>
/// SettingModel class, provides a model that represents an setting.
/// </summary>
public class SettingModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The value.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SettingModel.
    /// </summary>
    public SettingModel() { }

    /// <summary>
    /// Creates a new instance of an SettingModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SettingModel(Entities.Setting entity) : base(entity)
    {
        this.Value = entity.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Setting object.
    /// </summary>
    /// <returns></returns>
    public Entities.Setting ToEntity()
    {
        var entity = (Entities.Setting)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Setting(SettingModel model)
    {
        var entity = new Entities.Setting(model.Name, model.Value)
        {
            Id = model.Id,
            IsEnabled = model.IsEnabled,
            Description = model.Description,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
