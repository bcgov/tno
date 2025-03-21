using System.Security.Claims;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Ches.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Managers;
using TNO.Services.Notification.Config;
using TNO.Services.Notification.Validation;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Config;

namespace TNO.Services.Notification;

/// <summary>
/// NotificationManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class NotificationManager : ServiceManager<NotificationOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly ClaimsPrincipal _user;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer.
    /// </summary>
    protected IKafkaListener<string, NotificationRequestModel> Listener { get; }

    /// <summary>
    /// get - Razor report template engine.
    /// </summary>
    protected INotificationEngine NotificationEngine { get; }

    /// <summary>
    /// get - Template options.
    /// </summary>
    protected TemplateOptions TemplateOptions { get; }

    /// <summary>
    /// get - Notification validator.
    /// </summary>
    protected INotificationValidator NotificationValidator { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="notificationEngine"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="notificationOptions"></param>
    /// <param name="reportingOptions"></param>
    /// <param name="notificationValidator"></param>
    /// <param name="logger"></param>
    public NotificationManager(
        IKafkaListener<string, NotificationRequestModel> listener,
        IApiService api,
        ClaimsPrincipal user,
        INotificationEngine notificationEngine,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<NotificationOptions> notificationOptions,
        IOptions<TemplateOptions> reportingOptions,
        INotificationValidator notificationValidator,
        ILogger<NotificationManager> logger)
        : base(api, chesService, chesOptions, notificationOptions, logger)
    {
        _user = user;
        this.NotificationEngine = notificationEngine;
        this.TemplateOptions = reportingOptions.Value;
        _serializationOptions = serializationOptions.Value;
        this.NotificationValidator = notificationValidator as NotificationValidator ?? throw new ArgumentException("NotificationValidator must be of the correct type");
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Listen to active topics and import content.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Listener.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    var topics = this.Options.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (topics.Length != 0)
                    {
                        this.Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                    await this.SendErrorEmailAsync("Service had an Unexpected Failure", ex);
                }
            }

            // The delay ensures we don't have a run away thread.
            this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new Task if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            // Make sure the prior task is cancelled before creating a new one.
            if (_cancelToken?.IsCancellationRequested == false)
                _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Run(ListenerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ListenerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Listener.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private void ListenerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                this.Listener.Stop();
        }
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken != null && !_cancelToken.IsCancellationRequested)
        {
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Retrieve a file from storage and send to Microsoft Cognitive Services. Obtain
    /// the notification and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, NotificationRequestModel> result)
    {
        try
        {
            // The service has stopped, so to should consuming messages.
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Listener.Stop();
                this.State.Stop();
            }
            else
            {
                await ProcessNotificationAsync(result);

                // Inform Kafka this message is completed.
                this.Listener.Commit(result);

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
            }
        }
        catch (Exception ex)
        {
            if (ex is HttpClientRequestException httpEx)
            {
                this.Logger.LogError(ex, "HTTP exception while consuming. {response}", httpEx.Data["body"] ?? "");
                await this.SendErrorEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
                await this.SendErrorEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
        finally
        {
            if (State.Status == ServiceStatus.Running) Listener.Resume();
        }
    }

    /// <summary>
    /// Process the notification request.
    /// There are two types of notification requests.
    /// 1) A specific notification has been requested to be sent out
    /// 2) All notifications will be validated to determine if they should be sent out for the specific content.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessNotificationAsync(ConsumeResult<string, NotificationRequestModel> result)
    {
        var request = result.Message.Value;
        if (request.Destination.HasFlag(NotificationDestination.NotificationService))
        {
            if (request.ContentId.HasValue)
            {
                var content = await this.Api.FindContentByIdAsync(request.ContentId.Value, true);
                if (content != null)
                {
                    this.Logger.LogInformation("Notification requested.  Content ID: {contentId}", content.Id);
                    await this.NotificationValidator.InitializeContentAsync(content);
                    // If the request specified a notification then use it, otherwise test all notifications.
                    if (request.NotificationId.HasValue)
                    {
                        var notification = await this.Api.GetNotificationAsync(request.NotificationId.Value);
                        if (notification != null && notification.IsEnabled)
                        {
                            this.NotificationValidator.InitializeNotification(notification);
                            if (request.IgnoreValidation || await this.NotificationValidator.ConfirmSendAsync())
                                await SendNotificationAsync(request, notification, content);
                            else
                                this.Logger.LogInformation("Notification not sent.  Notification: {notification}, Content ID: {contentId}", notification.Id, content.Id);
                        }
                        else
                            this.Logger.LogDebug("Notification does not exist or is disabled.  Notification: {notification}", request.NotificationId);
                    }
                    else
                    {
                        // Only fetch notifications that are configured to alert on index.
                        var notifications = await this.Api.FindNotificationsAsync(new TNO.Models.Filters.NotificationFilter()
                        {
                            AlertOnIndex = true,
                            IsEnabled = true,
                        });
                        foreach (var notification in notifications)
                        {
                            this.NotificationValidator.InitializeNotification(notification);
                            if (request.IgnoreValidation || await this.NotificationValidator.ConfirmSendAsync())
                                await SendNotificationAsync(request, notification, content);
                            else
                                this.Logger.LogDebug("Notification not sent.  Notification: {notification}, Content ID: {contentId}", notification.Id, content.Id);
                        }
                    }

                    await ResetAlertAsync(content);
                }
                else
                {
                    // Identify requests for notification for content that does not exist.
                    this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, request.ContentId);
                }
            }
            else if (request.NotificationId.HasValue)
            {
                // A notification with no content will execute its filter to find content.
                // It will only return content posted on after the prior instance.
                var notification = await this.Api.GetNotificationAsync(request.NotificationId.Value);
                if (notification != null)
                {
                    if (notification.IsEnabled)
                    {
                        // Fetch content.
                        var results = await this.Api.FindContentForNotificationIdAsync(request.NotificationId.Value, request.RequestorId);
                        if (results != null && results.Hits.Hits.Any() == true)
                        {
                            foreach (var content in results.Hits.Hits.Select(h => h.Source).Where(c => c != null))
                            {
                                this.NotificationValidator.Initialize(notification, content);
                                if (request.IgnoreValidation || await this.NotificationValidator.ConfirmSendAsync())
                                    await SendNotificationAsync(request, notification, content);

                                await ResetAlertAsync(content);
                            }
                        }
                        else
                            this.Logger.LogDebug("Notifications not sent.  No content found.  Notification: {notification}", notification.Id);
                    }
                    else
                        this.Logger.LogDebug("Notification is disabled.  Notification: {notification}", notification.Id);
                }
                else
                    this.Logger.LogDebug("Notification does not exist.  Notification: {notification}", request.NotificationId);
            }
        }
    }

    /// <summary>
    /// The alert has been sent, remove the flag from the content so that future alerts can be sent.
    /// </summary>
    /// <param name="content"></param>
    private async Task ResetAlertAsync(API.Areas.Services.Models.Content.ContentModel content)
    {
        // The alert has been sent, remove the flag from the content so that future alerts can be sent.
        var alert = content.Actions.FirstOrDefault(a => a.Id == this.Options.AlertId);
        if (alert != null && alert.Value == "true")
        {
            alert.Value = "false";
            await this.Api.UpdateContentActionAsync(alert);
        }
    }

    /// <summary>
    /// Get all the subscribers for the specified 'notification' and 'content'.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<IEnumerable<API.Areas.Services.Models.Notification.UserModel>> GetNotificationSubscribersAsync(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content)
    {
        // Get all the subscribers for this notification.
        // Expand any distribution lists.
        var subscribers = new List<API.Areas.Services.Models.Notification.UserModel>();
        subscribers.AddRange(await notification.Subscribers.Where(s => s.IsSubscribed).SelectManyAsync(async subscriber =>
        {
            if (subscriber.User == null) return Array.Empty<API.Areas.Services.Models.Notification.UserModel>();

            if (subscriber.User.AccountType == UserAccountType.Distribution)
            {
                var users = await this.Api.GetDistributionListAsync(subscriber.UserId);
                var filteredUsers = users.Where(u => !u.IsVacationMode());

                return filteredUsers.Select(u => new API.Areas.Services.Models.Notification.UserModel(u));
            }
            else
            {
                return !subscriber.User.IsVacationMode()
                    ? new[] { subscriber.User }
                    : Array.Empty<API.Areas.Services.Models.Notification.UserModel>();
            }
        }));

        // Add any users who are subscribed to the content.
        // Users can subscriber to content by asking for a transcript.
        subscribers.AddRange(content.UserNotifications.Where(cun => cun.User != null && cun.IsSubscribed && cun.User.AccountType != UserAccountType.Distribution && !cun.User.IsVacationMode())
            .Select(cun => new API.Areas.Services.Models.Notification.UserModel(cun.User!)));

        // Remove duplicate subscribers.  This can occur if a user is both subscribed to the content and the notification.
        return subscribers.GroupBy(s => s.Id).Select(s => s.First());
    }

    /// <summary>
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task SendNotificationAsync(
        NotificationRequestModel request,
        API.Areas.Services.Models.Notification.NotificationModel notification,
        API.Areas.Services.Models.Content.ContentModel content)
    {
        await HandleChesEmailOverrideAsync(request);

        var subscribers = await GetNotificationSubscribersAsync(notification, content);

        var body = await GenerateNotificationBodyAsync(notification, content, null, request.IsPreview);
        var subject = string.Empty;
        if (!String.IsNullOrWhiteSpace(request.To))
        {
            var contexts = new List<EmailContextModel>();
            // When a notification request has specified 'To' it means only send it to the emails in that property.
            var requestTo = request.To.Split(",").Select(v => v.Trim());
            contexts.Add(new EmailContextModel(requestTo, new Dictionary<string, object>(), DateTime.Now));
            // There are no subscribers, or a notification has been sent for this content to all the subscribers.
            if (!contexts.Any())
            {
                if (!subscribers.Any())
                {
                    this.Logger.LogDebug("Notification does not have subscribers. Notification: {notificationId}", notification.Id);
                }
                else
                {
                    this.Logger.LogInformation("Notification is not sent because all users has already received this content. Notification: {notificationId}", notification.Id);
                }
                return;
            }
            subject = await GenerateNotificationSubjectAsync(notification, content, request.IsPreview, false);
            SendOutNotificationEmailsAsync(contexts, subject, body, new List<API.Areas.Services.Models.Notification.UserModel>(), request, notification, content);
        }
        else
        {
            foreach (var subscriber in subscribers)
            {
                var contexts = new List<EmailContextModel>();
                contexts.AddRange(this.NotificationValidator.GetSubscriberEmails(new List<API.Areas.Services.Models.Notification.UserModel>() { subscriber }));

                // There are no subscribers, or a notification has been sent for this content to all the subscribers.
                if (!contexts.Any())
                {
                    this.Logger.LogInformation("Notification is not sent because user {subscriberName} has already received this content. Notification: {notificationId}",
                                                subscriber.DisplayName, notification.Id);
                    continue;
                }
                subject = await GenerateNotificationSubjectAsync(notification, content, request.IsPreview, subscriber.EnableReportSentiment());
                SendOutNotificationEmailsAsync(contexts, subject, body, new List<API.Areas.Services.Models.Notification.UserModel> { subscriber }, request, notification, content);
            }
        }
    }

    private async void SendOutNotificationEmailsAsync(IEnumerable<EmailContextModel> contexts, string subject, string body,
        IEnumerable<API.Areas.Services.Models.Notification.UserModel> subscriber,
        NotificationRequestModel request,
        API.Areas.Services.Models.Notification.NotificationModel notification,
        API.Areas.Services.Models.Content.ContentModel content)
    {
        var merge = new EmailMergeModel(this.ChesOptions.From, contexts, subject, body)
        {
            // TODO: Extract values from notification settings.
            Encoding = EmailEncodings.Utf8,
            BodyType = EmailBodyTypes.Html,
            Priority = EmailPriorities.Normal,
        };
        // Add the subscribers to the notification validator so that they don't receive more than one email for a specific content item.
        this.NotificationValidator.AddUsers(subscriber);
        var allEmails = String.Join(", ", contexts.Select(c => String.Join(", ", c.To)));

        try
        {
            var response = await this.Ches.SendEmailAsync(merge);
            this.Logger.LogInformation($"Notification sent to CHES. Notification: {notification.Id}, Content ID: {content.Id}, Subscriber: {subscriber.FirstOrDefault()?.DisplayName}, Emails: {allEmails}");

            if (!request.IsPreview)
            {
                // Save the notification instance.
                var instance = new NotificationInstance(notification.Id, content.Id)
                {
                    Status = NotificationStatus.Accepted,
                    SentOn = DateTime.UtcNow,
                    OwnerId = request.RequestorId ?? notification.OwnerId,
                    Response = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions)),
                    Subject = subject,
                    Body = body,
                };
                await this.Api.AddNotificationInstanceAsync(new API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel(instance, _serializationOptions));
            }
        }
        catch (ChesException ex)
        {
            this.Logger.LogError(ex, "Failed to send email.  Notification: {notificationId}, Content ID: {contentId}, Emails: {emails}", notification.Id, content.Id, allEmails);
            if (!request.IsPreview)
            {
                var instance = new NotificationInstance(notification.Id, content.Id)
                {
                    Status = NotificationStatus.Failed,
                    OwnerId = request.RequestorId ?? notification.OwnerId,
                    Response = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions)),
                    Subject = subject,
                    Body = body,
                };
                await this.Api.AddNotificationInstanceAsync(new API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel(instance, _serializationOptions));
            }
        }
    }

    /// <summary>
    /// If CHES has been configured to send emails to the user we need to provide an appropriate user.
    /// </summary>
    /// <param name="request"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleChesEmailOverrideAsync(NotificationRequestModel request)
    {
        // The requestor becomes the current user.
        var email = this.ChesOptions.OverrideTo ?? "";
        if (request.RequestorId.HasValue)
        {
            var user = await this.Api.GetUserAsync(request.RequestorId.Value);
            if (user != null) email = user.GetEmail();
        }
        var identity = _user.Identity as ClaimsIdentity ?? throw new ConfigurationException("CHES requires an active ClaimsPrincipal");
        identity.RemoveClaim(_user.FindFirst(ClaimTypes.Email));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
    }

    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    private async Task<string> GenerateNotificationSubjectAsync(API.Areas.Services.Models.Notification.NotificationModel notification, API.Areas.Services.Models.Content.ContentModel content,
        bool isPreview = false, bool enableReportSentiment = false)
    {
        if (notification.Template == null) throw new InvalidOperationException("Notification template cannot be null.  Update endpoint model serialization.");

        return await this.NotificationEngine.GenerateNotificationSubjectAsync(notification, new TNO.TemplateEngine.Models.ContentModel(content), isPreview, enableReportSentiment);
    }

    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <param name="uploadPath"></param>
    /// <param name="isPreview"></param>
    /// <returns></returns>
    private async Task<string> GenerateNotificationBodyAsync(
        API.Areas.Services.Models.Notification.NotificationModel notification,
        API.Areas.Services.Models.Content.ContentModel content,
        string? uploadPath = null,
        bool isPreview = false)
    {
        if (notification.Template == null) throw new InvalidOperationException("Notification template cannot be null.  Update endpoint model serialization.");

        return await this.NotificationEngine.GenerateNotificationBodyAsync(notification, new TNO.TemplateEngine.Models.ContentModel(content), uploadPath, isPreview);
    }
    #endregion
}
