using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Role;

/// <summary>
/// RoleModel class, provides a model that represents an role.
/// </summary>
public class RoleModel : BaseTypeModel<string>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an RoleModel.
    /// </summary>
    public RoleModel() { }

    /// <summary>
    /// Creates a new instance of an RoleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public RoleModel(string name)
    {
        this.Id = name;
        this.Name = name;
        this.IsEnabled = true;
    }
    #endregion
}
