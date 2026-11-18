using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AVOverviewSectionItem class, provides a DB model to manage different types of overviews.
/// </summary>
[Table("av_overview_section_item")]
public class AVOverviewSectionItem : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - The foreign key to the section.
    /// </summary>
    [Column("av_overview_section_id")]
    public int SectionId { get; set; }

    /// <summary>
    /// get/set - The parent section.
    /// </summary>
    public AVOverviewSection? Section { get; set; }

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
    /// get - Foreign key to the owning content.
    /// </summary>
    [Column("content_id")]
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The content linked to this item.
    /// </summary>
    public Content? Content { get; set; }

    /// <summary>
    /// get/set - A way to control the sort order of the entities.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItem object.
    /// </summary>
    protected AVOverviewSectionItem() : base() { }

    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="summary"></param>
    /// <param name="content"></param>
    public AVOverviewSectionItem(AVOverviewSection section, AVOverviewItemType type, string time, string summary, Content? content = null)
    {
        this.Section = section ?? throw new ArgumentNullException(nameof(section));
        this.SectionId = section.Id;
        this.ItemType = type;
        this.Time = time;
        this.Summary = summary;
        this.Content = content;
        this.ContentId = content?.Id;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="type"></param>
    /// <param name="time"></param>
    /// <param name="summary"></param>
    /// <param name="contentId"></param>
    public AVOverviewSectionItem(int sectionId, AVOverviewItemType type, string time, string summary, long? contentId = null)
    {
        this.SectionId = sectionId;
        this.ItemType = type;
        this.Time = time;
        this.Summary = summary;
        this.ContentId = contentId;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="item"></param>
    /// <param name="content"></param>
    public AVOverviewSectionItem(AVOverviewSection section, AVOverviewTemplateSectionItem item, Content? content = null)
    {
        this.Section = section ?? throw new ArgumentNullException(nameof(section));
        this.SectionId = section.Id;
        this.ItemType = item.ItemType;
        this.Time = item.Time;
        this.Summary = item.Summary;
        this.SortOrder = item.SortOrder;
        this.Content = content;
        this.ContentId = content?.Id;
    }

    /// <summary>
    /// Creates a new instance of a AVOverviewSectionItem object, initializes with specified parameters.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="item"></param>
    /// <param name="contentId"></param>
    public AVOverviewSectionItem(int sectionId, AVOverviewTemplateSectionItem item, long? contentId = null)
    {
        this.SectionId = sectionId;
        this.ItemType = item.ItemType;
        this.Time = item.Time;
        this.Summary = item.Summary;
        this.SortOrder = item.SortOrder;
        this.ContentId = contentId;
    }
    #endregion

    #region Methods
    public bool Equals(AVOverviewSectionItem? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as AVOverviewSectionItem);
    public override int GetHashCode() => (this.Id).GetHashCode();
    #endregion
}
