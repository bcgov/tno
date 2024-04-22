using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Editor.Models.ReportInstance;

/// <summary>
/// ReportSectionModel class, provides a model that represents an report section.
/// </summary>
public class ReportSectionModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the report.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the filter for this section.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get/set - The filter for this section.
    /// </summary>
    public FilterModel? Filter { get; set; }

    /// <summary>
    /// get/set - Foreign key to the folder for this section.
    /// </summary>
    public int? FolderId { get; set; }

    /// <summary>
    /// get/set - The folder for this section.
    /// </summary>
    public FolderModel? Folder { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report this section pulls content from.
    /// </summary>
    public int? LinkedReportId { get; set; }

    /// <summary>
    /// get/set - The linked report for this section.
    /// </summary>
    public ReportModel? LinkedReport { get; set; }

    /// <summary>
    /// get/set - The settings for this report section.
    /// </summary>
    public ReportSectionSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - An array of report instances.
    /// </summary>
    public IEnumerable<ChartTemplateModel> ChartTemplates { get; set; } = Array.Empty<ChartTemplateModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportSectionModel.
    /// </summary>
    public ReportSectionModel() { }

    /// <summary>
    /// Creates a new instance of an ReportSectionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportSectionModel(Entities.ReportSection entity, JsonSerializerOptions options) : base(entity)
    {
        this.ReportId = entity.ReportId;
        this.FolderId = entity.FolderId;
        this.Folder = entity.Folder != null ? new FolderModel(entity.Folder) : null;
        this.FilterId = entity.FilterId;
        this.Filter = entity.Filter != null ? new FilterModel(entity.Filter) : null;
        this.LinkedReportId = entity.LinkedReportId;
        this.Settings = new ReportSectionSettingsModel(JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>(), options);
        this.ChartTemplates = entity.ChartTemplatesManyToMany
            .OrderBy(c => c.SortOrder)
            .Select(c => new ChartTemplateModel(c, options)
            {
                SortOrder = c.SortOrder,
            });
    }
    #endregion
}
