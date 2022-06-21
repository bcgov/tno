using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// DataLocationModel class, provides a model that represents an data location.
/// </summary>
public class DataLocationModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The location type.
    /// </summary>
    public DataLocationType LocationType { get; set; }

    /// <summary>
    /// get/set - The connection settings.
    /// </summary>
    public Dictionary<string, object> Connection { get; set; } = new Dictionary<string, object>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an DataLocationModel.
    /// </summary>
    public DataLocationModel() { }

    /// <summary>
    /// Creates a new instance of an DataLocationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public DataLocationModel(Entities.DataLocation entity, JsonSerializerOptions options) : base(entity)
    {
        this.LocationType = entity.LocationType;
        this.Connection = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Connection, options) ?? new Dictionary<string, object>();
    }
    #endregion
}
