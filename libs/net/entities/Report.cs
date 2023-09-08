using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Report class, provides a DB model to manage different types of reports.
/// </summary>
[Cache("report")]
[Table("report")]
public class Report : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// Also a unique index with the report name.
    /// Cannot have a report with the same name as another report for a single owner.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this report.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - Foreign key to the Razor template to generate the report.
    /// </summary>
    [Column("report_template_id")]
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - The report razor template.
    /// </summary>
    public ReportTemplate? Template { get; set; }

    /// <summary>
    /// get/set - Whether this report is public to all users.
    /// </summary>
    [Column("is_public")]
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The report settings to control the output.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - List of sections with this report.
    /// </summary>
    public virtual List<ReportSection> Sections { get; } = new List<ReportSection>();

    /// <summary>
    /// get - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public virtual List<UserReport> SubscribersManyToMany { get; } = new List<UserReport>();

    /// <summary>
    /// get - List of users who are subscribed to this report.
    /// </summary>
    public virtual List<User> Subscribers { get; } = new List<User>();

    /// <summary>
    /// get - Collection of report instances.
    /// </summary>
    public virtual List<ReportInstance> Instances { get; } = new List<ReportInstance>();

    /// <summary>
    /// get - Collection of event schedules for this report.
    /// </summary>
    public virtual List<EventSchedule> Schedules { get; } = new List<EventSchedule>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    protected Report() : base() { }

    /// <summary>
    /// Creates a new instance of a Report object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <param name="owner"></param>
    public Report(string name, ReportTemplate template, User? owner = null)
        : this(0, name, template?.Id ?? throw new ArgumentNullException(nameof(template)), owner?.Id)
    {
        this.Owner = owner;
        this.Template = template;
    }

    /// <summary>
    /// Creates a new instance of a Report object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="templateId"></param>
    /// <param name="ownerId"></param>
    public Report(string name, int templateId, int? ownerId = null) : base(name)
    {
        this.OwnerId = ownerId;
        this.TemplateId = templateId;
    }

    /// <summary>
    /// Creates a new instance of a Report object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="templateId"></param>
    /// <param name="ownerId"></param>
    public Report(int id, string name, int templateId, int? ownerId = null) : base(id, name)
    {
        this.OwnerId = ownerId;
        this.TemplateId = templateId;
    }
    #endregion
}
