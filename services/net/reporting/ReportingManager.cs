using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Services.Managers;
using TNO.Services.Reporting.Config;
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
using RazorLight;
using TNO.Services.Reporting.Models;

namespace TNO.Services.Reporting;

/// <summary>
/// ReportingManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class ReportingManager : ServiceManager<ReportingOptions>
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
    protected IKafkaListener<string, ReportRequestModel> Listener { get; }

    /// <summary>
    /// get - Razor template engine.
    /// </summary>
    protected IRazorLightEngine RazorEngine { get; }

    /// <summary>
    /// get - CHES service.
    /// </summary>
    protected IChesService Ches { get; }

    /// <summary>
    /// get - CHES options.
    /// </summary>
    protected ChesOptions ChesOptions { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportingManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="razorEngine"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="reportOptions"></param>
    /// <param name="logger"></param>
    public ReportingManager(
        IKafkaListener<string, ReportRequestModel> listener,
        IApiService api,
        ClaimsPrincipal user,
        IRazorLightEngine razorEngine,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<ReportingOptions> reportOptions,
        ILogger<ReportingManager> logger)
        : base(api, reportOptions, logger)
    {
        _user = user;
        this.RazorEngine = razorEngine;
        this.Ches = chesService;
        this.ChesOptions = chesOptions.Value;
        _serializationOptions = serializationOptions.Value;
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
    /// the report and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, ReportRequestModel> result)
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
                await ProcessReportAsync(result);

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
        finally
        {
            if (State.Status == ServiceStatus.Running) Listener.Resume();
        }
    }

    /// <summary>
    /// Process the report request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessReportAsync(ConsumeResult<string, ReportRequestModel> result)
    {
        var request = result.Message.Value;
        if (request.Destination.HasFlag(ReportDestination.ReportingService))
        {
            if (request.ReportInstanceId.HasValue)
            {
                var instance = await this.Api.GetReportInstanceAsync(request.ReportInstanceId.Value);
                if (instance != null)
                {
                    await GenerateReportAsync(request, instance);
                }
                else
                    this.Logger.LogDebug("Report instance does not exist.  Report Instance: {instance}", request.ReportInstanceId);
            }
            else
            {
                var report = await this.Api.GetReportAsync(request.ReportId);
                if (report != null)
                {
                    await GenerateReportAsync(request, report);
                }
                else
                    this.Logger.LogDebug("Report does not exist.  Report: {report}", request.ReportId);
            }
        }
    }

    /// <summary>
    /// Send out an email for the specified report.
    /// Generate a report instance for this email.
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task GenerateReportAsync(ReportRequestModel request, API.Areas.Services.Models.Report.ReportModel report)
    {
        // TODO: Control when a report is sent through configuration.
        var result = await this.Api.FindContentForReportIdAsync(report.Id);
        var content = result.Hits.Hits.Select(h => h.Source);

        var to = report.Subscribers.Where(s => !String.IsNullOrWhiteSpace(s.User?.Email)).Select(s => s.User!.Email).ToArray();
        var subject = await GenerateReportSubjectAsync(report, content, request.UpdateCache);
        var body = await GenerateReportBodyAsync(report, content, request.UpdateCache);

        var response = await SendEmailAsync(request, to, subject, body, $"{report.Name}-{report.Id}");

        // Save the report instance.
        var instance = new ReportInstance(report.Id, content.Select(c => c.Id))
        {
            PublishedOn = DateTime.UtcNow,
            Response = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions))
        };
        await this.Api.AddReportInstanceAsync(new API.Areas.Services.Models.ReportInstance.ReportInstanceModel(instance, _serializationOptions));
    }

    /// <summary>
    /// Send out an email for the specified report instance.
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="reportInstance"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task GenerateReportAsync(ReportRequestModel request, API.Areas.Services.Models.ReportInstance.ReportInstanceModel reportInstance)
    {
        // TODO: Control when a report is sent through configuration.
        var report = reportInstance.Report ?? throw new ArgumentException("Report instance must include the report model.");
        var content = await this.Api.GetContentForReportInstanceIdAsync(reportInstance.Id);

        var to = report.Subscribers.Where(s => !String.IsNullOrWhiteSpace(s.User?.Email)).Select(s => s.User!.Email).ToArray();
        var subject = await GenerateReportSubjectAsync(reportInstance.Report, content, request.UpdateCache);
        var body = await GenerateReportBodyAsync(reportInstance.Report, content, request.UpdateCache);

        var response = await SendEmailAsync(request, to, subject, body, $"{report.Name}-{report.Id}");

        // Update the report instance.
        var json = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
        if (reportInstance.PublishedOn == null) reportInstance.PublishedOn = DateTime.UtcNow;
        reportInstance.Response = JsonSerializer.Deserialize<Dictionary<string, object>>(json, _serializationOptions) ?? new Dictionary<string, object>();
        await this.Api.UpdateReportInstanceAsync(reportInstance);


    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="content"></param>
    /// <param name="updateCache"></param>
    /// <returns></returns>
    private async Task<string> GenerateReportSubjectAsync(API.Areas.Services.Models.Report.ReportModel report, IEnumerable<API.Areas.Services.Models.Content.ContentModel> content, bool updateCache = false)
    {
        // TODO: Having a key for every version is a memory leak, but the RazorLight library is junk and has no way to invalidate a cached item.
        var key = $"report_{report.Id}-{report.Version}_subject";
        var model = new TemplateModel(content, this.Options);
        var cache = this.RazorEngine.Handler.Cache?.RetrieveTemplate(key);
        if (!updateCache && cache?.Success == true)
            return await this.RazorEngine.RenderTemplateAsync(cache.Template.TemplatePageFactory(), model);
        else
        {
            return await this.RazorEngine.CompileRenderStringAsync(key, report.Settings.GetDictionaryJsonValue<string>("subject") ?? "", model);
        }
    }

    /// <summary>
    /// Generate the output of the report with the Razor engine.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<string> GenerateReportBodyAsync(API.Areas.Services.Models.Report.ReportModel report, IEnumerable<API.Areas.Services.Models.Content.ContentModel> content, bool updateCache = false)
    {
        // TODO: Having a key for every version is a memory leak, but the RazorLight library is junk and has no way to invalidate a cached item.
        var key = $"report_{report.Id}-{report.Version}";
        var model = new TemplateModel(content, this.Options);
        var cache = this.RazorEngine.Handler.Cache?.RetrieveTemplate(key);
        if (!updateCache && cache?.Success == true)
            return await this.RazorEngine.RenderTemplateAsync(cache.Template.TemplatePageFactory(), model);
        else
        {
            return await this.RazorEngine.CompileRenderStringAsync(key, report.Template, model);
        }
    }

    /// <summary>
    /// Send an email to CHES.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="to"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private async Task<EmailResponseModel?> SendEmailAsync(ReportRequestModel request, IEnumerable<string> to, string subject, string body, string tag)
    {
        await HandleChesEmailOverrideAsync(request.RequestorId);

        var contexts = new List<EmailContextModel>();
        if (!String.IsNullOrWhiteSpace(request.To))
        {
            // Add a context for the requested list of users.
            var another = request.To.Split(",").Select(v => v.Trim());
            contexts.Add(new EmailContextModel(another, new Dictionary<string, object>(), DateTime.Now)
            {
                Tag = tag,
            });
        }
        else
        {
            contexts = to.Select(v => new EmailContextModel(new[] { v }, new Dictionary<string, object>(), DateTime.Now)
            {
                Tag = tag,
            }).ToList();
        }

        var merge = new EmailMergeModel(this.ChesOptions.From, contexts, subject, body)
        {
            // TODO: Extract values from report settings.
            Encoding = EmailEncodings.Utf8,
            BodyType = EmailBodyTypes.Html,
            Priority = EmailPriorities.Normal,
        };

        var response = await this.Ches.SendEmailAsync(merge);
        this.Logger.LogInformation("Report sent to CHES.  Report: {report}", request.ReportId);

        return response;
    }

    /// <summary>
    /// If CHES has been configured to send emails to the user we need to provide an appropriate user.
    /// </summary>
    /// <param name="requestorId"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleChesEmailOverrideAsync(int? requestorId)
    {
        // The requestor becomes the current user.
        var email = this.ChesOptions.OverrideTo ?? "";
        if (requestorId.HasValue)
        {
            var user = await this.Api.GetUserAsync(requestorId.Value);
            if (user != null) email = user.Email;
        }
        var identity = _user.Identity as ClaimsIdentity ?? throw new ConfigurationException("CHES requires an active ClaimsPrincipal");
        identity.RemoveClaim(_user.FindFirst(ClaimTypes.Email));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
    }
    #endregion
}
