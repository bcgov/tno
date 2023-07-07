using TNO.API.Models;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// SeriesModel class, provides a model that represents an series.
/// </summary>
public class SeriesModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int? SourceId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SeriesModel.
    /// </summary>
    public SeriesModel() { }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SeriesModel(Entities.Series entity) : base(entity)
    {
        this.SourceId = entity.SourceId;
    }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public SeriesModel(TNO.API.Areas.Editor.Models.Content.SeriesModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.SourceId = model.SourceId;
    }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public SeriesModel(TNO.API.Areas.Services.Models.Content.SeriesModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.SourceId = model.SourceId;
    }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public SeriesModel(TNO.API.Areas.Services.Models.ReportInstance.SeriesModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.SourceId = model.SourceId;
    }
    #endregion
}
