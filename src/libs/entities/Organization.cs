using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// Organization class, provides a DB model to manage organizations.
/// </summary>
[Table("organization")]
public class Organization : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent organization.
    /// </summary>
    [Column("parent_id")]
    public int? ParentId { get; set; }

    /// <summary>
    /// get/set - The user who owns this organization.
    /// </summary>
    public virtual Organization? Parent { get; set; }

    /// <summary>
    /// get - List of child organizations.
    /// </summary>
    public virtual List<Organization> Children { get; } = new List<Organization>();

    /// <summary>
    /// get - List of minister's that belong to this organization.
    /// </summary>
    public virtual List<Minister> Ministers { get; } = new List<Minister>();

    /// <summary>
    /// get - List of users belonging to this organization.
    /// </summary>
    public virtual List<User> Users { get; } = new List<User>();

    /// <summary>
    /// get - List of users belonging to this organization (many-to-many).
    /// </summary>
    public virtual List<UserOrganization> UsersManyToMany { get; } = new List<UserOrganization>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Organization object.
    /// </summary>
    protected Organization() : base() { }

    /// <summary>
    /// Creates a new instance of a Organization object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="parent"></param>
    public Organization(string name, Organization? parent = null) : base(0, name)
    {
        this.Name = name;
        this.Parent = parent;
        this.ParentId = parent?.Id;
    }

    /// <summary>
    /// Creates a new instance of a Organization object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="parentId"></param>
    public Organization(int id, string name, int? parentId = null) : base(id, name)
    {
        this.ParentId = parentId;
        this.Name = name;
    }
    #endregion
}
