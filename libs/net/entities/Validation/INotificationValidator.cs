using TNO.Entities.Models;

namespace TNO.Entities.Validation;

/// <summary>
/// INotificationValidator interface, provides a way to validate whether a notification should be generated for content.
/// </summary>
public interface INotificationValidator
{
    #region Properties
    /// <summary>
    /// get/set - The primary key to the alert action used to validate content.
    /// </summary>
    public int? AlertId { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    /// <param name="alertId"></param>
    Task InitializeAsync(int notificationId, long contentId, int alertId);

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Use this method to reset the NotificationValidator with specified values.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="alertId"></param>
    void Initialize(Entities.Notification notification, Entities.Content content, int alertId);

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    bool ConfirmSend(NotificationFilter filter);
    #endregion
}
