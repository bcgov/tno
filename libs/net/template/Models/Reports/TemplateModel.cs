using RazorEngineCore;
using System.Web;
using TNO.Core.Extensions;

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
        this.Content = content.ToArray();
    }

    /// <summary>
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="sections"></param>
    public TemplateModel(Dictionary<string, ReportSectionModel> sections, string? uploadPath = null)
    {
        if (sections.TryGetValue("", out ReportSectionModel? value))
        {
            this.Content = value?.Content.ToArray() ?? Array.Empty<ContentModel>();
        }
        else
        {
            this.Content = Array.Empty<ContentModel>();
        }
        this.Sections = sections;

        if (!string.IsNullOrWhiteSpace(uploadPath) && Content.Any())
        {
            foreach (var frontPage in Content.Where(x => x.ContentType == Entities.ContentType.Image))
            {
                frontPage.ImageContent = GetImageContent(uploadPath, frontPage.FileReferences.FirstOrDefault()?.Path);
            }
        }
    }

    /// <summary>
    /// Fetch the image from the specified 'path' and convert it into a base64 string.
    /// </summary>
    /// <param name="uploadPath"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private static string? GetImageContent(string uploadPath, string? path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(uploadPath, path);
        if (!safePath.FileExists()) return null;

        using FileStream fileStream = new(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        fileStream.Read(imageBytes, 0, (int)fileStream.Length);
        return Convert.ToBase64String(imageBytes);
    }
    #endregion
}
