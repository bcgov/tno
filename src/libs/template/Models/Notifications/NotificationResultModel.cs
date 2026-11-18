namespace TNO.TemplateEngine.Models.Notifications;

/// <summary>
/// NotificationResultModel class, provides a model that represents an notification preview result.
/// </summary>
public class NotificationResultModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The body of the notification.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - JSON data that was used to generate the notification.
    /// </summary>
    public object? Data { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationResultModel.
    /// </summary>
    public NotificationResultModel() { }

    /// <summary>
    /// Creates a new instance of an NotificationResultModel, initializes with specified parameter.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="data"></param>
    public NotificationResultModel(string subject, string body, object? data = null)
    {
        this.Subject = subject;
        this.Body = body;
        this.Data = data;
    }
    #endregion
}
