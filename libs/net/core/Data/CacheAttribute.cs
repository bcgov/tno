namespace TNO.Core.Data;

/// <summary>
/// CacheAttribute class, provides a way to identify an entity for caching.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class CacheAttribute : Attribute
{
    #region Properties
    /// <summary>
    /// get/set - Unique key to identify this cache item.
    /// </summary>
    public string Key { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CacheAttribute object, initializes with specified parameter.
    /// </summary>
    /// <param name="key">Unique key to identify this cache item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CacheAttribute(string key)
    {
        this.Key = key ?? throw new ArgumentNullException(nameof(key));
    }

    /// <summary>
    /// Creates a new instance of a CacheAttribute object, initializes with specified parameter.
    /// </summary>
    /// <param name="type">Name of type provides unique key to identify this cache item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CacheAttribute(Type type)
    {
        this.Key = type?.Name ?? throw new ArgumentNullException(nameof(type));
    }
    #endregion
}
