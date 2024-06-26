using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Setting;

/// <summary>
/// SettingModel class, provides a model that represents an setting.
/// </summary>
public class SettingModel : BaseTypeModel<int>
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
}
