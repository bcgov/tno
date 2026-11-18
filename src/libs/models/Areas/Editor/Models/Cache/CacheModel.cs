using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Cache;

/// <summary>
/// CacheModel class, provides a model that represents an cache.
/// </summary>
public class CacheModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to identify the cache item
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The value label.
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// get/set - The default value.
    /// </summary>
    public string Description { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an CacheModel.
    /// </summary>
    public CacheModel() { }

    /// <summary>
    /// Creates a new instance of an CacheModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public CacheModel(Entities.Cache entity) : base(entity)
    {
        this.Key = entity.Key;
        this.Value = entity.Value;
        this.Description = entity.Description;
    }
    #endregion
}
