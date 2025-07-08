using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.User;

/// <summary>
/// OrganizationModel class, provides a model that represents an organization.
/// </summary>
public class OrganizationModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to parent organization.
    /// </summary>
    public int? ParentId { get; set; }
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

        return entity;
    }
    #endregion
}
