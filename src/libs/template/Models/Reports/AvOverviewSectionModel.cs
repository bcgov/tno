namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class AVOverviewSectionModel
{
    #region Properties
    /// <summary>
    /// get/set - A unique name to identify this section.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to the instance.
    /// </summary>
    public long InstanceId { get; set; }

    /// <summary>
    /// get/set - The source reference.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source code to identify the publisher.
    /// </summary>
    public string OtherSource { get; set; } = "";

    /// <summary>
    /// get/set - The series reference.
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The anchors for the template.
    /// </summary>
    public string Anchors { get; set; } = "";

    /// <summary>
    /// get/set - The start time for the template section
    /// </summary>
    public string StartTime { get; set; } = "";

    /// <summary>
    /// get/set - The sorting order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - An array of items.
    /// </summary>
    public IEnumerable<AVOverviewSectionItemModel> Items { get; set; } = Array.Empty<AVOverviewSectionItemModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewSectionModel.
    /// </summary>
    public AVOverviewSectionModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AVOverviewSectionModel(Entities.AVOverviewSection entity)
    {
        this.Name = entity.Name;
        this.SortOrder = entity.SortOrder;
        this.InstanceId = entity.InstanceId;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.Anchors = entity.Anchors;
        this.StartTime = entity.StartTime;
        this.Items = entity.Items.Select(i => new AVOverviewSectionItemModel(i)).OrderBy(s => s.SortOrder).ToArray();
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewSectionModel(TNO.API.Areas.Editor.Models.AVOverview.AVOverviewSectionModel model)
    {
        this.Name = model.Name;
        this.SortOrder = model.SortOrder;
        this.InstanceId = model.InstanceId;
        this.SourceId = model.SourceId;
        this.OtherSource = model.OtherSource;
        this.SeriesId = model.SeriesId;
        this.Anchors = model.Anchors;
        this.StartTime = model.StartTime;
        this.Items = model.Items.Select(i => new AVOverviewSectionItemModel(i)).OrderBy(s => s.SortOrder).ToArray();
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionModel , initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public AVOverviewSectionModel(TNO.API.Areas.Services.Models.AVOverview.AVOverviewSectionModel model)
    {
        this.Name = model.Name;
        this.SortOrder = model.SortOrder;
        this.InstanceId = model.InstanceId;
        this.SourceId = model.SourceId;
        this.OtherSource = model.OtherSource;
        this.SeriesId = model.SeriesId;
        this.Anchors = model.Anchors;
        this.StartTime = model.StartTime;
        this.Items = model.Items.Select(i => new AVOverviewSectionItemModel(i)).OrderBy(s => s.SortOrder).ToArray();
    }
    #endregion
}
