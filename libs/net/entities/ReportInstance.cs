using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ReportInstance class, provides a DB model to capture an instance of a report.
/// </summary>
[Table("report_instance")]
public class ReportInstance : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report definition.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The report definition.
    /// </summary>
    public Report? Report { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get - Collection of content associated with this report instance.
    /// </summary>
    public virtual List<ReportInstanceContent> ContentManyToMany { get; } = new List<ReportInstanceContent>();

    /// <summary>
    /// get - Collection of content associated with this report instance.
    /// </summary>
    public virtual List<Content> Content { get; } = new List<Content>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstance object.
    /// </summary>
    protected ReportInstance() { }
    #endregion
}
