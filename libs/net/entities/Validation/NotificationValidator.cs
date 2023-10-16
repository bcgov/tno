using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.Entities.Models;

namespace TNO.Entities.Validation;

/// <summary>
/// NotificationValidator class, provides a way to validate whether a notification should be generated for content.
/// </summary>
public class NotificationValidator : INotificationValidator
{
    #region Variables
    private static readonly ContentStatus[] _onlyPublished = new[] { ContentStatus.Publish, ContentStatus.Published };
    private readonly JsonSerializerOptions _serializationOptions;
    #endregion

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
    protected NotificationValidator(IOptions<JsonSerializerOptions> serializationOptions)
    {
        _serializationOptions = serializationOptions.Value;
    }

    /// <summary>
    /// Creates a new instance of a NotificationValidator object, initializes with specified parameters.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    public NotificationValidator(
        Notification notification,
        Content content,
        IOptions<JsonSerializerOptions> serializationOptions) : this(serializationOptions)
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

        var alertRequested = this.Content.ActionsManyToMany.Any(a => a.ActionId == this.AlertId && a.Value == "true"); // The editor requested an alert.
        var resend = this.Notification.Resend switch
        {
            // Never resend after the first published alert.
            Entities.ResendOption.Never => this.Content.Status == Entities.ContentStatus.Published &&
                !this.Notification.Instances.Any(i => i.ContentId == this.Content.Id &&
                    (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Accepted || i.Status == NotificationStatus.Completed)) &&
                    !this.Content.NotificationsManyToMany.Any(i => i.NotificationId == this.Notification.Id &&
                        (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Accepted || i.Status == NotificationStatus.Completed)),
            // Send updates every time it has been updated.
            Entities.ResendOption.Updated => true,
            // Send every time published.
            Entities.ResendOption.Republished => this.Content.Status == Entities.ContentStatus.Published,
            // Send every time published an approved transcript
            Entities.ResendOption.Transcribed => this.Content.Status == Entities.ContentStatus.Published && this.Content.IsApproved,
            _ => false,
        };
        return resend && (!this.Notification.RequireAlert || alertRequested);

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
    /// Validate the notification filter to determine whether it should generate anything.
    /// </summary>
    /// <returns>True if the notification filter has been matched.</returns>
    protected virtual bool ValidateFilter()
    {
        if (Notification == null || Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        var filter = JsonSerializer.Deserialize<NotificationFilter>(Notification.Filter, _serializationOptions);
        if (filter == null) return true; // No filter will always send.

        var result =
            (string.IsNullOrWhiteSpace(filter.OtherSource) || Content.OtherSource.ToLower() == filter.OtherSource.ToLower()) &&
            (string.IsNullOrWhiteSpace(filter.Headline) || Content.Headline.ToLower().Contains(filter.Headline.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Page) || Content.Page.ToLower().Contains(filter.Page.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Section) || Content.Section.ToLower().Contains(filter.Section.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Edition) || Content.Edition.ToLower().Contains(filter.Edition.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Byline) || Content.Byline.ToLower().Contains(filter.Byline.ToLower())) &&
            // The filter should work with the primary key ProductId but not Product for the moment
            //(string.IsNullOrWhiteSpace(filter.Product) || (Content.Product != null && Content.Product.Name.ToLower().Contains(filter.Product.ToLower()))) &&
            (!filter.ProductIds.Any() || filter.ProductIds.Contains(Content.ProductId)) &&
            (!filter.SourceIds.Any() || (Content.SourceId.HasValue && filter.SourceIds.Contains(Content.SourceId.Value))) &&
            (!filter.ContentIds.Any() || filter.ContentIds.Contains(Content.Id)) &&
            (!filter.ContentTypes.Any() || filter.ContentTypes.Contains(Content.ContentType)) &&
            (!filter.Status.HasValue || filter.Status == Content.Status) &&
            (!filter.OwnerId.HasValue || filter.OwnerId == Content.OwnerId) &&
            (!filter.UserId.HasValue || filter.UserId == Content.OwnerId) &&
            (!filter.CreatedOn.HasValue || Content.CreatedOn == filter.CreatedOn.Value.ToUniversalTime()) &&
            (!filter.UpdatedOn.HasValue || Content.UpdatedOn == filter.UpdatedOn.Value.ToUniversalTime()) &&
            (!filter.PublishedOn.HasValue || Content.PublishedOn == filter.PublishedOn.Value.ToUniversalTime()) &&
            (!filter.HasTopic.HasValue || Content.TopicsManyToMany.Any()) &&
            ((filter.IncludeHidden.HasValue && filter.IncludeHidden.Value) || !Content.IsHidden) &&
            (!filter.OnlyHidden.HasValue || !filter.OnlyHidden.Value || Content.IsHidden) &&
            (!filter.OnlyPublished.HasValue || !filter.OnlyPublished.Value || _onlyPublished.Contains(Content.Status)) &&
            (!filter.Actions.Any() || Content.ActionsManyToMany.Any(ca => ca.Action != null &&
                filter.Actions.Contains(ca.Action.Name) &&
                ((ca.Action.ValueType == ValueType.Boolean && ca.Value == "true") ||
                 (ca.Action.ValueType != ValueType.Boolean && !string.IsNullOrWhiteSpace(ca.Value))))) &&
            ((!filter.CreatedStartOn.HasValue && !filter.CreatedEndOn.HasValue) ||
             (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue &&
              Content.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime() &&
              Content.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime()) ||
             (filter.CreatedStartOn.HasValue && Content.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime()) ||
             (filter.CreatedEndOn.HasValue && Content.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime())) &&
            ((!filter.UpdatedStartOn.HasValue && !filter.UpdatedEndOn.HasValue) ||
             (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue &&
              Content.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime() &&
              Content.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime()) ||
             (filter.UpdatedStartOn.HasValue && Content.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime()) ||
             (filter.UpdatedEndOn.HasValue && Content.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime())) &&
            ((!filter.PublishedStartOn.HasValue && !filter.PublishedEndOn.HasValue) ||
             (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue &&
              Content.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime() &&
              Content.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime()) ||
             (filter.PublishedStartOn.HasValue && Content.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime()) ||
             (filter.PublishedEndOn.HasValue && Content.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime()));

        return result;
    }

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <returns></returns>
    public bool ConfirmSend()
    {
        return ValidateNotification() && ValidateContent() && ValidateFilter();
    }
    #endregion
}
