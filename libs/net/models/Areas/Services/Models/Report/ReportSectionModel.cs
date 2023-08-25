using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Services.Models.Report;

/// <summary>
/// ReportSectionModel class, provides a model that represents an report section.
/// </summary>
public class ReportSectionModel : BaseTypeWithAuditColumnsModel<int>
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
        this.Settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(entity.Settings, options) ?? new();
        this.ChartTemplates = entity.ChartTemplatesManyToMany
            .OrderBy(c => c.SortOrder)
            .Select(c => new ChartTemplateModel(c, options)
            {
                SortOrder = c.SortOrder,
            });
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ReportSection object.
    /// </summary>
    /// <returns></returns>
    public Entities.ReportSection ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.ReportSection)this;
        entity.ChartTemplatesManyToMany.ForEach(c => c.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Filter, options)));
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ReportSection(ReportSectionModel model)
    {
        var entity = new Entities.ReportSection(model.Id, model.Name, model.ReportId, model.FilterId, model.FolderId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        entity.ChartTemplatesManyToMany.AddRange(model.ChartTemplates.OrderBy(ct => ct.SortOrder).Select(c => new Entities.ReportSectionChartTemplate(model.Id, c.Id, c.SortOrder)
        {
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(c.Settings)),
        }));

        return entity;
    }
    #endregion
}
