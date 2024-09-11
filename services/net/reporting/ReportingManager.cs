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
using TNO.Elastic.Models;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Managers;
using TNO.Services.Reporting.Config;
using TNO.Services.Reporting.Models;
using TNO.TemplateEngine;
using TNO.TemplateEngine.Converters;
using TNO.TemplateEngine.Models;
using TNO.TemplateEngine.Models.Reports;

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
    static readonly ReportDistributionFormat[] LinkOnlyFormats = new[] { ReportDistributionFormat.LinkOnly, ReportDistributionFormat.ReceiveBoth };
    static readonly ReportDistributionFormat[] FullTextFormats = new[] { ReportDistributionFormat.FullText, ReportDistributionFormat.ReceiveBoth };
    static readonly ReportStatus[] _successfulEmailStatuses = new[] { ReportStatus.Accepted, ReportStatus.Completed };
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer.
    /// </summary>
    protected IKafkaListener<string, ReportRequestModel> Listener { get; }

    /// <summary>
    /// get - Razor report template engine.
    /// </summary>
    protected IReportEngine ReportEngine { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportingManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="reportEngine"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="reportOptions"></param>
    /// <param name="logger"></param>
    public ReportingManager(
        IKafkaListener<string, ReportRequestModel> listener,
        IApiService api,
        ClaimsPrincipal user,
        IReportEngine reportEngine,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<ReportingOptions> reportOptions,
        ILogger<ReportingManager> logger)
        : base(api, chesService, chesOptions, reportOptions, logger)
    {
        _user = user;
        this.ReportEngine = reportEngine;
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
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Listener.Stop();
                _cancelToken?.Cancel();
            }
            else if (this.State.Status == ServiceStatus.Failed)
            {
                this.Logger.LogInformation("The service has failed: '{Status}'", this.State.Status);
                if (this.Options.AutoRestartAfterFailure)
                {
                    await Task.Delay(this.Options.RetryAfterFailedDelayMS);
                    this.State.Resume();
                    this.Listener.Resume();
                }
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogInformation("The service is not running: '{Status}'", this.State.Status);
            }

            if (this.State.Status == ServiceStatus.Running)
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
            this.Logger.LogTrace("Service sleeping for {delay} ms", delay);
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
    /// Add the request back to the queue.
    /// </summary>
    /// <typeparam name="ET"></typeparam>
    /// <param name="message"></param>
    /// <param name="ex"></param>
    /// <returns></returns>
    private async Task AddRequestToQueue<ET>(Message<string, ReportRequestModel> message, ET? ex = null)
        where ET : Exception
    {
        var failureCount = message.Value.Data.GetElementValue<int?>(".failureCount") ?? 0;
        if (this.Options.ResendOnFailure && failureCount < this.Options.ResendAttemptLimit)
        {
            message.Value.Resend = false;
            var reportingException = ex as ReportingException;
            if (reportingException != null)
            {
                message.Value.ReportInstanceId = reportingException.InstanceId;
                message.Value.Data = JsonDocument.Parse(JsonSerializer.Serialize(new { reportingException.Error, FailureCount = failureCount + 1 }, _serializationOptions));
            }
            else
            {
                message.Value.Data = JsonDocument.Parse(JsonSerializer.Serialize(new { FailureCount = failureCount + 1 }, _serializationOptions));
            }
            // Add the report request back to Kafka so that it runs again.
            await this.Api.SendMessageAsync(message.Value);
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
        var errorStatus = result.Message.Value.Data.GetElementValue<ReportingErrors?>(".error");
        if (errorStatus != null)
        {
            var failureCount = result.Message.Value.Data.GetElementValue<int?>(".failureCount") ?? 0;
            this.Logger.LogInformation("Retrying report after failure. ReportId:{reportId} InstanceId:{instanceId} FailureStatus:{status} RetryCount:{count}",
                result.Message.Value.ReportId,
                result.Message.Value.ReportInstanceId,
                errorStatus,
                failureCount);
        }
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

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
            }
        }
        catch (ReportingException ex)
        {
            // Failures will require retrying the report.  However, depending on the error the report instance could be in a unique state.
            // Resending a failure can result in cascading failures however, so it should limit the number of attempts.
            this.Logger.LogError(ex, "Failed to handle message");
            await this.SendErrorEmailAsync("Failed to handle message", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            // Add the report request back to Kafka so that it runs again.
            await AddRequestToQueue(result.Message, ex);
        }
        catch (HttpClientRequestException ex)
        {
            this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
            await this.SendErrorEmailAsync("HTTP exception while consuming. {response}", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            // Add the report request back to Kafka so that it runs again.
            await AddRequestToQueue(result.Message, ex);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to handle message");
            await this.SendErrorEmailAsync("Failed to handle message", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            // Add the report request back to Kafka so that it runs again.
            await AddRequestToQueue(result.Message, ex);
        }
        finally
        {
            // Inform Kafka this message is completed.
            // We do this regardless of whether it failed or succeeded.
            this.Listener.Commit(result);
            if (State.Status == ServiceStatus.Running)
            {
                this.Listener.Resume();
            }
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
            if (request.ReportType == Entities.ReportType.Content)
            {
                if (request.ReportInstanceId.HasValue)
                {
                    var instance = await this.Api.GetReportInstanceAsync(request.ReportInstanceId.Value);
                    if (instance != null)
                    {
                        await GenerateAndSendReportAsync(request, instance);

                        if (instance.Status == ReportStatus.Failed)
                            await AddRequestToQueue<Exception>(result.Message);
                    }
                    else
                        this.Logger.LogWarning("Report instance does not exist.  Report Instance: {id}", request.ReportInstanceId);
                }
                else
                {
                    var report = await this.Api.GetReportAsync(request.ReportId);
                    if (report != null)
                    {
                        var instance = await GenerateAndSendReportAsync(request, report);

                        if (instance != null && instance.Status == ReportStatus.Failed)
                            await AddRequestToQueue<Exception>(result.Message);
                    }
                    else
                        this.Logger.LogWarning("Report does not exist.  Report: {id}", request.ReportId);
                }
            }
            else if (request.ReportType == Entities.ReportType.AVOverview)
            {
                var instance = await this.Api.GetAVOverviewInstanceAsync(request.ReportInstanceId ?? request.ReportId);
                if (instance != null)
                {
                    await GenerateAndSendReportAsync(request, instance);
                }
                else
                    this.Logger.LogWarning("AV overview instance does not exist.  Instance: {id}", request.ReportInstanceId);
            }
            else throw new NotImplementedException($"Report template type '{request.ReportType.GetName()}' has not been implemented");
        }

        // If the request originated from the scheduler service, update the last ran one.
        if (request.EventScheduleId.HasValue)
        {
            await this.Api.HandleConcurrencyAsync<API.Areas.Services.Models.EventSchedule.EventScheduleModel?>(async () =>
            {
                var eventSchedule = await this.Api.GetEventScheduleAsync(request.EventScheduleId.Value) ?? throw new NoContentException($"Event schedule {request.EventScheduleId.Value} does not exist.");
                eventSchedule.LastRanOn = DateTime.UtcNow;
                return await this.Api.UpdateEventScheduleAsync(eventSchedule, false);
            });
        }
    }

    /// <summary>
    /// Make a request to the API and get the linked report content.
    /// </summary>
    /// <param name="reportId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private async Task<Dictionary<string, ReportSectionModel>> GetLinkedReportAsync(int reportId, int? ownerId)
    {
        var instance = await this.Api.GetCurrentReportInstanceAsync(reportId, ownerId);
        if (instance == null) return new();

        var sections = instance.Report?.Sections.ToDictionary(section => section.Name, section =>
        {
            var content = instance.Content.Where(c => c.SectionName == section.Name && c.Content != null).Select(c => new ContentModel(c.Content!, c.SortOrder, c.SectionName, section.Settings.Label));
            return new ReportSectionModel(section, content);
        }) ?? new();

        return sections;
    }

    /// <summary>
    /// Fetch the current report instance content if it should be copied into the next report instance.
    /// If the next report instance
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<(API.Areas.Services.Models.ReportInstance.ReportInstanceModel?, Dictionary<string, ReportSectionModel>)> PopulateReportInstance(ReportRequestModel request, API.Areas.Services.Models.Report.ReportModel report)
    {
        var sections = report.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s));
        var sectionContent = new Dictionary<string, ReportSectionModel>();
        API.Areas.Services.Models.Report.ReportInstanceModel? reportInstanceModel = null;

        try
        {
            reportInstanceModel = await this.Api.GetCurrentReportInstanceAsync(report.Id, request.RequestorId);
        }
        catch (Exception ex)
        {
            throw new ReportingException(report.Id, reportInstanceModel?.Id, ReportingErrors.FailedToGetInstance, $"Failed to fetch current report instance. ReportId:{report.Id}", ex);
        }

        try
        {
            // If we don't clear the prior report we must append new content to it.
            var currentSectionContent = new Dictionary<string, ReportSectionModel>();
            if (reportInstanceModel != null && !reportInstanceModel.SentOn.HasValue && !report.Settings.Content.ClearOnStartNewReport)
            {
                // Extract the section content from the current instance.
                currentSectionContent = sections.ToDictionary(s => s.Name, section =>
                {
                    section.Content = reportInstanceModel.Content.Where(ric => ric.SectionName == section.Name && ric.Content != null).Select(ric => new ContentModel(ric.Content!, ric.SortOrder, ric.SectionName));
                    return section;
                });
            }

            // Generate a new instance, or accumulate new content each time the report is run.
            if (reportInstanceModel == null || reportInstanceModel.SentOn.HasValue
                || !(!report.Settings.Content.ClearOnStartNewReport && !report.Settings.Content.CopyPriorInstance))
            {
                // Fetch content for every section within the report.  This will include folders and filters.
                var searchResults = await this.Api.FindContentForReportIdAsync(report.Id, request.RequestorId);
                sectionContent = sections.ToDictionary(section => section.Name, section =>
                {
                    if (searchResults.TryGetValue(section.Name, out SearchResultModel<TNO.API.Areas.Services.Models.Content.ContentModel>? results))
                    {
                        var sortOrder = 0;
                        var newContent = results.Hits.Hits.Select(h => new ContentModel(h.Source, sortOrder++)).OrderBy(c => c.SortOrder).ToArray();
                        IEnumerable<ContentModel> distinctContent;
                        if (report.Settings.Content.CopyPriorInstance)
                            distinctContent = (
                            currentSectionContent.TryGetValue(section.Name, out ReportSectionModel? currentSection)
                                ? currentSection?.Content.ToArray().AppendRange(newContent).DistinctBy(c => c.Id)
                                : newContent
                            ) ?? Array.Empty<ContentModel>();
                        else
                            distinctContent = newContent;

                        section.Content = ReportEngine.OrderBySectionField(distinctContent, section.Settings.SortBy, section.Settings.SortDirection);
                        if (results.Aggregations != null)
                        {
                            section.Aggregations = new Dictionary<string, TNO.TemplateEngine.Models.Reports.AggregationRootModel>();
                            foreach (var aggregation in results.Aggregations)
                            {
                                section.Aggregations.Add(aggregation.Key, aggregation.Value.Convert());
                            }
                        }
                    }
                    return section;
                });
            }
            else
            {
                // Copy the prior report content into the new instance.
                sectionContent = currentSectionContent;
            }

            // Fetch all image data.  Need to do this in a separate step because of async+await.
            // TODO: Review this implementation due to performance issues.
            foreach (var section in sectionContent)
            {
                foreach (var content in section.Value.Content.Where(c => c.ContentType == Entities.ContentType.Image))
                {
                    content.ImageContent = await this.Api.GetImageFile(content.Id);
                }
            }

            // KGM: this logic should be removed once the underlying problem of malformed data for report content is found
            // filter out Content with no valid ContentId
            var reportInstanceContents = sectionContent.SelectMany(s => s.Value.Content.Where(c => c.Id > 0).Select(c => new ReportInstanceContent(0, c.Id, s.Key, c.SortOrder)).ToArray()).ToArray();
            var reportInstanceContentsBad = sectionContent.SelectMany(s => s.Value.Content.Where(c => c.Id == 0).Select(c => new ReportInstanceContent(0, c.Id, s.Key, c.SortOrder)).ToArray()).ToArray();
            if (reportInstanceContentsBad.Any())
            {
                this.Logger.LogWarning("Report [{name}] {generateInstance} has malformed content. It will be generated, but may not match expectations.", report.Name, request.GenerateInstance);
                foreach (var section in sectionContent)
                {
                    sectionContent[section.Key].Content = section.Value.Content.Where(c => c.Id > 0);
                }
            }
        }
        catch (Exception ex)
        {
            throw new ReportingException(report.Id, null, ReportingErrors.FailedToGetContent, $"Failed to fetch content for report. ReportId:{report.Id}", ex);
        }

        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instanceModel = null;
        if (reportInstanceModel == null || reportInstanceModel.SentOn.HasValue)
        {
            try
            {
                // A new report instance will need to be created.
                var instanceContent = sectionContent.SelectMany(s => s.Value.Content.Select(c => new ReportInstanceContent(0, c.Id, s.Key, c.SortOrder)));
                var instance = new ReportInstance(
                    report.Id,
                    request.RequestorId,
                    instanceContent
                    )
                {
                    OwnerId = request.RequestorId ?? report.OwnerId,
                    PublishedOn = DateTime.UtcNow,
                };
                instanceModel = request.GenerateInstance ? (await this.Api.AddReportInstanceAsync(new API.Areas.Services.Models.ReportInstance.ReportInstanceModel(instance, _serializationOptions))
                    ?? throw new InvalidOperationException($"Report instance failed to be returned by API. ReportId:{report.Id}")) : null;
            }
            catch (Exception ex)
            {
                throw new ReportingException(report.Id, instanceModel?.Id, ReportingErrors.FailedToAddInstance, $"Failed to add instance to report. ReportId:{report.Id}", ex);
            }
        }
        else
        {
            try
            {
                var instanceContent = sectionContent.SelectMany(s => s.Value.Content.Select(c => new API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel(reportInstanceModel.Id, c.Id, s.Key, c.SortOrder)));
                instanceModel = new API.Areas.Services.Models.ReportInstance.ReportInstanceModel((Entities.ReportInstance)reportInstanceModel, _serializationOptions)
                {
                    OwnerId = request.RequestorId ?? report.OwnerId,
                    PublishedOn = DateTime.UtcNow,
                    Content = instanceContent,
                };
                instanceModel = await this.Api.UpdateReportInstanceAsync(instanceModel, true) ?? throw new InvalidOperationException($"Report instance failed to be returned by API. ReportId:{instanceModel.ReportId}, InstanceId:{instanceModel.Id}");
            }
            catch (Exception ex)
            {
                throw new ReportingException(report.Id, instanceModel?.Id, ReportingErrors.FailedToUpdateInstance, $"Failed to updated instance for report. ReportId:{report.Id}, InstanceId:{instanceModel?.Id}", ex);
            }
        }

        return (instanceModel, sectionContent);
    }

    /// <summary>
    /// Send out an email for the specified report.
    /// Generate a report instance for this email.
    /// If an unsent report instance exists, use it instead.
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task<API.Areas.Services.Models.ReportInstance.ReportInstanceModel?> GenerateAndSendReportAsync(ReportRequestModel request, API.Areas.Services.Models.Report.ReportModel report)
    {
        (var instanceModel, var sectionContent) = await PopulateReportInstance(request, report);
        await SendReportAsync(request, report, instanceModel, sectionContent);
        return instanceModel;
    }

    /// <summary>
    /// Determine if this request is to resend the report.
    /// A resend will result in a report being sent to all subscribers again.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    private static bool IsResend(ReportRequestModel request, API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance)
    {
        return request.Resend && !String.IsNullOrWhiteSpace(instance?.Subject) && !String.IsNullOrWhiteSpace(instance?.Body);
    }

    /// <summary>
    /// Determine if this request is a retry.
    /// A retry is due to a failure in the prior attempt.
    /// A retry will result in a report being sent only to failed subscribers.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    private static bool IsRetry(ReportRequestModel request, API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance)
    {
        var errorStatus = request.Data.GetElementValue<ReportingErrors?>(".error");
        return !IsResend(request, instance) && errorStatus.HasValue;
    }

    /// <summary>
    /// Send out emails for the specified report instance.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task GenerateAndSendReportAsync(ReportRequestModel request, API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance)
    {
        var resending = IsResend(request, instance);
        var retry = IsRetry(request, instance);
        var report = instance.Report ?? throw new ArgumentException("Report instance must include the report model.");
        var sections = report.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s));

        var searchResults = !resending && !retry ? await this.Api.GetContentForReportInstanceIdAsync(instance.Id) : Array.Empty<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>();
        var sectionContent = sections.ToDictionary(s => s.Name, section =>
        {
            section.Content = searchResults.Where(sr => sr.SectionName == section.Name && sr.Content != null).Select(ri => new ContentModel(ri.Content!, ri.SortOrder)).ToArray();
            return section;
        });

        if (!resending && !retry)
        {
            // Fetch all image data.  Need to do this in a separate step because of async+await.
            // TODO: Review this implementation due to performance issues.
            foreach (var section in sectionContent)
            {
                foreach (var content in section.Value.Content.Where(c => c.ContentType == Entities.ContentType.Image))
                {
                    content.ImageContent = await this.Api.GetImageFile(content.Id);
                }
            }
        }

        await SendReportAsync(request, report, instance, sectionContent);
    }

    /// <summary>
    /// Checks if a report has any subscribers, including those in distribution lists.
    /// </summary>
    /// <param name="report">The report model to check for subscribers.</param>
    /// <returns>True if there are any subscribers, false otherwise.</returns>
    private async Task<bool> CheckForSubscribersAsync(API.Areas.Services.Models.Report.ReportModel report)
    {
        var subscribers = report.Subscribers.Where(s => s.IsSubscribed && s.User != null && s.User.IsVacationMode() != true).ToArray();
        if (subscribers.Any())
        {
            return true;
        }

        // If there are no direct subscribers, check for distribution lists
        foreach (var subscriber in report.Subscribers)
        {
            if (subscriber.User?.AccountType == UserAccountType.Distribution)
            {
                var distributionList = await this.Api.GetDistributionListAsync(subscriber.UserId);
                if (distributionList.Any())
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Generate the report output and send it to all subscribers.
    /// Ensure the instance is updated with the results.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <param name="instance"></param>
    /// <param name="sectionContent"></param>
    /// <returns></returns>
    private async Task SendReportAsync(
        ReportRequestModel request,
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance,
        Dictionary<string, ReportSectionModel> sectionContent)
    {
        try
        {
            var (subject, linkBody, fullBody) = await GenerateReportOutputAsync(request, report, instance, sectionContent);

            if (request.SendToSubscribers || !String.IsNullOrEmpty(request.To))
            {
                var hasSubscribers = await CheckForSubscribersAsync(report);
                if (!hasSubscribers)
                {
                    if (instance != null)
                    {
                        instance.Status = ReportStatus.Cancelled;
                        instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = "No subscribers found for this report." }, _serializationOptions));
                    }
                    this.Logger.LogWarning("No subscribers found for report. ReportId:{reportId}", report.Id);
                    return;
                }

                await SendEmailsAsync(request, report, instance, subject, linkBody, fullBody);
            }

            // Only clear folders if the instance was successfully saved.
            if (instance != null &&
                report.Settings.Content.ClearFolders &&
                request.SendToSubscribers &&
                !IsResend(request, instance))
            {
                // TODO: On a failure or a resend this could result in removing content in a folder that is not part of the report (due to timing).
                await this.Api.ClearFoldersInReport(report.Id);
            }
        }
        finally
        {
            if (request.GenerateInstance && instance != null)
            {
                await UpdateReportInstanceAsync(instance, false);
            }
        }
    }

    /// <summary>
    /// Generate the subject, and both email body messages based on the report template and content.
    /// Only regenerate the report if required.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <param name="instance"></param>
    /// <param name="sectionContent"></param>
    /// <returns></returns>
    /// <exception cref="ReportingException"></exception>
    private async Task<(string subject, string linkBody, string fullBody)> GenerateReportOutputAsync(
        ReportRequestModel request,
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance,
        Dictionary<string, ReportSectionModel> sectionContent)
    {
        try
        {
            var resend = IsResend(request, instance);
            var retry = IsRetry(request, instance);
            var subject = resend || retry ? instance!.Subject : await this.ReportEngine.GenerateReportSubjectAsync(report, instance, sectionContent, false, false);
            // We regenerate the link only email because we don't save it.  This could be an issue if the template needed content information in it.
            var linkOnlyFormatBody = await this.ReportEngine.GenerateReportBodyAsync(report, instance, sectionContent, GetLinkedReportAsync, null, true, false);
            var fullTextFormatBody = resend || retry ? instance!.Body : await this.ReportEngine.GenerateReportBodyAsync(report, instance, sectionContent, GetLinkedReportAsync, null, false, false);

            return (subject, linkOnlyFormatBody, fullTextFormatBody);
        }
        catch (Exception ex)
        {
            if (instance != null)
            {
                instance.Status = ReportStatus.Failed;
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = ex.GetAllMessages() }, _serializationOptions));
            }

            throw new ReportingException(report.Id, instance?.Id, ReportingErrors.FailedToGenerateOutput, $"Failed to generate output for report.  ReportId:{report.Id}, InstanceId:{instance?.Id}", ex);
        }
    }

    /// <summary>
    /// Send report out by email to all subscribers and their configured formats (link and/or full body).
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <param name="instance"></param>
    /// <param name="subject"></param>
    /// <param name="linkBody"></param>
    /// <param name="fullBody"></param>
    /// <returns></returns>
    /// <exception cref="ReportingException"></exception>
    private async Task SendEmailsAsync(
        ReportRequestModel request,
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance,
        string subject,
        string linkBody,
        string fullBody)
    {
        // TODO: This implementation can result in one set of emails being successful and the second failing.
        var responseModel = new ReportEmailResponseModel();
        try
        {
            var userReportInstances = new List<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
            if (!this.Options.UseMailMerge && instance?.Id > 0)
            {
                // Get users who have either received the email, or failed.
                userReportInstances.AddRange(await this.Api.GetUserReportInstancesAsync(instance.Id));
            }

            var (linkEmails, fullEmails) = await GetEmailAddressesAsync(request, report, instance, userReportInstances);

            if (instance != null && request.GenerateInstance)
            {
                instance.Subject = subject;
                instance.Body = fullBody;
                if (instance.PublishedOn == null) instance.PublishedOn = DateTime.UtcNow;
                if (request.SendToSubscribers)
                    instance.SentOn = DateTime.UtcNow; // We track when it was sent, even if it failed.
            }

            if (!report.Settings.DoNotSendEmail && String.IsNullOrEmpty(request.To) && (linkEmails[EmailSentTo.To].Any() || linkEmails[EmailSentTo.CC].Any() || linkEmails[EmailSentTo.BCC].Any()))
            {
                // Send the email.
                var (linkOnlyStatus, responseLinkOnly) = await SendEmailAsync(
                    request,
                    linkEmails[EmailSentTo.To],
                    linkEmails[EmailSentTo.CC],
                    linkEmails[EmailSentTo.BCC],
                    subject,
                    linkBody,
                    $"{report.Name}-{report.Id}-linkOnly",
                    UpdateUserReportInstances(report, instance, userReportInstances, false));
                responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseLinkOnly, _serializationOptions));

                if (instance != null)
                {
                    instance.Status = linkOnlyStatus;
                    instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                }
            }

            if (!report.Settings.DoNotSendEmail && (fullEmails[EmailSentTo.To].Any() || fullEmails[EmailSentTo.CC].Any() || fullEmails[EmailSentTo.BCC].Any() || !String.IsNullOrEmpty(request.To)))
            {
                // Send the email.
                var (fullTextStatus, responseFullText) = await SendEmailAsync(
                    request,
                    fullEmails[EmailSentTo.To],
                    fullEmails[EmailSentTo.CC],
                    fullEmails[EmailSentTo.BCC],
                    subject,
                    fullBody,
                    $"{report.Name}-{report.Id}",
                    UpdateUserReportInstances(report, instance, userReportInstances, true));
                responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseFullText, _serializationOptions));

                if (instance != null)
                {
                    instance.Status = fullTextStatus;
                    instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                }
            }

            // If the report wasn't sent out due to having no subscribers, update the status based on the current status.
            if (instance != null && !linkEmails[EmailSentTo.To].Any() && !linkEmails[EmailSentTo.CC].Any() && !linkEmails[EmailSentTo.BCC].Any() &&
                !fullEmails[EmailSentTo.To].Any() && !fullEmails[EmailSentTo.CC].Any() && !fullEmails[EmailSentTo.BCC].Any())
            {
                // A report without subscribers is cancelled (it should never have gotten this far).
                // All other statuses are considered complete, as there is no more work to do.
                if (instance.Status == ReportStatus.Submitted)
                    instance.Status = ReportStatus.Cancelled;
                else
                    instance.Status = ReportStatus.Completed;
            }
        }
        catch (ChesException ex)
        {
            this.Logger.LogError(ex, "Failed to email report.  ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance?.Id);
            if (responseModel.LinkOnlyFormatResponse != null)
                responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));
            else
                responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));

            if (instance != null)
            {
                instance.Status = ReportStatus.Failed;
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
            }
            throw new ReportingException(report.Id, instance?.Id, ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instance?.Id}", ex);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to email report.  ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance?.Id);
            if (instance != null)
            {
                instance.Status = ReportStatus.Failed;
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
            }
            throw new ReportingException(report.Id, instance?.Id, ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instance?.Id}", ex);
        }
    }

    /// <summary>
    /// Update the specified report instance.
    /// Keep looping until successful.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="updateContent"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task UpdateReportInstanceAsync(API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance, bool updateContent)
    {
        // Need to keep trying until successful.
        var isUpdated = false;
        var count = 0;
        while (!isUpdated)
        {
            try
            {
                // We're getting optimistic errors here, most likely due to changes occurring from the UI.  Since we only need to change values related to status, we can get the latest and reapply.
                var latestInstanceModel = await this.Api.GetReportInstanceAsync(instance.Id) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
                if (latestInstanceModel.Version != instance.Version)
                {
                    latestInstanceModel.Subject = instance.Subject;
                    latestInstanceModel.Body = instance.Body;
                    latestInstanceModel.SentOn = instance.SentOn;
                    latestInstanceModel.Status = instance.Status;
                    latestInstanceModel.Response = instance.Response;
                    latestInstanceModel.PublishedOn = instance.PublishedOn;
                    instance = await this.Api.UpdateReportInstanceAsync(latestInstanceModel, updateContent) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
                }
                else
                    instance = await this.Api.UpdateReportInstanceAsync(instance, updateContent) ?? throw new InvalidOperationException("Report instance failed to be returned by API");

                isUpdated = true;
            }
            catch (Exception ex)
            {
                count++;
                this.Logger.LogError(ex, "Failed to update report instance. ReportId:{reportId} InstanceId:{instanceId}", instance.ReportId, instance.Id);
                if (count >= this.Options.RetryConcurrencyFailureLimit)
                    throw;
            }
        }
    }

    /// <summary>
    /// Provides callback function when sending an email to save the responses.
    /// </summary>
    /// <param name="report"></param>
    /// <param name="instance"></param>
    /// <param name="fullBodyReport"></param>
    /// <returns></returns>
    private Func<IEnumerable<UserEmail>, ReportStatus, JsonDocument, Task> UpdateUserReportInstances(
        API.Areas.Services.Models.Report.ReportModel report,
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance,
        List<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel> userReportInstances,
        bool fullBodyReport)
    {
        return async (IEnumerable<UserEmail> users, ReportStatus status, JsonDocument response) =>
        {
            if (instance?.Id > 0)
            {
                var updateUserReportInstances = new List<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
                foreach (var user in users)
                {
                    var userReportInstance = userReportInstances.FirstOrDefault(uri => uri.UserId == user.UserId);
                    if (userReportInstance == null)
                    {
                        userReportInstance = new API.Areas.Services.Models.ReportInstance.UserReportInstanceModel(user.UserId, instance.Id)
                        {
                            LinkStatus = !fullBodyReport ? status : ReportStatus.Pending,
                            LinkSentOn = !fullBodyReport ? DateTime.UtcNow : null,
                            LinkResponse = !fullBodyReport ? response : JsonDocument.Parse("{}"),
                            TextStatus = fullBodyReport ? status : ReportStatus.Pending,
                            TextSentOn = fullBodyReport ? DateTime.UtcNow : null,
                            TextResponse = fullBodyReport ? response : JsonDocument.Parse("{}"),
                        };
                        userReportInstances.Add(userReportInstance);
                    }
                    else
                    {
                        if (!fullBodyReport)
                        {
                            userReportInstance.LinkStatus = status;
                            userReportInstance.LinkSentOn = DateTime.UtcNow;
                            userReportInstance.LinkResponse = response;
                        }
                        else
                        {
                            userReportInstance.TextStatus = status;
                            userReportInstance.TextSentOn = DateTime.UtcNow;
                            userReportInstance.TextResponse = response;
                        }
                    }
                    updateUserReportInstances.Add(userReportInstance);
                }
                var toEmails = String.Join(",", users.Select(u => u.To));
                var ccEmails = String.Join(",", users.SelectMany(u => u.CC.Select(v => v.To)));
                var bccEmails = String.Join(",", users.SelectMany(u => u.BCC.Select(v => v.To)));
                this.Logger.LogDebug("Saving email responses.  reportId:{reportId}, InstanceId:{instanceId}, Status:{status}, To:{to}, CC:{cc}, BCC:{bcc}", report.Id, instance.Id, status, toEmails, ccEmails, bccEmails);
                await this.Api.AddOrUpdateUserReportInstancesAsync(updateUserReportInstances);
            }
        };
    }

    /// <summary>
    /// Update the specified report instance.
    /// Keep looping until successful.
    /// </summary>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task UpdateReportInstanceAsync(API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel instance)
    {
        // Need to keep trying until successful.
        var count = 0;
        var isUpdated = false;
        while (!isUpdated)
        {
            try
            {
                // We're getting optimistic errors here, most likely due to changes occurring from the UI.  Since we only need to change values related to status, we can get the latest and reapply.
                var latestInstanceModel = await this.Api.GetAVOverviewInstanceAsync(instance.Id) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
                if (latestInstanceModel.Version != instance.Version)
                {
                    latestInstanceModel.Response = instance.Response;
                    latestInstanceModel.PublishedOn = instance.PublishedOn;
                    instance = await this.Api.UpdateAVOverviewInstanceAsync(latestInstanceModel) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
                }
                else
                    instance = await this.Api.UpdateAVOverviewInstanceAsync(instance) ?? throw new InvalidOperationException("Report instance failed to be returned by API");

                isUpdated = true;
            }
            catch (Exception ex)
            {
                count++;
                this.Logger.LogError(ex, "Failed to update AV overview report instance. InstanceId:{instanceId}", instance.Id);
                if (count >= this.Options.RetryConcurrencyFailureLimit)
                    throw;
            }
        }
    }

    /// <summary>
    /// Get all the emails that both the link and full text report should be sent to.
    /// Do not include subscribers that have already received this report unless this is a resend.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="report"></param>
    /// <param name="instance"></param>
    /// <param name="userReportInstances"></param>
    /// <returns></returns>
    private async Task<(Dictionary<EmailSentTo, List<UserEmail>> link, Dictionary<EmailSentTo, List<UserEmail>> full)> GetEmailAddressesAsync(
            ReportRequestModel request,
            API.Areas.Services.Models.Report.ReportModel report,
            API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instance,
            IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel> userReportInstances)
    {
        var linkOnlyFormatSubscribers = report.Subscribers.Where(s =>
            s.IsSubscribed &&
            s.User != null &&
            LinkOnlyFormats.Contains(s.Format) &&
            (request.Resend ||
                !userReportInstances.Any(uri =>
                    uri.UserId == s.UserId &&
                    _successfulEmailStatuses.Contains(uri.LinkStatus) &&
                    (instance == null || uri.InstanceId == instance.Id)))).ToArray();
        var fullTextFormatSubscribers = report.Subscribers.Where(s =>
            s.IsSubscribed &&
            s.User != null &&
            FullTextFormats.Contains(s.Format) &&
            (request.Resend ||
                !userReportInstances.Any(uri =>
                    uri.UserId == s.UserId &&
                    _successfulEmailStatuses.Contains(uri.TextStatus) &&
                    (instance == null || uri.InstanceId == instance.Id)))).ToArray();

        var linkEmails = await GetEmailAddressesAsync(request, linkOnlyFormatSubscribers, userReportInstances);
        var fullEmails = await GetEmailAddressesAsync(request, fullTextFormatSubscribers, userReportInstances);

        return (linkEmails, fullEmails);
    }

    /// <summary>
    /// Get all the emails for this report request.
    /// Only include users who have not received an email, or include all users if the request is to resend.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="subscribers"></param>
    /// <param name="userReportInstances"></param>
    /// <returns></returns>
    private async Task<Dictionary<EmailSentTo, List<UserEmail>>> GetEmailAddressesAsync(
        ReportRequestModel request,
        API.Areas.Services.Models.Report.UserReportModel[] subscribers,
        IEnumerable<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel> userReportInstances)
    {
        var emails = new Dictionary<EmailSentTo, List<UserEmail>>()
        {
            { EmailSentTo.To, new List<UserEmail>() },
            { EmailSentTo.CC, new List<UserEmail>() },
            { EmailSentTo.BCC, new List<UserEmail>() },
        };

        foreach (var user in subscribers)
        {
            if (user.User == null) throw new InvalidOperationException("Report subscriber is missing user information");

            // Only include users who have not received an email yet.
            var (a, b, c) = await GetEmailAddressesAsync(user.UserId, user.User.GetEmail(), user.User.AccountType, user.SendTo);
            emails[EmailSentTo.To].AddRange(a.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
            emails[EmailSentTo.CC].AddRange(b.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
            emails[EmailSentTo.BCC].AddRange(c.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
        }

        return emails;
    }

    /// <summary>
    /// Return a user email, or return all user email addresses in a distribution list.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="email"></param>
    /// <param name="isVacationMode"></param>
    /// <param name="accountType"></param>
    /// <param name="sendTo"></param>
    /// <returns></returns>
    private async Task<(IEnumerable<UserEmail> to, IEnumerable<UserEmail> cc, IEnumerable<UserEmail> bcc)> GetEmailAddressesAsync(int userId, string email, UserAccountType accountType, EmailSentTo sendTo)
    {
        var to = new List<UserEmail>();
        var cc = new List<UserEmail>();
        var bcc = new List<UserEmail>();
        // If the user is a distribution list, fetch the rest of the users.
        if (accountType == UserAccountType.Distribution)
        {
            var users = await this.Api.GetDistributionListAsync(userId);
            var filteredUsers = users.Where(u => !u.IsVacationMode() && u.GetEmail().IsValidEmail()).ToList();
            var emails = filteredUsers.Select(u => new UserEmail(u.Id, u.GetEmail()));
            switch (sendTo)
            {
                case EmailSentTo.To:
                    to.AddRange(emails);
                    break;
                case EmailSentTo.CC:
                    cc.AddRange(emails);
                    break;
                case EmailSentTo.BCC:
                    bcc.AddRange(emails);
                    break;
            }
        }
        else if (email.IsValidEmail())
        {
            switch (sendTo)
            {
                case EmailSentTo.To:
                    to.Add(new UserEmail(userId, email));
                    break;
                case EmailSentTo.CC:
                    cc.Add(new UserEmail(userId, email));
                    break;
                case EmailSentTo.BCC:
                    bcc.Add(new UserEmail(userId, email));
                    break;
            }
        }
        else
        {
            this.Logger.LogError("Email address is invalid {email}", email);
        }

        return (to.Distinct(), cc.Distinct(), bcc.Distinct());
    }

    /// <summary>
    /// Send out an email for the specified report.
    /// Generate a report instance for this email.
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task GenerateAndSendReportAsync(ReportRequestModel request, API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel instance)
    {
        var model = new AVOverviewInstanceModel(instance);
        var template = instance.Template ?? throw new InvalidOperationException($"Report template was not included in model.");

        // No need to send an email if there are no subscribers.
        if (request.SendToSubscribers)
        {
            var userReportInstances = new List<API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel>();
            if (!this.Options.UseMailMerge && instance.Id > 0)
            {
                userReportInstances.AddRange(await this.Api.GetUserAVOverviewInstancesAsync(instance.Id));
            }

            var subscribers = instance.Subscribers.Where(s => s.IsSubscribed).ToArray();

            var to = new List<UserEmail>();
            var cc = new List<UserEmail>();
            var bcc = new List<UserEmail>();
            foreach (var user in subscribers)
            {
                // Determine which users have already received the email.
                var (a, b, c) = await GetEmailAddressesAsync(user.Id, user.GetEmail(), user.AccountType, user.SendTo);
                to.AddRange(a.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
                cc.AddRange(b.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
                bcc.AddRange(c.Where(s => request.Resend || !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
            }

            if (to.Any() || !String.IsNullOrEmpty(request.To))
            {
                var subject = await this.ReportEngine.GenerateReportSubjectAsync(template, model, false);
                var body = await this.ReportEngine.GenerateReportBodyAsync(template, model, false);

                // Send the email.
                var (status, response) = await SendEmailAsync(request, to, cc, bcc, subject, body, $"{instance.TemplateType}-{instance.Id}", async (users, status, response) =>
                {
                    if (instance.Id > 0)
                    {
                        var userReportInstances = users.Select((user) => new API.Areas.Services.Models.AVOverview.UserAVOverviewInstanceModel(user.UserId, instance.Id, DateTime.UtcNow, status, response));
                        await this.Api.AddOrUpdateUserAVOverviewInstancesAsync(userReportInstances);
                    }
                });

                // Update the report instance with the email response.
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
            }

            instance.IsPublished = true;
            await UpdateReportInstanceAsync(instance);
        }
    }

    /// <summary>
    /// Send an email to CHES.
    /// Also update user report instances to identify who has received an email.
    /// Handles sending email as a mail merge, or individually.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private async Task<(ReportStatus status, EmailResponseModel[] response)> SendEmailAsync(
        ReportRequestModel request,
        IEnumerable<UserEmail> to,
        IEnumerable<UserEmail> cc,
        IEnumerable<UserEmail> bcc,
        string subject,
        string body,
        string tag,
        Func<IEnumerable<UserEmail>, ReportStatus, JsonDocument, Task> updateCallbackAsync)
    {
        await HandleChesEmailOverrideAsync(request.RequestorId);

        var contexts = new List<(UserEmail User, EmailContextModel Context)>();
        if (!String.IsNullOrWhiteSpace(request.To) && request.To.IsValidEmail())
        {
            // Add a context for the requested list of users.
            var toAddresses = request.To.Split(",").Select(v => v.Trim()).ToArray();
            contexts.Add((new UserEmail(0, request.To), new EmailContextModel(toAddresses, new Dictionary<string, object>(), DateTime.Now)
            {
                Tag = tag,
            }));
        }
        else
        {
            var sendToRequestorSeparately = request.RequestorId.HasValue && (cc.Any() || bcc.Any());
            var requestor = sendToRequestorSeparately ? await this.Api.GetUserAsync(request.RequestorId!.Value) : null;
            var requestorEmail = !String.IsNullOrWhiteSpace(requestor?.PreferredEmail) ? requestor.PreferredEmail : requestor?.Email;

            // Do not send multiple emails to the requestor if there is also a CC/BCC list.
            var toAddresses = String.IsNullOrWhiteSpace(requestorEmail) ? to.Distinct() : to.Distinct().Where(v => v.To != requestorEmail);
            var toContexts = toAddresses
                .Where(v => v.To.IsValidEmail())
                .Select(v => (v, new EmailContextModel(new[] { v.To }, new Dictionary<string, object>(), DateTime.Now)
                {
                    Tag = tag,
                })).ToArray();
            contexts.AddRange(toContexts);

            // If there are BCC and CC options then they must be sent with an email that is sent to the requestor.
            if (sendToRequestorSeparately && !String.IsNullOrWhiteSpace(requestorEmail))
            {
                var cc1 = cc.Distinct().Where(a => a.To.IsValidEmail()).ToArray();
                var bcc1 = bcc.Distinct().Where(a => a.To.IsValidEmail()).ToArray();
                contexts.Add((new UserEmail(request.RequestorId!.Value, requestorEmail, cc1, bcc1), new EmailContextModel(new[] { requestorEmail }, new Dictionary<string, object>(), DateTime.Now)
                {
                    Cc = cc1.Select(v => v.To),
                    Bcc = bcc1.Select(v => v.To),
                    Tag = tag,
                }));
            }
        }

        if (!contexts.Any()) return (ReportStatus.Cancelled, Array.Empty<EmailResponseModel>());

        if (this.Options.UseMailMerge)
        {
            var merge = new EmailMergeModel(this.ChesOptions.From, contexts.Select(c => c.Context), subject, body)
            {
                // TODO: Extract values from report settings.
                Encoding = EmailEncodings.Utf8,
                BodyType = EmailBodyTypes.Html,
                Priority = EmailPriorities.Normal,
            };

            var failureCount = 0;
            while (true)
            {
                try
                {
                    var response = await this.Ches.SendEmailAsync(merge);
                    this.Logger.LogInformation("Report sent to CHES.  ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

                    if (request.ReportInstanceId.HasValue)
                    {
                        var document = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
                        await updateCallbackAsync(contexts.Select(c => c.User), ReportStatus.Accepted, document);
                    }

                    return (ReportStatus.Accepted, new[] { response });
                }
                catch (ChesException ex)
                {
                    // Retry X times before giving up.
                    failureCount++;
                    if (failureCount >= this.Options.RetryLimit)
                        throw;
                    else
                        this.Logger.LogError(ex, "Failed to send report to CHES. ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

                    // Wait before trying again.
                    await Task.Delay(this.Options.DefaultDelayMS);
                }
            }
        }
        else
        {
            var responses = new List<EmailResponseModel>();
            var failureCount = 0; // Keep track of each failed subscriber email.
            foreach (var (user, context) in contexts)
            {
                failureCount++;
                var retryCount = 0;
                var allUsers = new[] { user }.Concat(user.CC.Concat(user.BCC)).Distinct();
                var allEmails = String.Join(", ", allUsers.Select(u => u.To));
                while (true)
                {
                    try
                    {
                        var email = new EmailModel(this.ChesOptions.From, context.To.ToArray(), subject, body)
                        {
                            Cc = context.Cc.ToArray(),
                            Bcc = context.Bcc.ToArray(),
                            // TODO: Extract values from report settings.
                            Encoding = EmailEncodings.Utf8,
                            BodyType = EmailBodyTypes.Html,
                            Priority = EmailPriorities.Normal,
                        };

                        var response = await this.Ches.SendEmailAsync(email);
                        responses.Add(response);

                        failureCount--; // Success, remove from failure count.
                        if (user.UserId != 0)
                        {
                            // Save the status of each email sent.
                            var document = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
                            await updateCallbackAsync(allUsers, ReportStatus.Accepted, document);
                        }
                        break;
                    }
                    catch (ChesException ex)
                    {
                        if (user.UserId != 0)
                        {
                            // Save the status of each email sent.
                            var document = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));
                            await updateCallbackAsync(allUsers, ReportStatus.Failed, document);
                        }

                        retryCount++;
                        if (retryCount >= this.Options.RetryLimit)
                        {
                            // Escape from retry loop.
                            if (!this.Options.SendToAllSubscribersBeforeFailing)
                                throw;
                            break;
                        }
                        else
                            this.Logger.LogError(ex, "Failed to send report to CHES. ReportId:{report}, InstanceId:{instance}, Emails:{emails}", request.ReportId, request.ReportInstanceId, allEmails);

                        // Wait before trying again.
                        await Task.Delay(this.Options.RetryDelayMS);
                    }
                    catch (Exception ex)
                    {
                        if (user.UserId != 0)
                        {
                            // Save the status of each email sent.
                            var document = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = ex.GetAllMessages() }, _serializationOptions));
                            await updateCallbackAsync(allUsers, ReportStatus.Failed, document);
                        }

                        retryCount++;
                        if (retryCount >= this.Options.RetryLimit)
                        {
                            // Escape from retry loop.
                            if (!this.Options.SendToAllSubscribersBeforeFailing)
                                throw;
                            break;
                        }
                        else
                            this.Logger.LogError(ex, "Failed to send report to CHES. ReportId:{report}, InstanceId:{instance}, Emails:{emails}", request.ReportId, request.ReportInstanceId, allEmails);

                        // Wait before trying again.
                        await Task.Delay(this.Options.RetryDelayMS);
                    }
                }
            }
            this.Logger.LogInformation("Report sent to CHES. ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

            return (failureCount > 0 ? ReportStatus.Failed : ReportStatus.Accepted, responses.ToArray());
        }
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
            if (user != null) email = user.GetEmail();
        }
        var identity = _user.Identity as ClaimsIdentity ?? throw new ConfigurationException("CHES requires an active ClaimsPrincipal");
        identity.RemoveClaim(_user.FindFirst(ClaimTypes.Email));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
    }
    #endregion
}
