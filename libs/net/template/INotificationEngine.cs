using TNO.TemplateEngine.Models;

namespace TNO.TemplateEngine;

public interface INotificationEngine
{
    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    Task<string> GenerateNotificationSubjectAsync(
        API.Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        bool isPreview = false,
        bool enableReportSentiment = false);

    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="uploadPath"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    Task<string> GenerateNotificationBodyAsync(
        API.Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        string? uploadPath = null,
        bool isPreview = false);
}
