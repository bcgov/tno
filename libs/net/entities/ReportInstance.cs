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
    /// get/set - Foreign key to user who owns this report instance.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this report.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on (generated).
    /// </summary>
    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The date and time the report was sent on (emailed out).
    /// </summary>
    [Column("sent_on")]
    public DateTime? SentOn { get; set; }

    /// <summary>
    /// get/set - The compiled subject of the report.
    /// Used to recreate the report.
    /// </summary>
    [Column("subject")]
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The compiled body of the report.
    /// Used to recreate the report.
    /// </summary>
    [Column("body")]
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - The status of this report.
    /// </summary>
    [Column("status")]
    public ReportStatus Status { get; set; }

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

    /// <summary>
    /// get - Collection of user report instance, used to identify who it was sent to.
    /// </summary>
    public virtual List<UserReportInstance> UserInstances { get; } = new List<UserReportInstance>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstance object.
    /// </summary>
    protected ReportInstance() { }

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// This constructor provides a way to group content into sections.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="content"></param>
    public ReportInstance(int reportId, IEnumerable<ReportInstanceContent>? content = null)
    {
        this.ReportId = reportId;
        if (content != null)
            this.ContentManyToMany.AddRange(content);
    }

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// This constructor provides a way to group content into sections.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="content"></param>
    public ReportInstance(int reportId, int? ownerId, IEnumerable<ReportInstanceContent>? content = null)
        : this(reportId, content)
    {
        this.OwnerId = ownerId;
    }

    /// <summary>
    /// Creates a new instance of a ReportInstance object, initializes with specified parameters.
    /// This constructor provides a way to group content into sections.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <param name="content"></param>
    public ReportInstance(long instanceId, int reportId, int? ownerId, IEnumerable<ReportInstanceContent>? content = null)
        : this(reportId, content)
    {
        this.Id = instanceId;
        this.OwnerId = ownerId;
    }
    #endregion
}
