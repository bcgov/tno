using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.Ingest;

/// <summary>
/// ConnectionModel class, provides a model that represents an data location.
/// </summary>
public class ConnectionModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The connection type.
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// get/set - Whether the connection is read only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// get/set - The connection settings.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ConnectionModel.
    /// </summary>
    public ConnectionModel() { }

    /// <summary>
    /// Creates a new instance of an ConnectionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ConnectionModel(Entities.Connection entity, JsonSerializerOptions options) : base(entity)
    {
        this.ConnectionType = entity.ConnectionType;
        this.IsReadOnly = entity.IsReadOnly;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
    }
    #endregion
}
