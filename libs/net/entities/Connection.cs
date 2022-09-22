using System.ComponentModel.DataAnnotations.Schema;
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
    public string Configuration { get; set; } = "{}";

    /// <summary>
    /// get/set - Whether the connection is read only.
    /// </summary>
    [Column("is_read_only")]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// get - List of ingest linked to connection
    /// </summary>
    public virtual List<Ingest> SourceIngests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of ingest linked to connection
    /// </summary>
    public virtual List<Ingest> DestinationIngests { get; } = new List<Ingest>();
    #endregion

    #region Constructors
    protected Connection() { }

    public Connection(string name) : base(name)
    {
    }
    #endregion
}
