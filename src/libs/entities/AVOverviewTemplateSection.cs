using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplateSection class, provides a DB model to manage a section template within an overview template.
/// </summary>
[Table("av_overview_template_section")]
public class AVOverviewTemplateSection : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    [Column("name")]
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A way to control the sort order of the entities.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The foreign key to the overview template.
    /// </summary>
    [Column("av_overview_template_id")]
    public AVOverviewTemplateType TemplateType { get; set; }

    /// <summary>
    /// get/set - The overview template;
    /// </summary>
    public AVOverviewTemplate? Template { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public Source? Source { get; set; }

    /// <summary>
    /// get/set - The source code to identify the publisher.
    /// </summary>
    [Column("other_source")]
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - The foreign key for series
    /// </summary>
    [Column("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The series (show).
    /// </summary>
    public Series? Series { get; set; }

    /// <summary>
    /// get/set - The anchors for the template.
    /// </summary>
    [Column("anchors")]
    public string Anchors { get; set; } = "";

    /// <summary>
    /// get/set - The start time for the template section
    /// </summary>
    [Column("start_time")]
    public string StartTime { get; set; } = "";

    /// <summary>
    /// get - A collection of items within this section.
    /// </summary>
    public List<AVOverviewTemplateSectionItem> Items { get; } = new List<AVOverviewTemplateSectionItem>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object.
    /// </summary>
    protected AVOverviewTemplateSection() { }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="otherSource"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplateType type, string otherSource)
    {
        this.Name = name;
        this.TemplateType = type;
        this.OtherSource = otherSource;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <param name="otherSource"></param>
    /// <param name="series"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplate template, string otherSource, Series series)
    {
        this.Name = name;
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
        this.TemplateType = template.TemplateType;
        this.OtherSource = otherSource;
        this.Series = series;
        this.SeriesId = series?.Id;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <param name="source"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplate template, Source source)
    {
        this.Name = name;
        this.Template = template ?? throw new ArgumentNullException(nameof(template));
        this.TemplateType = template.TemplateType;
        this.Source = source;
        this.SourceId = source.Id;
        this.OtherSource = source.Code;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="sourceId"></param>
    /// <param name="otherSource"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplateType type, int sourceId, string otherSource)
    {
        this.Name = name;
        this.TemplateType = type;
        this.SourceId = sourceId;
        this.OtherSource = otherSource;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="template"></param>
    /// <param name="source"></param>
    /// <param name="series"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplate template, Source source, Series series) : this(name, template, source)
    {
        this.Series = series;
        this.SeriesId = series.Id;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="sourceId"></param>
    /// <param name="otherSource"></param>
    /// <param name="seriesId"></param>
    public AVOverviewTemplateSection(string name, AVOverviewTemplateType type, int sourceId, string otherSource, int seriesId)
    {
        this.Name = name;
        this.TemplateType = type;
        this.SourceId = sourceId;
        this.OtherSource = otherSource;
        this.SeriesId = seriesId;
    }
    #endregion
}
