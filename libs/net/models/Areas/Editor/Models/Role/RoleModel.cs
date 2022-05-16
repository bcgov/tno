using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Role;

/// <summary>
/// RoleModel class, provides a model that represents an role.
/// </summary>
public class RoleModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to role.
    /// </summary>
    public int Id { get; set; } = default!;

    /// <summary>
    /// get/set - Unique key to identify the role.
    /// </summary>
    public Guid Key { get; set; } = Guid.Empty;

    /// <summary>
    /// get/set - Unique name to identify role.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Description of role.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the role is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }
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
    public RoleModel(Entities.Role entity)
    {
        this.Id = entity.Id;
        this.Key = entity.Key;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
    }
    #endregion
}
