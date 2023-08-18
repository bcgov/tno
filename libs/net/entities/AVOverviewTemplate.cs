using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplate class, provides a DB model to manage different types of overviews.
/// </summary>
[Table("av_overview_template")]
public class AVOverviewTemplate : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - The type of template.
    /// </summary>
    [Key]
    [Column("template_type")]
    public AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report template.
    /// </summary>
    [Column("report_template_id")]
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - The report template.
    /// </summary>
    public ReportTemplate? Template { get; set; }

    /// <summary>
    /// get - A collection of sections that belong to this template.
    /// </summary>
    public List<AVOverviewTemplateSection> Sections { get; } = new List<AVOverviewTemplateSection>();

    /// <summary>
    /// get - A collection of users subscribed to this report (many-to-many).
    /// </summary>
    public List<UserAVOverview> SubscribersManyToMany { get; } = new List<UserAVOverview>();

    /// <summary>
    /// get - A collection of users subscribed to this report.
    /// </summary>
    public List<User> Subscribers { get; } = new List<User>();

    /// <summary>
    /// get/set - A collection of instances for this template.
    /// </summary>
    public List<AVOverviewInstance> Instances { get; } = new List<AVOverviewInstance>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewTemplate object.
    /// </summary>
    protected AVOverviewTemplate() : base() { }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="template"></param>
    public AVOverviewTemplate(AVOverviewTemplateType type, ReportTemplate template)
    {
        this.TemplateType = type;
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
        this.TemplateId = template.Id;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplate object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="templateId"></param>
    public AVOverviewTemplate(AVOverviewTemplateType type, int templateId)
    {
        this.TemplateType = type;
        this.TemplateId = templateId;
    }
    #endregion
}
