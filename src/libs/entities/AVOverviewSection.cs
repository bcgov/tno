using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// AVOverviewTemplate class, provides a DB model to manage an evening overview instance section.
/// </summary>
[Table("av_overview_section")]
public class AVOverviewSection : AuditColumns
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
    /// get/set - The foreign key to the overview instance.
    /// </summary>
    [Column("av_overview_instance_id")]
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The evening instance.
    /// </summary>
    public AVOverviewInstance? Instance { get; set; }

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
    /// get/set - The series.
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
    /// get/set - A collection of items that belong in this section.
    /// </summary>
    public List<AVOverviewSectionItem> Items { get; } = new List<AVOverviewSectionItem>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a av overview template object.
    /// </summary>
    protected AVOverviewSection() : base() { }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    /// <param name="otherSource"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, AVOverviewInstance instance, string otherSource, string startTime)
    {
        this.Name = name;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.OtherSource = otherSource;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instanceId"></param>
    /// <param name="otherSource"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, long instanceId, string otherSource, string startTime)
    {
        this.Name = name;
        this.InstanceId = instanceId;
        this.OtherSource = otherSource;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    /// <param name="source"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, AVOverviewInstance instance, Source source, string startTime)
    {
        this.Name = name;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.OtherSource = source.Code;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    /// <param name="source"></param>
    /// <param name="series"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, AVOverviewInstance instance, Source source, Series series, string startTime)
    {
        this.Name = name;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.Series = series ?? throw new ArgumentNullException(nameof(series));
        this.SeriesId = series.Id;
        this.OtherSource = source.Code;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instanceId"></param>
    /// <param name="source"></param>
    /// <param name="otherSource"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, long instanceId, int sourceId, string otherSource, string startTime)
    {
        this.Name = name;
        this.InstanceId = instanceId;
        this.SourceId = sourceId;
        this.OtherSource = otherSource;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instanceId"></param>
    /// <param name="source"></param>
    /// <param name="otherSource"></param>
    /// <param name="seriesId"></param>
    /// <param name="startTime"></param>
    public AVOverviewSection(string name, long instanceId, int sourceId, string otherSource, int seriesId, string startTime)
    {
        this.Name = name;
        this.InstanceId = instanceId;
        this.SourceId = sourceId;
        this.OtherSource = otherSource;
        this.SeriesId = seriesId;
        this.StartTime = startTime;
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instance"></param>
    /// <param name="section"></param>
    public AVOverviewSection(string name, AVOverviewInstance instance, AVOverviewTemplateSection section)
    {
        this.Name = name;
        this.Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        this.InstanceId = instance.Id;
        this.SourceId = section.SourceId;
        this.Source = section.Source;
        this.OtherSource = section.OtherSource;
        this.SeriesId = section.SeriesId;
        this.Series = section.Series;
        this.StartTime = section.StartTime;
        this.Anchors = section.Anchors;
        this.Items.AddRange(section.Items.Select(i => new AVOverviewSectionItem(this.Id, i)));
    }

    /// <summary>
    /// Creates a new instance of a av overview section object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="instanceId"></param>
    /// <param name="section"></param>
    public AVOverviewSection(string name, long instanceId, AVOverviewTemplateSection section)
    {
        this.Name = name;
        this.InstanceId = instanceId;
        this.SourceId = section.SourceId;
        this.Source = section.Source;
        this.OtherSource = section.OtherSource;
        this.SeriesId = section.SeriesId;
        this.Series = section.Series;
        this.StartTime = section.StartTime;
        this.Anchors = section.Anchors;
        this.Items.AddRange(section.Items.Select(i => new AVOverviewSectionItem(this.Id, i)));
    }
    #endregion

    #region Methods
    public bool Equals(AVOverviewSection? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as AVOverviewSection);
    public override int GetHashCode() => (this.Id).GetHashCode();
    #endregion
}
