using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// BaseType abstract class, provides common properties for list 'type' entities.
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class BaseType<TKey> : AuditColumns
  where TKey : notnull
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the entity.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether the entity is enabled.
    /// </summary>
    [Column("is_enabled")]
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - A way to control the sort order of the entities.
    /// </summary>
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
