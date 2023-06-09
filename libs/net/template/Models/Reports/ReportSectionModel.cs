using RazorEngineCore;
using TNO.API.Areas.Services.Models.Content;

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
    /// get/set - The label to display for this section.
    /// </summary>
    public string Label { get; set; } = "";

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
    /// <param name="name"></param>
    /// <param name="label"></param>
    /// <param name="content"></param>
    public ReportSectionModel(string name, string label, IEnumerable<ContentModel>? content = null)
    {
        this.Name = name;
        this.Label = label;
        this.Content = content ?? Array.Empty<ContentModel>();
    }
    #endregion
}
