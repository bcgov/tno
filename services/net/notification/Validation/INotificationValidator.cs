namespace TNO.Services.Notification.Validation;

/// <summary>
/// INotificationValidator interface, provides a way to validate whether a notification should be generated for content.
/// </summary>
public interface INotificationValidator
{
    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Content.
    /// Makes requests to the API to fetch the Content notifications.
    /// </summary>
    /// <param name="contentId"></param>
    Task InitializeContentAsync(long contentId);

    /// <summary>
    /// Initialize the validator and fetch the Content notifications.
    /// </summary>
    /// <param name="content"></param>
    Task InitializeContentAsync(API.Areas.Services.Models.Content.ContentModel content);

    /// <summary>
    /// Initialize the validator.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    void Initialize(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content);

    /// <summary>
    /// Initialize the validator and fetch the Notification.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    Task InitializeNotificationAsync(int notificationId);

    /// <summary>
    /// Initialize the validator for the Notification.
    /// </summary>
    /// <param name="notification"></param>
    void InitializeNotification(API.Areas.Services.Models.Notification.NotificationModel notification);

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <returns></returns>
    bool ConfirmSend();

    /// <summary>
    /// Add users who have received a notification for this content.
    /// </summary>
    /// <param name="subscribers"></param>
    void AddUsers(IEnumerable<API.Areas.Services.Models.Notification.UserNotificationModel> subscribers);

    /// <summary>
    /// Validates whether this notification should be sent to the specified user.
    /// If the user has already received a notification for the current content it will return false.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    bool ValidateSentToUser(int userId);

    /// <summary>
    /// Get all valid subscribers who have not received a notification yet.
    /// </summary>
    /// <returns></returns>
    IEnumerable<string> GetSubscriberEmails();
    #endregion
}
