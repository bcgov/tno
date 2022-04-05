using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

public abstract class BaseType<TKey> : AuditColumns
  where TKey : notnull
{
    #region Properties
    [Key]
    [Column("id")]
    public TKey Id { get; set; } = default!;

    [Column("name")]
    public string Name { get; set; } = "";

    [Column("description")]
    public string Description { get; set; } = "";

    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    protected BaseType() { }

    public BaseType(string name)
    {
        if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(name));

        this.Name = name;
    }

    public BaseType(TKey id, string name) : this(name)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));

        this.Id = id;
    }
    #endregion
}
