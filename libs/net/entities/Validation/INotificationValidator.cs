namespace TNO.Entities.Validation;

/// <summary>
/// INotificationValidator interface, provides a way to validate whether a notification should be generated for content.
/// </summary>
public interface INotificationValidator
{
    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    Task InitializeAsync(int notificationId, long contentId);

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Use this method to reset the NotificationValidator with specified values.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    void Initialize(Entities.Notification? notification = null, Entities.Content? content = null);

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <returns></returns>
    bool ConfirmSend();
    #endregion
}
