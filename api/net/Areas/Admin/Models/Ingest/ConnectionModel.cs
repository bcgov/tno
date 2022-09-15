using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Ingest;

/// <summary>
/// ConnectionModel class, provides a model that represents an product.
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
}
