namespace TNO.TemplateEngine.Models.Notifications;

/// <summary>
/// NotificationEngineContentModel class, provides a model to pass to the razor engine.
/// </summary>
public class NotificationEngineContentModel : BaseTemplateModel<ContentModel>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationEngineContentModel.
    /// </summary>
    public NotificationEngineContentModel()
        : base(new())
    {
    }

    /// <summary>
    /// Creates a new instance of a NotificationEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="uploadPath"></param>
    public NotificationEngineContentModel(ContentModel content, string? uploadPath = null)
        : base(content)
    {
        this.Content = content;

        // Convert any images to base64 and include them in the email.
        if (!string.IsNullOrWhiteSpace(uploadPath) && this.Content.ContentType == Entities.ContentType.Image)
            this.Content.ImageContent = ConvertImageToBase64String(uploadPath, this.Content.FileReferences.FirstOrDefault()?.Path);
    }

    /// <summary>
    /// Creates a new instance of a NotificationEngineContentModel, initializes with specified parameters.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="uploadPath"></param>
    public NotificationEngineContentModel(TNO.API.Areas.Services.Models.Content.ContentModel content, string? uploadPath = null)
        : this(new ContentModel(content), uploadPath)
    { }
    #endregion
}
