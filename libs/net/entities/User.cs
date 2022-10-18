using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("users", "lookups")]
[Table("user")]
public class User : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; } = "";

    [Column("email")]
    public string Email { get; set; } = "";

    [Column("key")]
    public Guid Key { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = "";

    [Column("first_name")]
    public string FirstName { get; set; } = "";

    [Column("last_name")]
    public string LastName { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; }

    [Column("status")]
    public UserStatus Status { get; set; } = UserStatus.Preapproved;

    [Column("email_verified")]
    public bool EmailVerified { get; set; }

    [Column("is_system_account")]
    public bool IsSystemAccount { get; set; }

    [Column("last_login_on")]
    public DateTime? LastLoginOn { get; set; }

    [Column("note")]
    public string Note { get; set; } = "";

    [Column("code")]
    public string Code { get; set; } = "";

    [Column("code_created_on")]
    public DateTime? CodeCreatedOn { get; set; }

    public virtual List<Role> Roles { get; } = new List<Role>();

    public virtual List<UserRole> RolesManyToMany { get; } = new List<UserRole>();

    public virtual List<Content> Contents { get; } = new List<Content>();

    public virtual List<TonePool> TonePools { get; } = new List<TonePool>();

    public virtual List<TimeTracking> TimeTrackings { get; set; } = new List<TimeTracking>();
    #endregion

    #region Constructors
    protected User() { }

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
