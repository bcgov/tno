using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// Cache class, provides an entity to store cache key values in the database.
/// A cache key is used to determine whether the data represented by the key has been changed.
/// </summary>
[Table("cache")]
public class Cache : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to uniquely identify the cache item.
    /// </summary>
    [Key]
    [Column("key")]
    public string Key { get; set; }

    /// <summary>
    /// get/set - The cache value to determine if cache has changed.
    /// </summary>
    [Column("value")]
    public string Value { get; set; }

    /// <summary>
    /// get/set - A description of the cache item.
    /// </summary>
    [Column("description")]
    public string Description { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Cache entity, initializes with specified parameters.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Cache(string key, Guid value) : this(key, value.ToString())
    {
    }

    /// <summary>
    /// Creates a new instance of a Cache entity, initializes with specified parameters.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Cache(string key, string value)
    {
        this.Key = key ?? throw new ArgumentNullException(nameof(key));
        this.Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    #endregion
}
