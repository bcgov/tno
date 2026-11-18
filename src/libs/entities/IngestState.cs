using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// IngestState class, provides an entity model that is a one-to-one reference to ingest.
/// This is used by the ingest services to keep track of service failures and runs.
/// </summary>
[Table("ingest_state")]
public class IngestState
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to ingest.
    /// </summary>
    [Key]
    [Column("ingest_id")]
    public int IngestId { get; set; }

    /// <summary>
    /// get/set - The ingest.
    /// </summary>
    public virtual Ingest? Ingest { get; set; }

    /// <summary>
    /// get/set - When the ingest service was ingested last.
    /// </summary>
    [Column("last_ran_on")]
    public DateTime? LastRanOn { get; set; }

    /// <summary>
    /// get/set - Creation date of last ingest content item.
    /// </summary>
    [Column("creation_date_of_last_item")]
    public DateTime? CreationDateOfLastItem { get; set; }

    /// <summary>
    /// get/set - Number of sequential failures that have occurred.
    /// </summary>
    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }
    #endregion

    #region Constructors
    protected IngestState() { }

    public IngestState(int ingestId)
    {
        this.IngestId = ingestId;
    }

    public IngestState(Ingest ingest)
    {
        this.IngestId = ingest?.Id ?? throw new ArgumentNullException(nameof(ingest));
        this.Ingest = ingest;
    }
    #endregion
}
