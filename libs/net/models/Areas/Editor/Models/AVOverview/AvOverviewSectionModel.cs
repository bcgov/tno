using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.AVOverview;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class AVOverviewSectionModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    public int Id { get; set; }

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
    /// <param name="options"></param>
    public AVOverviewSectionModel(Entities.AVOverviewSection entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.InstanceId = entity.InstanceId;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.Anchors = entity.Anchors;
        this.StartTime = entity.StartTime;
        this.SortOrder = entity.SortOrder;
        this.Items = entity.Items.OrderBy(s => s.SortOrder).Select(i => new AVOverviewSectionItemModel(i)).ToArray();
    }

    /// <summary>
    /// Creates a new instance of an AVOverviewSectionModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public AVOverviewSectionModel(Entities.AVOverviewTemplateSection entity) : base(entity)
    {
        this.Name = entity.Name;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.Anchors = entity.Anchors;
        this.StartTime = entity.StartTime;
        this.SortOrder = entity.SortOrder;
        this.Items = entity.Items.OrderBy(s => s.SortOrder).Select(i => new AVOverviewSectionItemModel(i)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewSection(AVOverviewSectionModel model)
    {
        var entity = new Entities.AVOverviewSection(model.Name, model.InstanceId, model.OtherSource, model.StartTime)
        {
            Id = model.Id,
            SourceId = model.SourceId,
            SeriesId = model.SeriesId,
            Anchors = model.Anchors,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        entity.Items.AddRange(model.Items.OrderBy(s => s.SortOrder).Select(i => (Entities.AVOverviewSectionItem)i));

        return entity;
    }
    #endregion
}
