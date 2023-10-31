namespace TNO.Services.Notification.Validation;

/// <summary>
/// INotificationValidator interface, provides a way to validate whether a notification should be generated for content.
/// </summary>
public interface INotificationValidator
{
    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    Task InitializeAsync(int notificationId, long contentId);

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    Task InitializeAsync(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content);

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    void Initialize(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content);

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <returns></returns>
    bool ConfirmSend();
    #endregion
}
