using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ReportSection class, provides a DB model to manage report sections.
/// </summary>
[Table("report_section")]
public class ReportSection : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to report.
    /// </summary>
    [Column("report_id")]
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The owning report.
    /// </summary>
    public virtual Report? Report { get; set; }

    /// <summary>
    /// get/set - Foreign key to the filter.
    /// </summary>
    [Column("filter_id")]
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter for this section.
    /// </summary>
    public Filter? Filter { get; set; }

    /// <summary>
    /// get/set - Foreign key to the folder.
    /// </summary>
    [Column("folder_id")]
    public int? FolderId { get; set; }

    /// <summary>
    /// get/set - The folder for this section.
    /// </summary>
    public Folder? Folder { get; set; }

    /// <summary>
    /// get/set - The report settings to control the output.
    /// </summary>
    [Column("settings")]
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - List chart templates selected for this section (many-to-many).
    /// </summary>
    public virtual List<ReportSectionChartTemplate> ChartTemplatesManyToMany { get; } = new List<ReportSectionChartTemplate>();

    /// <summary>
    /// get - List chart templates selected for this section.
    /// </summary>
    public virtual List<ChartTemplate> ChartTemplates { get; } = new List<ChartTemplate>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportSection object.
    /// </summary>
    protected ReportSection() : base() { }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="report"></param>
    public ReportSection(string name, Report report) : base(name)
    {
        this.Name = name;
        this.Report = report ?? throw new ArgumentNullException(nameof(report));
        this.ReportId = report.Id;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reportId"></param>
    public ReportSection(string name, int reportId) : base(name)
    {
        this.ReportId = reportId;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="reportId"></param>
    public ReportSection(int id, string name, int reportId) : base(id, name)
    {
        this.ReportId = reportId;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="report"></param>
    /// <param name="filter"></param>
    public ReportSection(string name, Report report, Filter filter) : this(name, report)
    {
        this.Filter = filter;
        this.FilterId = filter?.Id;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="report"></param>
    /// <param name="folder"></param>
    public ReportSection(string name, Report report, Folder folder) : this(name, report)
    {
        this.Folder = folder;
        this.FolderId = folder?.Id;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="reportId"></param>
    /// <param name="filterId"></param>
    /// <param name="folderId"></param>
    public ReportSection(string name, int reportId, int? filterId, int? folderId) : this(name, reportId)
    {
        this.FilterId = filterId;
        this.FolderId = folderId;
    }

    /// <summary>
    /// Creates a new instance of a ReportSection object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="reportId"></param>
    /// <param name="filterId"></param>
    /// <param name="folderId"></param>
    public ReportSection(int id, string name, int reportId, int? filterId, int? folderId) : this(id, name, reportId)
    {
        this.FilterId = filterId;
        this.FolderId = folderId;
    }
    #endregion

    #region Methods
    public bool Equals(ReportSection? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as ReportSection);
    public override int GetHashCode() => (this.Id).GetHashCode();
    #endregion
}
