using RazorEngineCore;
using TNO.API.Areas.Services.Models.Content;

namespace TNO.TemplateEngine.Models.Reports;

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
    /// get/set - A dictionary with each section.
    /// </summary>
    public Dictionary<string, ReportSectionModel> Sections { get; set; } = new();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateModel.
    /// </summary>
    public TemplateModel()
    {
        this.Content = Array.Empty<ContentModel>();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public TemplateModel(IEnumerable<ContentModel> content)
    {
        this.Content = content;
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="sections"></param>
    public TemplateModel(Dictionary<string, ReportSectionModel> sections)
    {
        if (sections.TryGetValue("", out ReportSectionModel? value))
        {
            this.Content = value?.Content ?? Array.Empty<ContentModel>();
        }
        else
        {
            this.Content = Array.Empty<ContentModel>();
        }
        this.Sections = sections;
    }
    #endregion
}
