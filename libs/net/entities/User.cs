using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// User class, provides an entity to manager user account information.
/// </summary>
[Cache("users", "lookups")]
[Table("user")]
public class User : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key, identity seed.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique username.
    /// </summary>
    [Column("username")]
    public string Username { get; set; } = "";

    /// <summary>
    /// get/set - The user's email address.
    /// </summary>
    [Column("email")]
    public string Email { get; set; } = "";

    /// <summary>
    /// get/set - A unique key to identify the user.  Primarily used for keycloak.
    /// </summary>
    [Column("key")]
    public Guid Key { get; set; }

    /// <summary>
    /// get/set - The user's display name.
    /// </summary>
    [Column("display_name")]
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// get/set - The user's first name.
    /// </summary>
    [Column("first_name")]
    public string FirstName { get; set; } = "";

    /// <summary>
    /// get/set - The user's last name.
    /// </summary>
    [Column("last_name")]
    public string LastName { get; set; } = "";

    /// <summary>
    /// get/set - Whether the user account is enabled.
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The status of the user's account.
    /// </summary>
    [Column("status")]
    public UserStatus Status { get; set; } = UserStatus.Preapproved;

    /// <summary>
    /// get/set - Whether the user's email is verified.
    /// </summary>
    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    /// <summary>
    /// get/set - Whether this account is a system account.
    /// </summary>
    [Column("is_system_account")]
    public bool IsSystemAccount { get; set; }

    /// <summary>
    /// get/set - The last time this user logged in.
    /// </summary>
    [Column("last_login_on")]
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// get/set - A note for the user.
    /// </summary>
    [Column("note")]
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - A unique code for 2FA.
    /// </summary>
    [Column("code")]
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - When the 2FA code was created.
    /// </summary>
    [Column("code_created_on")]
    public DateTime? CodeCreatedOn { get; set; }

    /// <summary>
    /// get/set - A collection of roles this user belongs to.
    /// </summary>
    public virtual List<Role> Roles { get; } = new List<Role>();

    /// <summary>
    /// get/set - A collection of roles this user belongs to (many-to-many).
    /// </summary>
    public virtual List<UserRole> RolesManyToMany { get; } = new List<UserRole>();

    /// <summary>
    /// get/set - A collection of content this user owns.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get/set - A collection of tone pools for this user.
    /// </summary>
    public virtual List<TonePool> TonePools { get; } = new List<TonePool>();

    /// <summary>
    /// get/set - A collection of time tracking assigned to this user.
    /// </summary>
    public virtual List<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();

    /// <summary>
    /// get/set - A collection of work order requests this user has submitted.
    /// </summary>
    public virtual List<WorkOrder> WorkOrderRequests { get; set; } = new List<WorkOrder>();

    /// <summary>
    /// get/set - A collection of work order requests this user has been assigned.
    /// </summary>
    public virtual List<WorkOrder> WorkOrdersAssigned { get; set; } = new List<WorkOrder>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a User object.
    /// </summary>
    protected User() { }

    /// <summary>
    /// Creates a new instance of a User object, initializes with specified parameters.
    /// </summary>
    /// <param name="username"></param>
    /// <param name="email"></param>
    /// <param name="key"></param>
    /// <exception cref="ArgumentException"></exception>
    public User(string username, string email, Guid key)
    {
        if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(username));
        if (String.IsNullOrWhiteSpace(email)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(email));

        this.Username = username;
        this.Email = email;
        this.Key = key;
        this.DisplayName = username;
    }
    #endregion
}
