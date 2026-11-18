using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplateSectionItem class, provides a DB model to manage overview section item templates.
/// </summary>
[Table("av_overview_template_section_item")]
public class AVOverviewTemplateSectionItem : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - The foreign key to the overview template section.
    /// </summary>
    [Column("av_overview_template_section_id")]
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The overview template section.
    /// </summary>
    public AVOverviewTemplateSection? Section { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source.
    /// </summary>
    [Column("item_type")]
    public AVOverviewItemType ItemType { get; set; }

    /// <summary>
    /// get/set - The source time of the item.
    /// </summary>
    [Column("time")]
    public string Time { get; set; } = "";

    /// <summary>
    /// get/set - Story abstract or summary text.
    /// </summary>
    [Column("summary")]
    public string Summary { get; set; } = "";

    /// <summary>
    /// get/set - A way to control the sort order of the entities.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSectionItem object.
    /// </summary>
    protected AVOverviewTemplateSectionItem() : base() { }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="summary"></param>
    public AVOverviewTemplateSectionItem(AVOverviewTemplateSection section, AVOverviewItemType type, string time, string? summary = null)
    {
        this.Section = section ?? throw new ArgumentNullException(nameof(section));
        this.SectionId = section.Id;
        this.ItemType = type;
        this.Time = time;
        this.Summary = summary ?? "";
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewTemplateSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="summary"></param>
    public AVOverviewTemplateSectionItem(int sectionId, AVOverviewItemType type, string time, string? summary = null)
    {
        this.SectionId = sectionId;
        this.ItemType = type;
        this.Time = time;
        this.Summary = summary ?? "";
    }
    #endregion
}
