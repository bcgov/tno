using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Ingest;

/// <summary>
/// DataLocationModel class, provides a model that represents an product.
/// </summary>
public class DataLocationModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the connection.
    /// </summary>
    public int? ConnectionId { get; set; }

    /// <summary>
    /// get/set - The connection to this data location.
    /// </summary>
    public ConnectionModel? Connection { get; set; }
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
        this.ConnectionId = entity.ConnectionId;
        this.Connection = entity.Connection != null ? new ConnectionModel(entity.Connection, options) : null;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a DataLocation object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.DataLocation ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.DataLocation)this;
        entity.Connection = this.Connection?.ToEntity(options);
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.DataLocation(DataLocationModel model)
    {
        var entity = new Entities.DataLocation(model.Name, model.ConnectionId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
