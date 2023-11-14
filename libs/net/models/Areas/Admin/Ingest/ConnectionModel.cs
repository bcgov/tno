using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Ingest;

/// <summary>
/// ConnectionModel class, provides a model that represents an connection.
/// </summary>
public class ConnectionModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsReadOnly { get; set; }
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
    public ConnectionModel(Entities.Connection? entity, JsonSerializerOptions options) : base(entity)
    {
        if (entity != null)
        {
            this.IsReadOnly = entity.IsReadOnly;
            this.ConnectionType = entity.ConnectionType;
            this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Connection object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.Connection ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Connection)this;
        entity.Configuration = JsonDocument.Parse(JsonSerializer.Serialize(this.Configuration, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Connection(ConnectionModel model)
    {
        var entity = new Entities.Connection(model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            ConnectionType = model.ConnectionType,
            Configuration = JsonDocument.Parse(JsonSerializer.Serialize(model.Configuration)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
