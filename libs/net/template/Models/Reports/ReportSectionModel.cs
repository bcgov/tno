using RazorEngineCore;
using TNO.API.Models.Settings;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ReportSectionModel class, provides a model to pass to the razor engine.
/// </summary>
public class ReportSectionModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - A unique name to identify this section.
    /// </summary>
    public string Name { get; set; } = "";

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
    /// get/set - The settings for the section.
    /// </summary>
    public ReportSectionSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - An array of content.
    /// </summary>
    public IEnumerable<ContentModel> Content { get; set; }
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
    public ReportSectionModel(TNO.API.Areas.Admin.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ReportSectionModel(TNO.API.Areas.Editor.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    public ReportSectionModel(TNO.API.Areas.Services.Models.Report.ReportSectionModel model, IEnumerable<ContentModel>? content = null)
    {
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.Settings = model.Settings;
        this.Content = content?.ToArray() ?? Array.Empty<ContentModel>();
    }
    #endregion
}
