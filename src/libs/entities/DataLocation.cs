using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// DataLocation class, provides an entity model for configuring data locations.
/// </summary>
[Table("data_location")]
[Cache("data_locations", "lookups")]
public class DataLocation : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to a connection.
    /// </summary>
    [Column("connection_id")]
    public int? ConnectionId { get; set; }

    /// <summary>
    /// get/set - The connection to the data.
    /// </summary>
    public Connection? Connection { get; set; }

    /// <summary>
    /// get - Collection of ingests associated with this data location.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - Collection of ingests, the many-to-many relationship.
    /// </summary>
    public virtual List<IngestDataLocation> IngestsManyToMany { get; } = new List<IngestDataLocation>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataLocation object.
    /// </summary>
    protected DataLocation() { }

    /// <summary>
    /// Creates a new instance of a DataLocation object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="connectionId"></param>
    public DataLocation(string name, int? connectionId = null) : base(name)
    {
        this.ConnectionId = connectionId;
    }

    /// <summary>
    /// Creates a new instance of a DataLocation object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="connection"></param>
    public DataLocation(string name, Connection? connection = null) : base(name)
    {
        this.Connection = connection;
        this.ConnectionId = connection?.Id;
    }
    #endregion
}
