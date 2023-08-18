using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.AVOverview;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class AVOverviewTemplateSectionModel : AuditColumnsModel
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
    /// get/set - Foreign key to the template.
    /// </summary>
    public Entities.AVOverviewTemplateType TemplateType { get; set; }

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
    public IEnumerable<AVOverviewTemplateSectionItemModel> Items { get; set; } = Array.Empty<AVOverviewTemplateSectionItemModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateSectionModel.
    /// </summary>
    public AVOverviewTemplateSectionModel() { }

    /// <summary>
    /// Creates a new instance of an AVOverviewTemplateSectionModel , initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public AVOverviewTemplateSectionModel(Entities.AVOverviewTemplateSection entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.TemplateType = entity.TemplateType;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.Anchors = entity.Anchors;
        this.StartTime = entity.StartTime;
        this.SortOrder = entity.SortOrder;
        this.Items = entity.Items.OrderBy(s => s.SortOrder).Select(i => new AVOverviewTemplateSectionItemModel(i)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewTemplateSection(AVOverviewTemplateSectionModel model)
    {
        var entity = new Entities.AVOverviewTemplateSection(model.Name, model.TemplateType, model.OtherSource)
        {
            Id = model.Id,
            SourceId = model.SourceId,
            SeriesId = model.SeriesId,
            StartTime = model.StartTime,
            Anchors = model.Anchors,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        entity.Items.AddRange(model.Items.OrderBy(s => s.SortOrder).Select(i => (Entities.AVOverviewTemplateSectionItem)i));

        return entity;
    }
    #endregion
}
