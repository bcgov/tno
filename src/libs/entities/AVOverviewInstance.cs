using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// AVOverviewInstance class, provides a DB model to manage different types of overviews.
/// </summary>
[Table("av_overview_instance")]
public class AVOverviewInstance : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - The type of template.
    /// </summary>
    [Column("template_type")]
    public AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - The template.
    /// </summary>
    public AVOverviewTemplate? Template { get; set; }

    /// <summary>
    /// get/set - When the content has been or will be published.
    /// </summary>
    [Column("published_on")]
    public DateTime PublishedOn { get; set; }

    /// <summary>
    /// get/set - Whether this report has been published (emailed out).
    /// </summary>
    public bool IsPublished { get; set; }

    /// <summary>
    /// get/set - The response.
    /// </summary>
    [Column("response")]
    public JsonDocument Response { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - A collection of sections in this overview instance.
    /// </summary>
    public List<AVOverviewSection> Sections { get; } = new List<AVOverviewSection>();

    /// <summary>
    /// get - Collection of user report instance, used to identify who it was sent to.
    /// </summary>
    public virtual List<UserAVOverviewInstance> UserInstances { get; } = new List<UserAVOverviewInstance>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a av overview template object.
    /// </summary>
    protected AVOverviewInstance() : base() { }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="template"></param>
    /// <param name="publishedOn"></param>
    public AVOverviewInstance(AVOverviewTemplate template, DateTime publishedOn)
    {
        this.TemplateType = template?.TemplateType ?? throw new ArgumentNullException(nameof(template));
        this.Template = template;
        this.PublishedOn = publishedOn;
    }

    /// <summary>
    /// Creates a new instance of a av overview template object, initializes with specified parameters.
    /// </summary>
    /// <param name="templateType"></param>
    /// <param name="publishedOn"></param>
    public AVOverviewInstance(AVOverviewTemplateType templateType, DateTime publishedOn)
    {
        this.TemplateType = templateType;
        this.PublishedOn = publishedOn;
    }
    #endregion
}
