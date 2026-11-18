using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// IngestDataLocation class, provides an entity model that links (many-to-many) ingest with data locations.
/// </summary>
[Table("ingest_data_location")]
public class IngestDataLocation : AuditColumns, IEquatable<IngestDataLocation>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the ingest.
    /// </summary>
    [Column("ingest_id")]
    public int IngestId { get; set; }

    /// <summary>
    /// get/set - The ingest.
    /// </summary>
    public virtual Ingest? Ingest { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the data location.
    /// </summary>
    [Column("data_location_id")]
    public int DataLocationId { get; set; }

    /// <summary>
    /// get/set - The dataLocation.
    /// </summary>
    public virtual DataLocation? DataLocation { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestDataLocation.
    /// </summary>
    protected IngestDataLocation() { }

    /// <summary>
    /// Creates a new instance of an IngestDataLocation, initializes with specified parameters.
    /// </summary>
    /// <param name="ingestId"></param>
    /// <param name="dataLocationId"></param>
    public IngestDataLocation(int ingestId, int dataLocationId)
    {
        this.IngestId = ingestId;
        this.DataLocationId = dataLocationId;
    }

    /// <summary>
    /// Creates a new instance of an IngestDataLocation, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="dataLocation"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public IngestDataLocation(Ingest ingest, DataLocation dataLocation)
    {
        this.IngestId = ingest?.Id ?? throw new ArgumentNullException(nameof(ingest));
        this.Ingest = ingest;
        this.DataLocationId = dataLocation?.Id ?? throw new ArgumentNullException(nameof(dataLocation));
        this.DataLocation = dataLocation;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determine equality by the primary/foreign key values.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(IngestDataLocation? other)
    {
        if (other == null) return false;
        return this.IngestId == other.IngestId && this.DataLocationId == other.DataLocationId;
    }

    public override bool Equals(object? obj) => Equals(obj as IngestDataLocation);
    public override int GetHashCode() => (this.IngestId, this.DataLocationId).GetHashCode();
    #endregion
}
