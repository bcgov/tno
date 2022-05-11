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
    public List<string> Keys { get; set; } = new List<string>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CacheAttribute object, initializes with specified parameter.
    /// </summary>
    /// <param name="keys">Unique key to identify this cache item.  Multiple keys allow for updating multiple cache values.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CacheAttribute(params string[] keys)
    {
        foreach (var key in keys)
        {
            this.Keys.Add(key ?? throw new ArgumentNullException(nameof(keys)));
        }
    }

    /// <summary>
    /// Creates a new instance of a CacheAttribute object, initializes with specified parameter.
    /// </summary>
    /// <param name="type">Name of type provides unique key to identify this cache item.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CacheAttribute(Type type)
    {
        this.Keys.Add(type?.Name ?? throw new ArgumentNullException(nameof(type)));
    }
    #endregion
}
