using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("user_role")]
public class UserRole : AuditColumns
{
    #region Properties
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    public virtual User? User { get; set; }

    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    public virtual Role? Role { get; set; }
    #endregion

    #region Constructors
    protected UserRole() { }

    public UserRole(User user, Role role)
    {
        this.UserId = user?.Id ?? throw new ArgumentNullException(nameof(user));
        this.User = user;
        this.RoleId = role?.Id ?? throw new ArgumentNullException(nameof(role));
        this.Role = role;
    }

    public UserRole(int userId, int roleId)
    {
        this.UserId = userId;
        this.RoleId = roleId;
    }
    #endregion
}
