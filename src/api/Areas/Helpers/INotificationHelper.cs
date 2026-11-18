using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Notifications;

namespace TNO.API.Helpers;

/// <summary>
/// INotificationHelper interface, provides helper methods to generate notifications.
/// </summary>
public interface INotificationHelper
{
    /// <summary>
    /// Generate an instance of the notification for each content item returned by the filter.
    /// If you already have a content item, then manually create the NotificationInstance(id, contentId, requestorId)
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    Task<IEnumerable<Entities.NotificationInstance>> GenerateNotificationInstancesAsync(
        Areas.Services.Models.Notification.NotificationModel model,
        int? requestorId = null);

    /// <summary>
    /// Execute the notification template to generate the subject and body.
    /// Uses the content already in the notification instance.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="isPreview"></param>
    /// <param name="enableReportSentiment"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    Task<NotificationResultModel> GenerateNotificationAsync(
        Areas.Services.Models.NotificationInstance.NotificationInstanceModel model,
        bool isPreview = false,
        bool enableReportSentiment = false);

    /// <summary>
    /// Execute the notification template to generate the subject and body.
    /// Fetch content from elasticsearch.
    /// Use this method for preview.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="requestorId"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="NotImplementedException"></exception>
    Task<IEnumerable<NotificationResultModel>> GenerateNotificationsAsync(
        Areas.Services.Models.Notification.NotificationModel model,
        int? requestorId = null,
        bool isPreview = false);

    /// <summary>
    /// Use the Razor templates to generate the output.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="isPreview"></param>
    /// <param name="enableReportSentiment"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    Task<NotificationResultModel> GenerateNotificationAsync(
        Areas.Services.Models.Notification.NotificationModel notification,
        ContentModel content,
        bool isPreview = false,
        bool enableReportSentiment = false);
}
