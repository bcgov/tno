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

    /// <summary>
    /// get/set - The primary key to the alert action used to validate content.
    /// </summary>
    public int? AlertId { get; set; }
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
    /// <param name="alertId"></param>
    public virtual Task InitializeAsync(int notificationId, long contentId, int alertId)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="alertId"></param>
    public virtual void Initialize(Entities.Notification notification, Entities.Content content, int alertId)
    {
        this.Notification = notification;
        this.Content = content;
        this.AlertId = alertId;
    }

    /// <summary>
    /// Validate the content to determine whether it should generate anything.
    /// The Notification.Instances or Content.NotificationsManyToMany property must contain past notifications for the content for this method to work.
    /// </summary>
    /// <returns>True if the content should send a notification.</returns>
    protected virtual bool ValidateContent()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");
        if (!this.AlertId.HasValue) throw new InvalidOperationException("AlertId must be set for validation");

        var doAlert = !this.Notification.RequireAlert || this.Content.ActionsManyToMany.Any(a => a.ActionId == this.AlertId && a.Value == "true");
        var hasBeenSent = this.Notification.Instances.Any(i => i.ContentId == this.Content.Id) ||
            this.Content.NotificationsManyToMany.Any(n => n.NotificationId == this.Notification.Id);
        return this.Content.Status == Entities.ContentStatus.Published &&
            doAlert &&
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
    /// <returns>True if the notification settings should generate send a notification.</returns>
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
