using RazorEngineCore;
using TNO.API.Areas.Services.Models.Content;

namespace TNO.TemplateEngine.Models.Notifications;

/// <summary>
/// TemplateModel class, provides a model to pass to the razor engine.
/// </summary>
public class TemplateModel : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - The content model.
    /// </summary>
    public ContentModel Content { get; set; }

    /// <summary>
    /// get/set - The MMIA URL.
    /// </summary>
    public Uri? MmiaUrl { get; set; }

    /// <summary>
    /// get/set - The request transcript URL.
    /// </summary>
    public Uri? RequestTranscriptUrl { get; set; }

    /// <summary>
    /// get/set - The add to report URL.
    /// </summary>
    public Uri? AddToReportUrl { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TemplateModel.
    /// </summary>
    public TemplateModel()
    {
        this.Content = new();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public TemplateModel(ContentModel content)
    {
        this.Content = content;
    }
    #endregion
}
