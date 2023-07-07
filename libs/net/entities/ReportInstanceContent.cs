using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ReportInstanceContent class, provides a DB model to associate content with a instance of a report.
/// </summary>
[Table("report_instance_content")]
public class ReportInstanceContent : AuditColumns, IEquatable<ReportInstanceContent>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the report instance.
    /// </summary>
    [Key]
    [Column("report_instance_id")]
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The report instance.
    /// </summary>
    public ReportInstance? Instance { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the content.
    /// </summary>
    [Key]
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content for this report.
    /// </summary>
    public Content? Content { get; set; }

    /// <summary>
    /// get/set - The name of the section this content belongs.
    /// </summary>
    [Column("section_name")]
    public string? SectionName { get; set; } = "";

    /// <summary>
    /// get/set - The order the content is returned.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportInstanceContent object.
    /// </summary>
    protected ReportInstanceContent() { }

    /// <summary>
    /// Creates a new instance of a ReportInstanceContent object, initializes with specified parameters.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="content"></param>
    /// <param name="section"></param>
    /// <param name="sortOrder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportInstanceContent(ReportInstance instance, Content content, string section = "", int sortOrder = 0)
    {
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.Content = content ?? throw new ArgumentNullException(nameof(content));
        this.ContentId = content.Id;
        this.SectionName = section;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a ReportInstanceContent object, initializes with specified parameters.
    /// </summary>
    /// <param name="instanceId"></param>
    /// <param name="contentId"></param>
    /// <param name="section"></param>
    /// <param name="sortOrder"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ReportInstanceContent(long instanceId, long contentId, string section = "", int sortOrder = 0)
    {
        this.InstanceId = instanceId;
        this.ContentId = contentId;
        this.SectionName = section;
        this.SortOrder = sortOrder;
    }
    #endregion

    #region Methods
    public bool Equals(ReportInstanceContent? other)
    {
        if (other == null) return false;
        return this.InstanceId == other.InstanceId
            && this.ContentId == other.ContentId
            && this.SectionName == other.SectionName;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentAction);
    public override int GetHashCode() => (this.InstanceId, this.ContentId, this.SectionName).GetHashCode();
    #endregion
}
