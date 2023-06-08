using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

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
    [Column("report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The report definition.
    /// </summary>
    public Report? Report { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on.
    /// </summary>
    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    [Column("response")]
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");

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

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="content"></param>
    public ReportInstance(Report report, IEnumerable<Content> content)
    {
        this.Report = report ?? throw new ArgumentNullException(nameof(report));
        this.ReportId = report.Id;
        this.ContentManyToMany.AddRange(content.Select(c => new ReportInstanceContent(0, c.Id)));
    }

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="contentIds"></param>
    public ReportInstance(int reportId, IEnumerable<long> contentIds)
    {
        this.ReportId = reportId;
        this.ContentManyToMany.AddRange(contentIds.Select(c => new ReportInstanceContent(0, c)));
    }

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// This constructor provides a way to group content into sections.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="contentIds"></param>
    public ReportInstance(int reportId, IEnumerable<KeyValuePair<string, long>> contentIds)
    {
        this.ReportId = reportId;
        this.ContentManyToMany.AddRange(contentIds.Select(c => new ReportInstanceContent(0, c.Value, c.Key)));
    }
    #endregion
}
