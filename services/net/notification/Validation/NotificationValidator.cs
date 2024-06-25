using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches.Models;
using TNO.Core.Extensions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Services.Notification.Config;

namespace TNO.Services.Notification.Validation;

/// <summary>
/// NotificationValidator class, provides a way to validate whether a notification should be generated for content.
/// </summary>
public class NotificationValidator : INotificationValidator
{
    #region Variables
    private static readonly ContentStatus[] _onlyPublished = new[] { ContentStatus.Publish, ContentStatus.Published };
    private readonly ElasticOptions _elasticOptions;
    private readonly ILogger _logger;
    #endregion

    #region Properties
    /// <summary>
    /// get - The API service.
    /// </summary>
    protected IApiService Api { get; }

    /// <summary>
    /// get - The Elasticsearch client.
    /// </summary>
    protected ITNOElasticClient Client { get; private set; }

    /// <summary>
    /// get/set - The notification to validate.
    /// </summary>
    protected API.Areas.Services.Models.Notification.NotificationModel? Notification { get; private set; }

    /// <summary>
    /// get/set - Notification service configuration options.
    /// </summary>
    protected NotificationOptions Options { get; }

    /// <summary>
    /// get/set - The content to validate.
    /// </summary>
    protected API.Areas.Services.Models.Content.ContentModel? Content { get; private set; }

    /// <summary>
    /// get/set - Dictionary of users that a notification was sent to for the current content.
    /// </summary>
    public HashSet<int> SentToUsers { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationValidator object.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="elasticClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="notificationOptions"></param>
    /// <param name="logger"></param>
    public NotificationValidator(IApiService api,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<NotificationOptions> notificationOptions,
        ILogger<NotificationValidator> logger)
    {
        this.Api = api;
        this.Client = elasticClient;
        _elasticOptions = elasticOptions.Value;
        this.Options = notificationOptions.Value;
        this.SentToUsers = new HashSet<int>();
        _logger = logger;
    }

    /// <summary>
    /// Creates a new instance of a NotificationValidator object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="elasticClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="notificationOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="logger"></param>
    public NotificationValidator(
        IApiService api,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        API.Areas.Services.Models.Notification.NotificationModel notification,
        API.Areas.Services.Models.Content.ContentModel content,
        IOptions<NotificationOptions> notificationOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        ILogger<NotificationValidator> logger)
        : this(api, elasticClient, elasticOptions, notificationOptions, logger)
    {
        this.Notification = notification;
        this.Content = content;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Initialize the validator and fetch the Content.
    /// Makes requests to the API to fetch the Content notifications.
    /// Resets who has received notifications for this content.
    /// </summary>
    /// <param name="contentId"></param>
    public virtual async Task InitializeContentAsync(long contentId)
    {
        this.Content = await this.Api.FindContentByIdAsync(contentId, true);
        if (this.Content != null)
            this.Content.Notifications = await this.Api.GetNotificationsForAsync(contentId);
        this.SentToUsers = new HashSet<int>();
    }

    /// <summary>
    /// Initialize the validator and fetch the Content notifications.
    /// </summary>
    /// <param name="content"></param>
    public virtual async Task InitializeContentAsync(API.Areas.Services.Models.Content.ContentModel content)
    {
        this.Content = content;
        this.Content.Notifications = await this.Api.GetNotificationsForAsync(content.Id);
        this.SentToUsers = new HashSet<int>();
    }

    /// <summary>
    /// Initialize the validator and fetch the Notification.
    /// Makes request to the API to fetch the notification instances.
    /// </summary>
    /// <param name="notificationId"></param>
    public virtual async Task InitializeNotificationAsync(int notificationId)
    {
        this.Notification = await this.Api.GetNotificationAsync(notificationId);
    }

    /// <summary>
    /// Initialize the validator for the Notification.
    /// </summary>
    /// <param name="notification"></param>
    public virtual void InitializeNotification(API.Areas.Services.Models.Notification.NotificationModel notification)
    {
        this.Notification = notification;
    }

    /// <summary>
    /// Initialize the validator.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    public virtual void Initialize(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content)
    {
        this.Notification = notification;
        this.Content = content;
        this.SentToUsers = new HashSet<int>();
    }

    /// <summary>
    /// Validate the content to determine whether it should generate anything.
    /// The Notification.Instances or Content.NotificationsManyToMany property must contain past notifications for the content for this method to work.
    /// </summary>
    /// <returns>True if the content should send a notification.</returns>
    protected virtual bool ValidateContent()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

        // If the content was published before the specified offset, ignore it.
        if (this.Options.IgnoreContentPublishedBeforeOffset.HasValue
            && this.Options.IgnoreContentPublishedBeforeOffset > 0
            && this.Content.PublishedOn.HasValue
            && this.Content.PublishedOn.Value < DateTime.UtcNow.AddDays(-1 * this.Options.IgnoreContentPublishedBeforeOffset.Value))
            return false;

        var send = !this.Notification.AlertOnIndex || (this.Notification.Resend switch
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

        var send = this.Notification.IsEnabled;

        if (!send) _logger.LogDebug("Notification '{name}' is not enabled or does not have subscribers.", this.Notification.Name);
        return send;
    }

    /// <summary>
    /// Validate the notification filter to determine whether it should generate anything.
    /// </summary>
    /// <returns>True if the notification filter has been matched.</returns>
    protected virtual async Task<bool> ValidateFilterAsync()
    {
        if (this.Notification == null || this.Content == null) throw new InvalidOperationException("Notification and Content properties cannot be null");

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
              Content.PublishedOn?.ToTimeZone(this.Options.TimeZone) >= filter.StartDate.Value.ToTimeZone(this.Options.TimeZone) &&
              Content.PublishedOn?.ToTimeZone(this.Options.TimeZone) <= filter.EndDate.Value.ToTimeZone(this.Options.TimeZone)) ||
             (filter.StartDate.HasValue && Content.PublishedOn?.ToTimeZone(this.Options.TimeZone) >= filter.StartDate.Value.ToTimeZone(this.Options.TimeZone)) ||
             (filter.EndDate.HasValue && Content.PublishedOn?.ToTimeZone(this.Options.TimeZone) <= filter.EndDate.Value.ToTimeZone(this.Options.TimeZone))) &&

            (string.IsNullOrWhiteSpace(filter.OtherSource) || Content.OtherSource.ToLower() == filter.OtherSource.ToLower()) &&
            (string.IsNullOrWhiteSpace(filter.Page) || Content.Page.ToLower().Contains(filter.Page.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Section) || Content.Section.ToLower().Contains(filter.Section.ToLower())) &&
            (string.IsNullOrWhiteSpace(filter.Edition) || Content.Edition.ToLower().Contains(filter.Edition.ToLower())) &&

            (filter.MediaTypeIds == null || filter.MediaTypeIds.Any() == false || filter.MediaTypeIds?.Contains(Content.MediaTypeId) == true) &&
            (filter.SourceIds == null || filter.SourceIds?.Any() == false || (Content.SourceId.HasValue && filter.SourceIds?.Contains(Content.SourceId.Value) == true)) &&
            (filter.ContentIds == null || filter.ContentIds?.Any() == false || filter.ContentIds?.Contains(Content.Id) == true) &&
            (filter.ContentTypes == null || filter.ContentTypes?.Any() == false || filter.ContentTypes?.Contains(Content.ContentType) == true) &&
            (filter.Actions == null || filter.Actions?.Any() == false || Content.Actions.Any(
                ca => filter.Actions?.Any(a => a.Id == ca.Id) == true &&
                ((ca.ValueType == Entities.ValueType.Boolean && ca.Value == "true") ||
                 (ca.ValueType != Entities.ValueType.Boolean && !string.IsNullOrWhiteSpace(ca.Value)))));

        if (!String.IsNullOrWhiteSpace(filter.Search) && (filter.InHeadline == true || filter.InByline == true || filter.InStory == true))
        {
            // Make a request to Elasticsearch to compare the text search.
            var index = filter.SearchUnpublished ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex;
            var query = ModifyElasticQuery(this.Notification.Query, this.Content.Id);
            var response = await this.Client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, query);

            // If the content item wasn't returned it wasn't a match.
            if (response.Hits.Total.Value != 1)
                result = false;
        }

        if (!result) _logger.LogDebug("Notification '{id}:{name}' with content '{id}' did not pass the filter.", this.Notification.Id, this.Notification.Name, this.Content.Id);
        return result;
    }

    /// <summary>
    /// Determine if the specified 'notification' should be sent for the specified 'content'.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> ConfirmSendAsync()
    {
        return ValidateNotification() && ValidateContent() && await ValidateFilterAsync();
    }

    /// <summary>
    /// Add users who have received a notification for this content.
    /// </summary>
    /// <param name="subscribers"></param>
    public void AddUsers(IEnumerable<API.Areas.Services.Models.Notification.UserNotificationModel> subscribers)
    {
        var users = subscribers.Where(s => s.IsSubscribed);
        users.ForEach(u =>
        {
            if (!this.SentToUsers.Contains(u.UserId)) this.SentToUsers.Add(u.UserId);
        });
    }

    /// <summary>
    /// Validates whether this notification should be sent to the specified user.
    /// If the user has already received a notification for the current content it will return false.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public bool ValidateSentToUser(int userId)
    {
        if (this.Content == null) return true;
        return !this.SentToUsers.Contains(userId);
    }

    /// <summary>
    /// Get all valid subscribers who have not received a notification yet.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<EmailContextModel> GetSubscriberEmails()
    {
        var now = DateTime.Now;
        var emails = new List<EmailContextModel>();
        if (this.Notification == null) return Array.Empty<EmailContextModel>();
        if (this.Content != null)
        {
            emails.AddRange(this.Content.UserNotifications.Where(un => un.User!.AccountType != UserAccountType.Distribution).Select(un =>
            {
                var email = un.User!.GetEmail();
                var context = new Dictionary<string, object>() {
                    { "id", un.UserId },
                    { "firstName", un.User?.FirstName ?? "" },
                    { "lastName", un.User?.LastName ?? "" },
                };
                return new EmailContextModel(new[] { email }, context, now)
                {
                    Tag = $"{this.Notification.Name}-{this.Content.Id}",
                };
            }));
        }
        emails.AddRange(this.Notification.Subscribers
            .Where(s => s.User!.AccountType != UserAccountType.Distribution
                && !String.IsNullOrWhiteSpace(s.User?.GetEmail()) && s.IsSubscribed && !this.SentToUsers.Contains(s.UserId))
            .Select(s =>
            {
                var email = s.User!.GetEmail();
                var context = new Dictionary<string, object>() {
                    { "id", s.UserId },
                    { "firstName", s.User?.FirstName ?? "" },
                    { "lastName", s.User?.LastName ?? "" },
                };
                return new EmailContextModel(new[] { email }, context, now)
                {
                    Tag = $"{this.Notification.Name}",
                };
            }));
        var distributions = this.Notification.Subscribers.Where(s => s.User!.AccountType == UserAccountType.Distribution
            && s.IsSubscribed && !this.SentToUsers.Contains(s.UserId));
        emails.AddRange(distributions.SelectMany((u) =>
        {
            var addresses = u.User!.Preferences.GetElementValue<API.Models.UserEmailModel[]?>(".addresses");
            return addresses?.Select((a) =>
            {
                var context = new Dictionary<string, object>() {
                    { "id", a.UserId },
                };
                return new EmailContextModel(new[] { a.Email }, context, now)
                {
                    Tag = $"{this.Notification.Name}",
                };
            }) ?? Array.Empty<EmailContextModel>();
        }));
        return emails.GroupBy(context => String.Join(",", context.To)).Select(group => group.First());
    }

    /// <summary>
    /// Modify the Elasticsearch query to include the content ID.
    /// This will ensure we only compare a single content item in the search and reduce the bandwidth needed.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="contentId"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static JsonDocument ModifyElasticQuery(JsonDocument query, long contentId)
    {
        var json = JsonNode.Parse(query.ToJson())?.AsObject();
        if (json == null) return query;

        var jMustTerm = JsonNode.Parse($"{{ \"term\": {{ \"id\": {contentId} }}}}")?.AsObject() ?? throw new InvalidOperationException("Failed to parse JSON");
        if (json.TryGetPropertyValue("query", out JsonNode? jQuery))
        {
            if (jQuery?.AsObject().TryGetPropertyValue("bool", out JsonNode? jQueryBool) == true)
            {
                if (jQueryBool?.AsObject().TryGetPropertyValue("must", out JsonNode? jQueryBoolMust) == true)
                {
                    jQueryBoolMust?.AsArray().Add(jMustTerm);
                }
                else
                {
                    jQueryBool?.AsObject().Add("must", JsonNode.Parse($"[ {jMustTerm.ToJsonString()} ]"));
                }
            }
            else
            {
                jQuery?.AsObject().Add("bool", JsonNode.Parse($"{{ \"must\": [ {jMustTerm.ToJsonString()} ]}}"));
            }
        }
        else
        {
            json.Add("query", JsonNode.Parse($"{{ \"bool\": {{ \"must\": [ {jMustTerm.ToJsonString()} ] }}}}"));
        }
        return JsonDocument.Parse(json.ToJsonString());
    }
    #endregion
}
