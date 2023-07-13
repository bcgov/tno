using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.AvOverview;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class AVOverviewSectionModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The template reference.
    /// </summary>
    public int AVOverviewTemplateId { get; set; }

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
    public string? StartTime { get; set; }

    // <summary>
    // get/set - The foreign key for the AVOverviewInstance
    // </summary>
    public int AVOverviewInstanceId { get; set; }
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
    public AVOverviewSectionModel(Entities.AVOverviewSection entity, JsonSerializerOptions options) : base(entity)
    {
        this.AVOverviewTemplateId = entity.AVOverviewTemplateId;
        this.SourceId = entity.SourceId;
        this.OtherSource = entity.OtherSource;
        this.SeriesId = entity.SeriesId;
        this.Anchors = entity.Anchors;
        this.StartTime = entity.StartTime;
        this.AVOverviewInstanceId = entity.AVOverviewInstanceId;
        
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a AVOverviewSectionModel object.
    /// </summary>
    /// <returns></returns>
    public Entities.AVOverviewSection ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.AVOverviewSection)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.AVOverviewSection(AVOverviewSectionModel model)
    {
        var entity = new Entities.AVOverviewSection(model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };


        return entity;
    }
    #endregion
}
