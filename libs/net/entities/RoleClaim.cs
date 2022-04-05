using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("role_claim")]
public class RoleClaim : AuditColumns
{
    #region Properties
    [Key]
    [Column("role_id")]
    public int RoleId { get; set; }

    public virtual Role? Role { get; set; }

    [Key]
    [Column("claim_id")]
    public int ClaimId { get; set; }

    public virtual Claim? Claim { get; set; }
    #endregion

    #region Constructors
    protected RoleClaim() { }

    public RoleClaim(Role role, Claim claim)
    {
        this.RoleId = role?.Id ?? throw new ArgumentNullException(nameof(role));
        this.Role = role;
        this.ClaimId = claim?.Id ?? throw new ArgumentNullException(nameof(claim));
        this.Claim = claim;
    }

    public RoleClaim(int roleId, int claimId)
    {
        this.RoleId = roleId;
        this.ClaimId = claimId;
    }
    #endregion
}
