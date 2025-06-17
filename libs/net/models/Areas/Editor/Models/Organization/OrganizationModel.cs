using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Organization;

/// <summary>
/// OrganizationModel class, provides a model that represents an organization.
/// </summary>
public class OrganizationModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// get/set - An array of users that belong to this organization.
    /// </summary>
    public IEnumerable<UserModel> Users { get; set; } = Array.Empty<UserModel>();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an OrganizationModel.
    /// </summary>
    public OrganizationModel() { }

    /// <summary>
    /// Creates a new instance of an OrganizationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public OrganizationModel(Entities.Organization entity) : base(entity)
    {
        this.ParentId = entity.ParentId;
        this.Users = entity.UsersManyToMany.Where(u => u.User != null).Select(u => new UserModel(u.User!));
        if (entity.Users.Any())
            this.Users = entity.Users.Select(u => new UserModel(u));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Organization(OrganizationModel model)
    {
        var entity = new Entities.Organization(model.Id, model.Name, model.ParentId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        entity.UsersManyToMany.AddRange(model.Users.Select(u => new Entities.UserOrganization(u.Id, model.Id)));

        return entity;
    }
    #endregion
}
