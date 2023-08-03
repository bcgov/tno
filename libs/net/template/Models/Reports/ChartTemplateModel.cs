using RazorEngineCore;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ChartTemplateModel class, provides a model to pass to the razor engine for a chart.
/// </summary>
public class ChartTemplateModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - Unique key to identify this specific chart in a report template.
    /// </summary>
    public string Uid { get; set; } = "default";

    /// <summary>
    /// get/set - A unique name to identify the chart template.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - An array of content.
    /// </summary>
    public IEnumerable<ContentModel> Content { get; set; }

    /// <summary>
    /// get/set - A dictionary with each section.
    /// </summary>
    public Dictionary<string, ReportSectionModel> Sections { get; set; } = new();

    /// <summary>
    /// get/set - The chart settings.
    /// </summary>
    public ChartTemplateSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The section chart settings.
    /// </summary>
    public ChartSettingsModel SectionSettings { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartTemplateModel.
    /// </summary>
    public ChartTemplateModel()
    {
        this.Content = Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="settings"></param>
    /// <param name="content"></param>
    public ChartTemplateModel(ChartSettingsModel settings, IEnumerable<ContentModel>? content = null)
    {
        this.SectionSettings = settings;
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartTemplateModel(string uid, API.Areas.Admin.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Name = model.Name;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartTemplateModel(string uid, API.Areas.Editor.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Name = model.Name;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartTemplateModel(string uid, API.Areas.Services.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Name = model.Name;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="sections"></param>
    public ChartTemplateModel(string uid, API.Areas.Editor.Models.Report.ChartTemplateModel model, Dictionary<string, ReportSectionModel> sections)
    {
        this.Uid = uid;
        this.Name = model.Name;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Sections = sections;

        // Reference all section content in the root Content collection.
        this.Content = sections.SelectMany(s => s.Value.Content).GroupBy(c => c.Id).Select(c => c.First());
    }

    /// <summary>
    /// Creates a new instance of a ChartTemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="sections"></param>
    public ChartTemplateModel(string uid, API.Areas.Services.Models.Report.ChartTemplateModel model, Dictionary<string, ReportSectionModel> sections)
    {
        this.Uid = uid;
        this.Name = model.Name;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Sections = sections;

        // Reference all section content in the root Content collection.
        this.Content = sections.SelectMany(s => s.Value.Content).GroupBy(c => c.Id).Select(c => c.First());
    }
    #endregion
}
