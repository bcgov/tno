using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("claim")]
public class Claim : AuditColumns
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

    public virtual List<RoleClaim> RolesManyToMany { get; } = new List<RoleClaim>();

    public virtual List<Role> Roles { get; } = new List<Role>();
    #endregion

    #region Constructors
    protected Claim() { }

    public Claim(string name, Guid key)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required, not null, empty or whitespace", nameof(name));

        this.Name = name;
        this.Key = key;
    }
    #endregion
}
