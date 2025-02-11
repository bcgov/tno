using System.Web;
using RazorEngineCore;
using TNO.Core.Extensions;

namespace TNO.TemplateEngine.Models;

/// <summary>
/// BaseTemplateModel class, provides a model to pass to the razor engine.
/// </summary>
public abstract class BaseTemplateModel<T> : RazorEngineTemplateBase
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to the report.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - if enable report sentiment
    /// </summary>
    public bool EnableReportSentiment { get; set; }

    /// <summary>
    /// get/set - The primary key to the report instance.
    /// </summary>
    public long? ReportInstanceId { get; set; }

    /// <summary>
    /// get/set - The main model.
    /// </summary>
    public T Content { get; set; }

    /// <summary>
    /// get/set - The Subscriber app URL.
    /// </summary>
    public Uri? SubscriberAppUrl { get; set; }

    /// <summary>
    /// get/set - The view content URL.
    /// </summary>
    public Uri? ViewContentUrl { get; set; }

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
    /// Creates a new instance of a TemplateModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    public BaseTemplateModel(T content)
    {
        this.Content = content;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Fetch the image from the specified 'path' and convert it into a base64 string.
    /// </summary>
    /// <param name="uploadPath"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string? GetImageContent(string uploadPath, string? path)
    {
        path = string.IsNullOrWhiteSpace(path) ? "" : HttpUtility.UrlDecode(path).MakeRelativePath();
        var safePath = Path.Combine(uploadPath, path);
        if (!safePath.FileExists()) return null;

        using FileStream fileStream = new(safePath, FileMode.Open, FileAccess.Read);
        var imageBytes = new byte[fileStream.Length];
        fileStream.ReadExactly(imageBytes, 0, (int)fileStream.Length);
        return Convert.ToBase64String(imageBytes);
    }
    #endregion
}
