namespace TNO.Entities.Validation;

/// <summary>
/// NotificationValidator class, provides a way to validate whether a notification should be generated for content.
/// </summary>
public class NotificationValidator : INotificationValidator
{
    #region Properties
    /// <summary>
    /// get/set - The notification to validate.
    /// </summary>
    protected Entities.Notification? Notification { get; private set; }

    /// <summary>
    /// get/set - The content to validate.
    /// </summary>
    protected Entities.Content? Content { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationValidator object.
    /// </summary>
    protected NotificationValidator() { }

    /// <summary>
    /// Creates a new instance of a NotificationValidator object, initializes with specified parameters.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    public NotificationValidator(Entities.Notification notification, Entities.Content content)
    {
        this.Notification = notification;
        this.Content = content;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    public virtual Task InitializeAsync(int notificationId, long contentId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    public virtual void Initialize(Entities.Notification? notification = null, Entities.Content? content = null)
    {
        this.Notification = notification;
        this.Content = content;
    }

    /// <summary>
    /// Validate the content to determine whether it should generate anything.
    /// The Notification.Instances or Content.NotificationsManyToMany property must contain past notifications for the content for this method to work.
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateContent()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        var hasBeenSent = this.Notification.Instances.Any(i => i.ContentId == this.Content.Id) ||
            this.Content.NotificationsManyToMany.Any(n => n.NotificationId == this.Notification.Id);
        return this.Content.Status == Entities.ContentStatus.Published &&
            this.Notification.Resend switch
            {
                Entities.ResendOption.Never => !hasBeenSent,
                Entities.ResendOption.Updated => hasBeenSent, // TODO: Determine if it was updated
                Entities.ResendOption.Republished => hasBeenSent, // TODO: Determine if it was republished
                Entities.ResendOption.Transcribed => this.Content.IsApproved, // TODO: Determine if it was approved
                _ => false,
            };
    }

    /// <summary>
    /// Validate the notification to determine whether it should generate anything.
    /// The Notification.Subscribers must contain subscribers for this method to work.
    /// </summary>
    /// <returns></returns>
    protected virtual bool ValidateNotification()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        return this.Notification.IsEnabled &&
            (this.Notification.Subscribers.Any() || this.Notification.SubscribersManyToMany.Any());
    }

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <returns></returns>
    public bool ConfirmSend()
    {
        return ValidateNotification() && ValidateContent();
    }
    #endregion
}
