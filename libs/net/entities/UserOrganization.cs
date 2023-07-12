using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserOrganization class, provides a model to link users with their organizations.
/// </summary>
[Table("user_organization")]
public class UserOrganization : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the organization.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the organization.
    /// </summary>
    [Column("organization_id")]
    public int OrganizationId { get; set; }

    /// <summary>
    /// get/set - the organization linked to the user.
    /// </summary>
    public Organization? Organization { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserOrganization object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="organization"></param>
    public UserOrganization(User user, Organization organization)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        this.OrganizationId = organization.Id;
    }

    /// <summary>
    /// Creates a new instance of a UserOrganization object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="organizationId"></param>
    public UserOrganization(int userId, int organizationId)
    {
        this.UserId = userId;
        this.OrganizationId = organizationId;
    }
    #endregion

    #region Methods
    public bool Equals(UserOrganization? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.OrganizationId == other.OrganizationId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserOrganization);
    public override int GetHashCode() => (this.UserId, this.OrganizationId).GetHashCode();
    #endregion
}
