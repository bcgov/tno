using System.Text.Json;
using RazorEngineCore;
using TNO.API.Models.Settings;
using TNO.Core.Extensions;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportSectionModel class, provides a model to pass to the razor engine.
/// </summary>
public class ReportSectionModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - Primary key for section.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - A unique name to identify this section.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The report section type.
    /// </summary>
    public Entities.ReportSectionType SectionType { get; set; }

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The filter for this section.
    /// </summary>
    public FilterModel? Filter { get; set; }

    /// <summary>
    /// get/set - The folder for this section.
    /// </summary>
    public FolderModel? Folder { get; set; }

    /// <summary>
    /// get/set - The linked report for this section.
    /// </summary>
    public ReportModel? LinkedReport { get; set; }

    /// <summary>
    /// get/set - The aggregate(s) for this section.
    /// </summary>
    public Dictionary<string, AggregationRootModel>? Aggregations { get; set; }

    /// <summary>
    /// get/set - The settings for the section.
    /// </summary>
    public ReportSectionSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - An array of Chart Templates.
    /// </summary>
    public IEnumerable<ChartEngineContentModel> ChartTemplates { get; set; } = Array.Empty<ChartEngineContentModel>();

    /// <summary>
    /// get/set - An array of content.
    /// </summary>
    public IEnumerable<ContentModel> Content { get; set; }

    /// <summary>
    /// get/set - Raw data to display in the section.
    /// </summary>
    public string? Data { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateModel.
    /// </summary>
    public ReportSectionModel()
    {
        this.Content = Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    /// <param name="options"></param>
    public ReportSectionModel(Entities.ReportSection entity, IEnumerable<Entities.ReportInstanceContent> content, JsonSerializerOptions options)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.SectionType = entity.SectionType;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.SortOrder = entity.SortOrder;
        this.Settings = JsonSerializer.Deserialize<ReportSectionSettingsModel>(entity.Settings.ToJson(), options) ?? new();
        this.Folder = entity.Folder != null ? new FolderModel(entity.Folder) : null;
        this.Filter = entity.Filter != null ? new FilterModel(entity.Filter, options) : null;
        this.LinkedReport = entity.LinkedReport != null ? new ReportModel(entity.LinkedReport, options) : null;

        // Extract only content for this section.
        var sectionContent = content.Where(c => c.SectionName == this.Name);
        var reportSections = entity.ChartTemplatesManyToMany.Where(ct => ct.ChartTemplate != null).Select(rsct => new API.Areas.Services.Models.Report.ChartTemplateModel(rsct, options));
        var chartContent = sectionContent.Where(c => c.Content != null).Select(c => new ContentModel(c.Content!, c.SortOrder, c.SectionName, this.Settings.Label)).ToArray() ?? Array.Empty<ContentModel>();
        this.ChartTemplates = reportSections.Select(chart => new ChartEngineContentModel(GenerateChartUid(this.Id, chart.Id), chart, chartContent));
        this.Content = chartContent;
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ReportSectionModel(TNO.API.Areas.Admin.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.SectionType = model.SectionType;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Folder = model.Folder != null ? new FolderModel(model.Folder) : null;
        this.Filter = model.Filter != null ? new FilterModel(model.Filter) : null;
        this.LinkedReport = model.LinkedReport != null ? new ReportModel(model.LinkedReport) : null;
        this.ChartTemplates = model.ChartTemplates.Select(chart => new ChartEngineContentModel(GenerateChartUid(this.Id, chart.Id), chart, content));
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ReportSectionModel(TNO.API.Areas.Editor.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.SectionType = model.SectionType;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Folder = model.Folder != null ? new FolderModel(model.Folder) : null;
        this.Filter = model.Filter != null ? new FilterModel(model.Filter) : null;
        this.LinkedReport = model.LinkedReport != null ? new ReportModel(model.LinkedReport) : null;
        this.ChartTemplates = model.ChartTemplates.Select(chart => new ChartEngineContentModel(GenerateChartUid(this.Id, chart.Id), chart, content));
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ReportSectionModel(TNO.API.Areas.Services.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null, Dictionary<string, AggregationRootModel>? aggregations = null)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.SectionType = model.SectionType;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Folder = model.Folder != null ? new FolderModel(model.Folder) : null;
        this.Filter = model.Filter != null ? new FilterModel(model.Filter) : null;
        this.LinkedReport = model.LinkedReport != null ? new ReportModel(model.LinkedReport) : null;
        this.ChartTemplates = model.ChartTemplates.Select(chart => new ChartEngineContentModel(GenerateChartUid(this.Id, chart.Id), chart, content));
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
        this.Aggregations = aggregations;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate a unique id that will be used to match a string keyword value in the template.
    /// This provides a way to replace the value with the generated chart.
    /// </summary>
    /// <param name="sectionId"></param>
    /// <param name="chartId"></param>
    /// <returns></returns>
    public static string GenerateChartUid(int sectionId, int chartId)
    {
        return $"[[chart-{sectionId}-{chartId}]]";
    }
    #endregion
}
