using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

[Cache("roles", "lookups")]
[Table("role")]
public class Role : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("key")]
    public Guid Key { get; set; }

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    public virtual List<User> Users { get; } = new List<User>();

    public virtual List<UserRole> UsersManyToMany { get; } = new List<UserRole>();

    public virtual List<Claim> Claims { get; } = new List<Claim>();

    public virtual List<RoleClaim> ClaimsManyToMany { get; } = new List<RoleClaim>();
    #endregion

    #region Constructors
    protected Role() { }

    public Role(string name, Guid key)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(name));

        this.Name = name;
        this.Key = key;
    }
    #endregion
}
