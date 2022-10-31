using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.DataLocation;

/// <summary>
/// DataLocationModel class, provides a model that represents an data location.
/// </summary>
public class DataLocationModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to connection.
    /// </summary>
    public int? ConnectionId { get; set; }

    /// <summary>
    /// get/set - The connection.
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
        if (entity.Connection != null)
            this.Connection = new ConnectionModel(entity.Connection, options);
    }
    #endregion
}
