namespace TNO.API.Areas.Services.Models.Organization;

/// <summary>
/// OrganizationModel class, provides a model that represents an organization.
/// </summary>
public class OrganizationModel
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key
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
    public OrganizationModel(Entities.Organization entity)
    {
        this.ParentId = entity.ParentId;
    }
    #endregion
}
