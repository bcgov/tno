using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Entities;

namespace TNO.Services.Notification.Validation;

/// <summary>
/// NotificationValidator class, provides a way to validate whether a notification should be generated for content.
/// </summary>
public class NotificationValidator : INotificationValidator
{
    #region Variables
    private static readonly ContentStatus[] _onlyPublished = new[] { ContentStatus.Publish, ContentStatus.Published };
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - The API service.
    /// </summary>
    protected IApiService Api { get; }

    /// <summary>
    /// get/set - The notification to validate.
    /// </summary>
    protected API.Areas.Services.Models.Notification.NotificationModel? Notification { get; private set; }

    /// <summary>
    /// get/set - The content to validate.
    /// </summary>
    protected API.Areas.Services.Models.Content.ContentModel? Content { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationValidator object.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="logger"></param>
    public NotificationValidator(IApiService api, IOptions<JsonSerializerOptions> serializationOptions, ILogger<NotificationValidator> logger)
    {
        this.Api = api;
        _serializationOptions = serializationOptions.Value;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new instance of a NotificationValidator object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="logger"></param>
    public NotificationValidator(
        IApiService api,
        API.Areas.Services.Models.Notification.NotificationModel notification,
        API.Areas.Services.Models.Content.ContentModel content,
        IOptions<JsonSerializerOptions> serializationOptions,
        ILogger<NotificationValidator> logger)
        : this(api, serializationOptions, logger)
    {
        this.Notification = notification;
        this.Content = content;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Makes requests to the API to fetch the content and notification.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    public virtual async Task InitializeAsync(int notificationId, long contentId)
    {
        this.Notification = await this.Api.GetNotificationAsync(notificationId);
        this.Content = await this.Api.FindContentByIdAsync(contentId);

        if (this.Content != null)
            this.Content.Notifications = await this.Api.GetNotificationsForAsync(contentId);
    }

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    public virtual async Task InitializeAsync(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content)
    {
        this.Notification = notification;
        this.Content = content;
        this.Content.Notifications = await this.Api.GetNotificationsForAsync(content.Id);
    }

    /// <summary>
    /// Initialize the validator and fetch the Notification and Content.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="contentId"></param>
    public virtual void Initialize(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content)
    {
        this.Notification = notification;
        this.Content = content;
    }

    /// <summary>
    /// Validate the content to determine whether it should generate anything.
    /// The Notification.Instances or Content.NotificationsManyToMany property must contain past notifications for the content for this method to work.
    /// </summary>
    /// <returns>True if the content should send a notification.</returns>
    protected virtual bool ValidateContent()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        var send = this.Notification.AlertOnIndex && (this.Notification.Resend switch
        {
            // Never resend after the first published alert.
            Entities.ResendOption.Never => this.Content.Status == Entities.ContentStatus.Published &&
                !this.Notification.Instances.Any(i => i.ContentId == this.Content.Id &&
                    (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Accepted || i.Status == NotificationStatus.Completed)) &&
                    !this.Content.Notifications.Any(i => i.NotificationId == this.Notification.Id &&
                        (i.Status == NotificationStatus.Pending || i.Status == NotificationStatus.Accepted || i.Status == NotificationStatus.Completed)),
            // Send updates every time it has been updated.
            Entities.ResendOption.Updated => true,
            // Send every time published.
            Entities.ResendOption.Republished => this.Content.Status == Entities.ContentStatus.Published,
            // Send every time published an approved transcript
            Entities.ResendOption.Transcribed => this.Content.Status == Entities.ContentStatus.Published && this.Content.IsApproved,
            _ => false,
        });

        if (!send) _logger.LogDebug("Notification '{name}' with content '{contentId}' resend rule did not pass.", this.Notification.Name, this.Content.Id);
        return send;
    }

    /// <summary>
    /// Validate the notification to determine whether it should generate anything.
    /// The Notification.Subscribers must contain subscribers for this method to work.
    /// </summary>
    /// <returns>True if the notification settings should generate send a notification.</returns>
    protected virtual bool ValidateNotification()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        var send = this.Notification.IsEnabled && this.Notification.Subscribers.Any();

        if (!send) _logger.LogDebug("Notification '{name}' is not enabled or does not have subscribers.", this.Notification.Name);
        return send;
    }

    /// <summary>
    /// Validate the notification filter to determine whether it should generate anything.
    /// </summary>
    /// <returns>True if the notification filter has been matched.</returns>
    protected virtual bool ValidateFilter()
    {
        if (this.Notification == null || Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        var filter = this.Notification.Settings;
        if (filter == null) return true; // No filter will always send.

        var result =
            (!filter.SearchUnpublished || _onlyPublished.Contains(Content.Status)) &&
            (!filter.IsHidden.HasValue || Content.IsHidden == filter.IsHidden) &&
            (!filter.Status.HasValue || filter.Status == Content.Status) &&
            (!filter.UserId.HasValue || filter.UserId == Content.OwnerId) && // TODO: Created or Updated by this user.
            (!filter.OwnerId.HasValue || filter.OwnerId == Content.OwnerId) &&
            (!filter.HasTopic.HasValue || Content.Topics.Any()) &&

            ((!filter.StartDate.HasValue && !filter.EndDate.HasValue) ||
             (filter.StartDate.HasValue && filter.EndDate.HasValue &&
              Content.PublishedOn >= filter.StartDate.Value.ToUniversalTime() &&
              Content.PublishedOn <= filter.EndDate.Value.ToUniversalTime()) ||
             (filter.StartDate.HasValue && Content.PublishedOn >= filter.StartDate.Value.ToUniversalTime()) ||
             (filter.EndDate.HasValue && Content.PublishedOn <= filter.EndDate.Value.ToUniversalTime())) &&

            (string.IsNullOrWhiteSpace(filter.OtherSource) || Content.OtherSource.ToLower() == filter.OtherSource.ToLower()) &&
            (string.IsNullOrWhiteSpace(filter.Search) || // TODO: Search terms can contain Elasticsearch query operators.
                (filter.InHeadline == true && Content.Headline.ToLower().Contains(filter.Search.ToLower())) ||
                (filter.InByline == true && Content.Byline.ToLower().Contains(filter.Search.ToLower())) ||
                (filter.InStory == true && (Content.Summary.ToLower().Contains(filter.Search.ToLower()) || Content.Body.ToLower().Contains(filter.Search.ToLower()))
            )) &&
            (string.IsNullOrWhiteSpace(filter.Page) || Content.Page.ToLower().Contains(filter.Page.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Section) || Content.Section.ToLower().Contains(filter.Section.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Edition) || Content.Edition.ToLower().Contains(filter.Edition.ToLower())) &&

            (filter.ProductIds == null || filter.ProductIds.Any() == false || filter.ProductIds?.Contains(Content.ProductId) == true) &&
            (filter.SourceIds == null || filter.SourceIds?.Any() == false || (Content.SourceId.HasValue && filter.SourceIds?.Contains(Content.SourceId.Value) == true)) &&
            (filter.ContentIds == null || filter.ContentIds?.Any() == false || filter.ContentIds?.Contains(Content.Id) == true) &&
            (filter.ContentTypes == null || filter.ContentTypes?.Any() == false || filter.ContentTypes?.Contains(Content.ContentType) == true) &&
            (filter.Actions == null || filter.Actions?.Any() == false || Content.Actions.Any(
                ca => filter.Actions?.Any(a => a.Id == ca.Id) == true &&
                ((ca.ValueType == Entities.ValueType.Boolean && ca.Value == "true") ||
                 (ca.ValueType != Entities.ValueType.Boolean && !string.IsNullOrWhiteSpace(ca.Value)))));

        if (!result) _logger.LogDebug("Notification '{name}' with content '{id}' did not pass the filter.", this.Notification.Name, this.Content.Id);
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
