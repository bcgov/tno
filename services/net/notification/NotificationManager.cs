using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Notification.Config;
using TNO.Kafka.Models;
using Confluent.Kafka;
using TNO.Kafka;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Models.Extensions;
using TNO.Ches;
using TNO.Ches.Models;
using TNO.Ches.Configuration;
using System.Text.Json;
using System.Security.Claims;
using TNO.Entities.Validation;
using TNO.Services.Notification.Models;
using TNO.API.Areas.Services.Models.Notification;
using TNO.API.Areas.Services.Models.Content;

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
    private readonly ITnoRazorLightEngine _engine;
    private readonly IOptions<NotificationOptions> _notificationOptions;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer.
    /// </summary>
    protected IKafkaListener<string, NotificationRequestModel> Listener { get; }

    /// <summary>
    /// get - CHES service.
    /// </summary>
    protected IChesService Ches { get; }

    /// <summary>
    /// get - CHES options.
    /// </summary>
    protected ChesOptions ChesOptions { get; }

    /// <summary>
    /// get - Notification validator.
    /// </summary>
    protected NotificationValidator NotificationValidator { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="notificationOptions"></param>
    /// <param name="notificationValidator"></param>
    /// <param name="logger"></param>
    public NotificationManager(
        IKafkaListener<string, NotificationRequestModel> listener,
        IApiService api,
        ClaimsPrincipal user,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<NotificationOptions> notificationOptions,
        INotificationValidator notificationValidator,
        ITnoRazorLightEngine engine,
        ILogger<NotificationManager> logger)
        : base(api, notificationOptions, logger)
    {
        _user = user;
        this.Ches = chesService;
        this.ChesOptions = chesOptions.Value;
        _serializationOptions = serializationOptions.Value;
        this.NotificationValidator = notificationValidator as NotificationValidator ?? throw new ArgumentException("NotificationValidator must be of the correct type");
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
        _engine = engine;
        _notificationOptions = notificationOptions;
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
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
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
                this.Listener.Resume();

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
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Process the notification request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessNotificationAsync(ConsumeResult<string, NotificationRequestModel> result)
    {
        var request = result.Message.Value;
        if (request.Destination.HasFlag(NotificationDestination.NotificationService) && request.ContentId.HasValue)
        {
            var content = await this.Api.FindContentByIdAsync(request.ContentId.Value);
            if (content != null)
            {
                // If the request specified a notification then use it, otherwise test all notifications.
                if (request.NotificationId.HasValue)
                {
                    var notification = await this.Api.GetNotificationAsync(request.NotificationId.Value);
                    if (notification != null)
                    {
                        await this.NotificationValidator.InitializeAsync(notification, content, this.Options.AlertId);
                        if (this.NotificationValidator.ConfirmSend())
                            await SendNotificationAsync(request, notification, content);
                        else
                            this.Logger.LogDebug("Notification not sent.  Notification: {notification}, Content ID: {contentId}", notification.Id, content.Id);
                    }
                    else
                        this.Logger.LogDebug("Notification does not exist.  Notification: {notification}", request.NotificationId);
                }
                else
                {
                    var notifications = await this.Api.GetAllNotificationsAsync();
                    foreach (var notification in notifications)
                    {
                        await this.NotificationValidator.InitializeAsync(notification, content, this.Options.AlertId);
                        if (this.NotificationValidator.ConfirmSend())
                            await SendNotificationAsync(request, notification, content);
                        else
                            this.Logger.LogDebug("Notification not sent.  Notification: {notification}, Content ID: {contentId}", notification.Id, content.Id);
                    }
                }
            }
            else
            {
                // Identify requests for notification for content that does not exist.
                this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, request.ContentId);
            }
        }
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
    private async Task SendNotificationAsync(NotificationRequestModel request, NotificationModel notification, ContentModel content)
    {
        await HandleChesEmailOverrideAsync(request);

        var to = notification.Subscribers.Where(s => !String.IsNullOrWhiteSpace(s.User?.Email)).Select(s => s.User!.Email).ToArray();
        // TODO: Control when a notification is sent through configuration.
        var contexts = to.Select(v => new EmailContextModel(new[] { v }, new Dictionary<string, object>(), DateTime.Now)
        {
            Tag = $"{notification.Name}-{content.Id}",
        }).ToList();

        if (!String.IsNullOrWhiteSpace(request.To))
        {
            // Add a context for the requested list of users in addition to the subscribers.
            var requestTo = request.To.Split(",").Select(v => v.Trim());
            contexts.Add(new EmailContextModel(requestTo, new Dictionary<string, object>(), DateTime.Now));
        }

        var subject = await GenerateNotificationSubjectAsync(notification, content);
        var body = await GenerateNotificationBodyAsync(notification, content);
        var merge = new EmailMergeModel(this.ChesOptions.From, contexts, subject, body)
        {
            // TODO: Extract values from notification settings.
            Encoding = EmailEncodings.Utf8,
            BodyType = EmailBodyTypes.Html,
            Priority = EmailPriorities.Normal,
        };

        var response = await this.Ches.SendEmailAsync(merge);
        this.Logger.LogInformation("Notification sent to CHES.  Notification: {notification}, Content ID: {contentId}", notification.Id, content.Id);

        // Save the notification instance.
        var instance = new NotificationInstance(notification.Id, content.Id)
        {
            Response = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions))
        };
        await this.Api.AddNotificationInstanceAsync(new API.Areas.Services.Models.NotificationInstance.NotificationInstanceModel(instance, _serializationOptions));
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
            if (user != null) email = user.Email;
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
    /// <returns></returns>
    private async Task<string> GenerateNotificationSubjectAsync(NotificationModel notification, ContentModel content)
    {
        return await GenerateNotificationAsync(notification, content, true);
    }

    /// <summary>
    /// Generate the output of the notification with the Razor engine.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<string> GenerateNotificationBodyAsync(NotificationModel notification, ContentModel content)
    {
        return await GenerateNotificationAsync(notification, content);
    }

    private async Task<string> GenerateNotificationAsync(NotificationModel notification, ContentModel content, bool isSubject = false)
    {
        var result = await _engine.CompileRenderStringAsync(
            notification.Name + "_" + notification.Id + (isSubject ? "_subject" : ""),
            isSubject ? (notification.Settings.GetDictionaryJsonValue<string>("subject") ?? "") : notification.Template,
            new TemplateModel(_notificationOptions)
            {
                Content = content,
            });
        return result;
    }
    #endregion
}
