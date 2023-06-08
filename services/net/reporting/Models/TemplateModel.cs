using RazorEngineCore;
using TNO.API.Areas.Services.Models.Content;
using TNO.Services.Reporting.Config;

namespace TNO.Services.Reporting.Models;

/// <summary>
/// TemplateModel class, provides a model to pass to the razor engine.
/// </summary>
public class TemplateModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - An array of content.
    /// </summary>
    public IEnumerable<ContentModel> Content { get; set; }

    /// <summary>
    /// get - Notification options.
    /// </summary>
    public ReportingOptions ReportingOptions { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateModel.
    /// </summary>
    public TemplateModel()
    {
        this.Content = Array.Empty<ContentModel>();
        this.ReportingOptions = new();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="options"></param>
    public TemplateModel(IEnumerable<ContentModel> content, ReportingOptions options)
    {
        this.Content = content;
        this.ReportingOptions = options;
    }
    #endregion
}
