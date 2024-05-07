using RazorEngineCore;
using TNO.API.Models.Settings;
using TNO.Core.Extensions;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ChartEngineContentModel class, provides a model to pass to the razor engine for a chart.
/// </summary>
public class ChartEngineContentModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - The chart template primary key.
    /// This is used to identify the unique chart template.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Unique key to identify this specific chart in a report template.
    /// This is used to replace the value after a chart has been generated.
    /// </summary>
    public string Uid { get; set; } = "default";

    /// <summary>
    /// get/set - A unique name to identify the chart template.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The chart template.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - The chart settings.
    /// </summary>
    public ChartTemplateSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - The section chart settings.
    /// </summary>
    public ChartSectionSettingsModel SectionSettings { get; set; } = new();

    /// <summary>
    /// get/set - An array of content.
    /// </summary>
    public IEnumerable<ContentModel> Content { get; set; } = Array.Empty<ContentModel>();

    /// <summary>
    /// get/set - A dictionary with each section.
    /// </summary>
    public Dictionary<string, ReportSectionModel> Sections { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel.
    /// </summary>
    public ChartEngineContentModel()
    {
        this.Content = Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartEngineContentModel(string uid, API.Areas.Admin.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Id = model.Id;
        this.Name = model.Name;
        this.Template = model.Template;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartEngineContentModel(string uid, API.Areas.Editor.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Id = model.Id;
        this.Name = model.Name;
        this.Template = model.Template;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ChartEngineContentModel(string uid, API.Areas.Services.Models.Report.ChartTemplateModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Id = model.Id;
        this.Name = model.Name;
        this.Template = model.Template;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="sections"></param>
    /// <param name="content"></param>
    public ChartEngineContentModel(string uid, API.Areas.Admin.Models.Report.ChartTemplateModel model, Dictionary<string, ReportSectionModel> sections, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Id = model.Id;
        this.Name = model.Name;
        this.Template = model.Template;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Sections = sections;

        // Reference all section content in the root Content collection.
        // This can result in duplication of content if it existed in more than one section.
        this.Content = sections.SelectMany(s => s.Value.Content.Select(c =>
        {
            c.SectionName = s.Value.Name;
            c.SectionLabel = s.Value.Settings.Label;
            return c;
        })).AppendRange(content ?? Array.Empty<ContentModel>());
    }

    /// <summary>
    /// Creates a new instance of a ChartEngineContentModel, initializes with specified parameters.
    /// If content is provided it will be added to this chart, otherwise it will extract all the content in all sections above.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="model"></param>
    /// <param name="sections"></param>
    /// <param name="content"></param>
    public ChartEngineContentModel(string uid, API.Areas.Services.Models.Report.ChartTemplateModel model, Dictionary<string, ReportSectionModel> sections, IEnumerable<ContentModel>? content = null)
    {
        this.Uid = uid;
        this.Id = model.Id;
        this.Name = model.Name;
        this.Template = model.Template;
        this.Settings = model.Settings;
        this.SectionSettings = model.SectionSettings ?? new();
        this.Sections = sections;

        // Reference all section content in the root Content collection.
        // This can result in duplication of content if it existed in more than one section.
        this.Content = sections.SelectMany(s => s.Value.Content.Select(c =>
        {
            // We need the label and the section name because the name is a unique guid, but the label is the friendly name.
            c.SectionName = s.Value.Name;
            c.SectionLabel = s.Value.Settings.Label;
            return c;
        })).AppendRange(content ?? Array.Empty<ContentModel>());
    }
    #endregion

}
