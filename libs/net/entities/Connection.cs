using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Connection class, provides an entity model for configuring connections.
/// </summary>
[Cache("connections", "lookups")]
[Table("connection")]
public class Connection : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The connection type.
    /// </summary>
    [Column("connection_type")]
    public ConnectionType ConnectionType { get; set; }

    /// <summary>
    /// get/set - Connection settings.
    /// </summary>
    [Column("configuration")]
    public JsonDocument Configuration { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - Whether the connection is read only.
    /// </summary>
    [Column("is_read_only")]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// get - List of ingest linked to connection.
    /// </summary>
    public virtual List<Ingest> SourceIngests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of ingest linked to connection.
    /// </summary>
    public virtual List<Ingest> DestinationIngests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of ingest linked to data locations.
    /// </summary>
    public virtual List<DataLocation> DataLocations { get; } = new List<DataLocation>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Connection object.
    /// </summary>
    protected Connection() { }

    /// <summary>
    /// Creates a new instance of a Connection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="connectionType"></param>
    public Connection(string name, ConnectionType connectionType = ConnectionType.LocalVolume) : base(name)
    {
        this.ConnectionType = connectionType;
    }

    /// <summary>
    /// Creates a new instance of a Connection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="connectionType"></param>
    /// <param name="configuration"></param>
    /// <param name="isReadOnly"></param>
    public Connection(string name, ConnectionType connectionType, string configuration, bool isReadOnly) : base(name)
    {
        this.ConnectionType = connectionType;
        this.Configuration = JsonDocument.Parse(configuration);
        this.IsReadOnly = isReadOnly;
    }
    #endregion
}
