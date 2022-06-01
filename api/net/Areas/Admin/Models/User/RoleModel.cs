namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// RoleModel class, provides a model that represents an role.
/// </summary>
public class RoleModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The key to identify this role in Keycloak.
    /// </summary>
    public Guid Key { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
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

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Role(RoleModel model)
    {
        var entity = new Entities.Role(model.Name, model.Key)
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
        };
        return entity;
    }
    #endregion
}
