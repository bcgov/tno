using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

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

    public List<User> Users { get; } = new List<User>();

    public List<Claim> Claims { get; } = new List<Claim>();
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