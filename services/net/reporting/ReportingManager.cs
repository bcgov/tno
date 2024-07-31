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
                this.Logger.LogDebug("The service has failed: '{Status}'", this.State.Status);
                if (this.Options.AutoRestartAfterFailure)
                {
                    await Task.Delay(this.Options.RetryAfterFailedDelayMS);
                    this.State.Resume();
                    this.Listener.Resume();
                }
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
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
                    await this.SendEmailAsync("Service had an Unexpected Failure", ex);
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

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
            }
        }
        catch (ReportingException ex)
        {
            // Failures will require retrying the report.  However, depending on the error the report instance could be in a unique state.
            // Resending a failure can result in cascading failures however, so it should limit the number of attempts.
            // TODO: Limit number of retries.
            this.Logger.LogError(ex, "Failed to handle message");
            await this.SendEmailAsync("Failed to handle message", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            await this.Api.SendMessageAsync(result.Message.Value);
        }
        catch (HttpClientRequestException ex)
        {
            this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
            await this.SendEmailAsync("HTTP exception while consuming. {response}", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            await this.Api.SendMessageAsync(result.Message.Value);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Failed to handle message");
            await this.SendEmailAsync("Failed to handle message", ex);
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            await this.Api.SendMessageAsync(result.Message.Value);
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
                        await GenerateReportAsync(request, instance);
                    }
                    else
                        this.Logger.LogWarning("Report instance does not exist.  Report Instance: {id}", request.ReportInstanceId);
                }
                else
                {
                    var report = await this.Api.GetReportAsync(request.ReportId);
                    if (report != null)
                    {
                        await GenerateReportAsync(request, report);
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
                    await GenerateReportAsync(request, instance);
                }
                else
                    this.Logger.LogWarning("AV overview instance does not exist.  Instance: {id}", request.ReportInstanceId);
            }
            else throw new NotImplementedException($"Report template type '{request.ReportType.GetName()}' has not been implemented");
        }

        // If the request originated from the scheduler service, update the last ran one.
        if (request.EventScheduleId.HasValue)
        {
            var scheduledEvent = await this.Api.GetEventScheduleAsync(request.EventScheduleId.Value) ?? throw new NoContentException($"Event schedule '{request.EventScheduleId}' does not exist.");
            scheduledEvent.LastRanOn = DateTime.UtcNow;
            await this.Api.UpdateEventScheduleAsync(scheduledEvent);
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
            throw new ReportingException(ReportingErrors.FailedToGetInstance, $"Failed to fetch current report instance. ReportId:{report.Id}", ex);
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
            throw new ReportingException(ReportingErrors.FailedToGetContent, $"Failed to fetch content for report. ReportId:{report.Id}", ex);
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
                throw new ReportingException(ReportingErrors.FailedToAddInstance, $"Failed to add instance to report. ReportId:{report.Id}", ex);
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
                throw new ReportingException(ReportingErrors.FailedToUpdateInstance, $"Failed to updated instance for report. ReportId:{report.Id}, InstanceId:{instanceModel?.Id}", ex);
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
    private async Task GenerateReportAsync(ReportRequestModel request, API.Areas.Services.Models.Report.ReportModel report)
    {
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instanceModel = null;
        Dictionary<string, ReportSectionModel>? sectionContent = null;
        ReportingException? failure = null;
        try
        {
            (instanceModel, sectionContent) = await PopulateReportInstance(request, report);
        }
        catch (ReportingException ex)
        {
            // A failure has resulted in the report in an failed and possibly incomplete state.
            // If the the report instance exists and has an ID it means it was saved successfully in the DB but was not able to send.
            failure = ex;
        }

        if (failure == null && sectionContent != null)
        {
            var linkOnlyFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && LinkOnlyFormats.Contains(s.Format)).ToArray();
            var fullTextFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && FullTextFormats.Contains(s.Format)).ToArray();
            string subject = "", linkOnlyFormatBody = "", fullTextFormatBody = "";
            try
            {
                subject = await this.ReportEngine.GenerateReportSubjectAsync(report, instanceModel, sectionContent, false, false);

                // Generate and send report to subscribers who want an email with a link to the website.
                // We do this first because we don't want to save the output of this in the instance.
                linkOnlyFormatBody = linkOnlyFormatSubscribers.Any() ? await this.ReportEngine.GenerateReportBodyAsync(report, instanceModel, sectionContent, GetLinkedReportAsync, null, true, false) : "";

                // Generate and send report to subscribers who want an email format.
                fullTextFormatBody = await this.ReportEngine.GenerateReportBodyAsync(report, instanceModel, sectionContent, GetLinkedReportAsync, null, false, false);
            }
            catch (Exception ex)
            {
                if (instanceModel != null)
                {
                    instanceModel.Status = ReportStatus.Failed;
                    instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = ex.GetAllMessages() }, _serializationOptions));
                }

                failure = new ReportingException(ReportingErrors.FailedToGenerateOutput, $"Failed to generate output for report.  ReportId:{report.Id}, InstanceId:{instanceModel?.Id}", ex);
            }

            if (failure == null)
            {
                var userReportInstances = new List<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
                if (!this.Options.UseMailMerge && instanceModel?.Id > 0)
                {
                    // Get users who have either received the email, or failed.
                    userReportInstances.AddRange(await this.Api.GetUserReportInstancesAsync(instanceModel.Id));
                }

                var linkOnlyFormatTo = new List<UserEmail>();
                var linkOnlyFormatCC = new List<UserEmail>();
                var linkOnlyFormatBCC = new List<UserEmail>();
                foreach (var user in linkOnlyFormatSubscribers)
                {
                    // Only include users who have not received an email yet.
                    var (a, b, c) = await GetEmailAddressesAsync(user.UserId, user.User!.GetEmail(), user.User!.AccountType, user.SendTo);
                    linkOnlyFormatTo.AddRange(a.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
                    linkOnlyFormatCC.AddRange(b.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
                    linkOnlyFormatBCC.AddRange(c.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
                }

                var fullTextFormatTo = new List<UserEmail>();
                var fullTextFormatCC = new List<UserEmail>();
                var fullTextFormatBCC = new List<UserEmail>();
                foreach (var user in fullTextFormatSubscribers)
                {
                    // Only include users who have not received an email yet.
                    var (a, b, c) = await GetEmailAddressesAsync(user.UserId, user.User!.GetEmail(), user.User!.AccountType, user.SendTo);
                    fullTextFormatTo.AddRange(a.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
                    fullTextFormatCC.AddRange(b.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
                    fullTextFormatBCC.AddRange(c.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
                }

                if (request.SendToSubscribers || !String.IsNullOrEmpty(request.To))
                {
                    // TODO: This implementation can result in one set of emails being successful and the second failing.
                    var responseModel = new ReportEmailResponseModel();
                    try
                    {
                        if (!report.Settings.DoNotSendEmail && String.IsNullOrEmpty(request.To) && (linkOnlyFormatTo.Any() || linkOnlyFormatCC.Any() || linkOnlyFormatBCC.Any()))
                        {
                            // Send the email.
                            var responseLinkOnly = await SendEmailAsync(request, linkOnlyFormatTo, linkOnlyFormatCC, linkOnlyFormatBCC, subject, linkOnlyFormatBody, $"{report.Name}-{report.Id}-linkOnly",
                            async (users, status, response) =>
                            {
                                if (instanceModel?.Id > 0)
                                {
                                    var userReportInstances = users.Select((user) => new API.Areas.Services.Models.ReportInstance.UserReportInstanceModel(user.UserId, instanceModel.Id)
                                    {
                                        LinkStatus = status,
                                        LinkSentOn = DateTime.UtcNow,
                                        LinkResponse = response,
                                    });
                                    await this.Api.AddOrUpdateUserReportInstancesAsync(userReportInstances);
                                }
                            });
                            responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseLinkOnly, _serializationOptions));

                            if (instanceModel != null)
                            {
                                instanceModel.Status = ReportStatus.Accepted;
                                instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                            }
                        }

                        if (!report.Settings.DoNotSendEmail && (fullTextFormatTo.Any() || fullTextFormatCC.Any() || fullTextFormatBCC.Any() || !String.IsNullOrEmpty(request.To)))
                        {
                            // Send the email.
                            var responseFullText = await SendEmailAsync(request, fullTextFormatTo, fullTextFormatCC, fullTextFormatBCC, subject, fullTextFormatBody, $"{report.Name}-{report.Id}",
                            async (users, status, response) =>
                            {
                                if (instanceModel?.Id > 0)
                                {
                                    var userReportInstances = users.Select((user) => new API.Areas.Services.Models.ReportInstance.UserReportInstanceModel(user.UserId, instanceModel.Id)
                                    {
                                        TextStatus = status,
                                        TextSentOn = DateTime.UtcNow,
                                        TextResponse = response,
                                    });
                                    await this.Api.AddOrUpdateUserReportInstancesAsync(userReportInstances);
                                }
                            });
                            responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseFullText, _serializationOptions));

                            if (instanceModel != null)
                            {
                                instanceModel.Status = ReportStatus.Accepted;
                                instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                            }
                        }
                    }
                    catch (ChesException ex)
                    {
                        this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instanceModel?.ReportId, instanceModel?.Id);
                        if (responseModel.LinkOnlyFormatResponse != null)
                            responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));
                        else
                            responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));

                        if (instanceModel != null)
                        {
                            instanceModel.Status = ReportStatus.Failed;
                            instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                        }
                        failure = new ReportingException(ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instanceModel?.Id}", ex);
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instanceModel?.ReportId, instanceModel?.Id);
                        if (instanceModel != null)
                        {
                            instanceModel.Status = ReportStatus.Failed;
                            instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                        }
                        failure = new ReportingException(ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instanceModel?.Id}", ex);
                    }

                    if (instanceModel != null && request.GenerateInstance)
                    {
                        instanceModel.Subject = subject;
                        instanceModel.Body = fullTextFormatBody;
                        if (request.SendToSubscribers)
                            instanceModel.SentOn = DateTime.UtcNow; // We track when it was sent, even if it failed.
                        if (instanceModel.PublishedOn == null) instanceModel.PublishedOn = DateTime.UtcNow;
                    }
                }
            }
        }

        if (instanceModel != null && request.GenerateInstance)
        {
            await UpdateReportInstanceAsync(instanceModel, true);
        }

        // Only clear folders if the instance was successfully saved.
        if (request.GenerateInstance && report.Settings.Content.ClearFolders && request.SendToSubscribers &&
            (failure == null ||
            failure.Error != ReportingErrors.FailedToGetInstance &&
            failure.Error != ReportingErrors.FailedToGetContent))
        {
            await this.Api.ClearFoldersInReport(report.Id);
        }

        if (failure != null)
        {
            // Set the report instance ID so that retries will be able to continue successfully.
            request.ReportInstanceId = instanceModel?.Id;
            request.Data = new { failure.Error };
            throw failure;
        }
    }

    /// <summary>
    /// Send out an email for the specified report instance.
    /// Send an email merge to CHES.
    /// This will send out a separate email to each context provided.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task GenerateReportAsync(ReportRequestModel request, API.Areas.Services.Models.ReportInstance.ReportInstanceModel instance)
    {
        ReportingException? failure = null;
        var resending = instance.SentOn.HasValue && !String.IsNullOrWhiteSpace(instance.Subject) && !String.IsNullOrWhiteSpace(instance.Body);
        var report = instance.Report ?? throw new ArgumentException("Report instance must include the report model.");
        var sections = report.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s));

        var searchResults = !resending ? await this.Api.GetContentForReportInstanceIdAsync(instance.Id) : Array.Empty<API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel>();
        var sectionContent = sections.ToDictionary(s => s.Name, section =>
        {
            section.Content = searchResults.Where(sr => sr.SectionName == section.Name && sr.Content != null).Select(ri => new ContentModel(ri.Content!, ri.SortOrder)).ToArray();
            return section;
        });

        if (!resending)
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

        var linkOnlyFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && LinkOnlyFormats.Contains(s.Format)).ToArray();
        var fullTextFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && FullTextFormats.Contains(s.Format)).ToArray();
        string subject = "", linkOnlyFormatBody = "", fullTextFormatBody = "";

        try
        {
            this.Logger.LogDebug("Report is generating body. ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance.Id);
            subject = !resending ? await this.ReportEngine.GenerateReportSubjectAsync(instance.Report, instance, sectionContent, false, false) : instance.Subject;

            // Generate and send report to subscribers who want an email with a link to the website.
            // We do this first because we don't want to save the output of this in the instance.
            linkOnlyFormatBody = linkOnlyFormatSubscribers.Any() ? await this.ReportEngine.GenerateReportBodyAsync(instance.Report, instance, sectionContent, GetLinkedReportAsync, null, true, false) : "";

            // Generate and send report to subscribers who want an email format.
            fullTextFormatBody = !resending ? await this.ReportEngine.GenerateReportBodyAsync(instance.Report, instance, sectionContent, GetLinkedReportAsync, null, false, false) : instance.Body;
        }
        catch (Exception ex)
        {
            instance.Status = ReportStatus.Failed;
            instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = ex.GetAllMessages() }, _serializationOptions));
            failure = new ReportingException(ReportingErrors.FailedToGenerateOutput, $"Failed to generate output for report.  ReportId:{report.Id}, InstanceId:{instance.Id}", ex);
            this.Logger.LogError(ex, "Failed to generate output for report.  ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance.Id);
        }

        if (failure == null)
        {
            var userReportInstances = new List<API.Areas.Services.Models.ReportInstance.UserReportInstanceModel>();
            if (!this.Options.UseMailMerge && instance.Id > 0)
            {
                // Get users who have either received the email, or failed.
                userReportInstances.AddRange(await this.Api.GetUserReportInstancesAsync(instance.Id));
            }

            this.Logger.LogDebug("Report is generating subscriber lists. ReportId:{reportId}, InstanceId:{instanceId}, SendToSubscribers:{send}", report.Id, instance.Id, request.SendToSubscribers);
            var linkOnlyFormatTo = new List<UserEmail>();
            var linkOnlyFormatCC = new List<UserEmail>();
            var linkOnlyFormatBCC = new List<UserEmail>();
            foreach (var user in linkOnlyFormatSubscribers)
            {
                // Only include users who have not received an email yet.
                var (a, b, c) = await GetEmailAddressesAsync(user.UserId, user.User!.GetEmail(), user.User!.AccountType, user.SendTo);
                linkOnlyFormatTo.AddRange(a.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
                linkOnlyFormatCC.AddRange(b.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
                linkOnlyFormatBCC.AddRange(c.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.LinkStatus))));
            }

            var fullTextFormatTo = new List<UserEmail>();
            var fullTextFormatCC = new List<UserEmail>();
            var fullTextFormatBCC = new List<UserEmail>();
            foreach (var user in fullTextFormatSubscribers)
            {
                // Only include users who have not received an email yet.
                var (a, b, c) = await GetEmailAddressesAsync(user.UserId, user.User!.GetEmail(), user.User!.AccountType, user.SendTo);
                fullTextFormatTo.AddRange(a.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
                fullTextFormatCC.AddRange(b.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
                fullTextFormatBCC.AddRange(c.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.TextStatus))));
            }

            if (request.SendToSubscribers || !String.IsNullOrWhiteSpace(request.To))
            {
                // TODO: This implementation can result in one set of emails being successful and the second failing.
                var responseModel = new ReportEmailResponseModel();
                try
                {
                    if (!report.Settings.DoNotSendEmail && request.SendToSubscribers && (linkOnlyFormatTo.Any() || linkOnlyFormatCC.Any() || linkOnlyFormatBCC.Any()))
                    {
                        this.Logger.LogDebug("Report is sending link only email. ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance.Id);
                        // Send the email.
                        var responseLinkOnly = await SendEmailAsync(request, linkOnlyFormatTo, linkOnlyFormatCC, linkOnlyFormatBCC, subject, linkOnlyFormatBody, $"{report.Name}-{report.Id}-linkOnly",
                            async (users, status, response) =>
                            {
                                if (instance.Id > 0)
                                {
                                    var userReportInstances = users.Select((user) => new API.Areas.Services.Models.ReportInstance.UserReportInstanceModel(user.UserId, instance.Id)
                                    {
                                        LinkStatus = status,
                                        LinkSentOn = DateTime.UtcNow,
                                        LinkResponse = response,
                                    });
                                    await this.Api.AddOrUpdateUserReportInstancesAsync(userReportInstances);
                                }
                            });
                        responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseLinkOnly, _serializationOptions));
                        instance.Status = ReportStatus.Accepted;
                        instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                    }

                    if (!report.Settings.DoNotSendEmail && (fullTextFormatTo.Any() || fullTextFormatCC.Any() || fullTextFormatBCC.Any() || !String.IsNullOrEmpty(request.To)))
                    {
                        this.Logger.LogDebug("Report is sending full text email. ReportId:{reportId}, InstanceId:{instanceId}", report.Id, instance.Id);
                        // Send the email.
                        var responseFullText = await SendEmailAsync(request, fullTextFormatTo, fullTextFormatCC, fullTextFormatBCC, subject, fullTextFormatBody, $"{report.Name}-{report.Id}",
                            async (users, status, response) =>
                            {
                                if (instance.Id > 0)
                                {
                                    var userReportInstances = users.Select((user) => new API.Areas.Services.Models.ReportInstance.UserReportInstanceModel(user.UserId, instance.Id)
                                    {
                                        TextStatus = status,
                                        TextSentOn = DateTime.UtcNow,
                                        TextResponse = response,
                                    });
                                    await this.Api.AddOrUpdateUserReportInstancesAsync(userReportInstances);
                                }
                            });
                        responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(responseFullText, _serializationOptions));
                        instance.Status = ReportStatus.Accepted;
                        instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                    }

                }
                catch (ChesException ex)
                {
                    this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instance.ReportId, instance.Id);
                    if (responseModel.LinkOnlyFormatResponse != null)
                        responseModel.LinkOnlyFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));
                    else
                        responseModel.FullTextFormatResponse = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));

                    instance.Status = ReportStatus.Failed;
                    instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                    failure = new ReportingException(ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instance.Id}", ex);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instance.ReportId, instance.Id);
                    instance.Status = ReportStatus.Failed;
                    instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                    failure = new ReportingException(ReportingErrors.FailedToEmail, $"Failed to email report.  ReportId:{report.Id}, InstanceId:{instance.Id}", ex);
                }
            }

            if (request.GenerateInstance)
            {
                instance.Subject = subject;
                instance.Body = fullTextFormatBody;
                if (instance.PublishedOn == null) instance.PublishedOn = DateTime.UtcNow;
                if (request.SendToSubscribers)
                    instance.SentOn = DateTime.UtcNow;

                await UpdateReportInstanceAsync(instance, false);
            }
        }

        if (report.Settings.Content.ClearFolders && request.SendToSubscribers &&
            (failure == null ||
            failure.Error != ReportingErrors.FailedToGetInstance &&
            failure.Error != ReportingErrors.FailedToGetContent))
        {
            // Make a request to clear content from folders in this report.
            await this.Api.ClearFoldersInReport(report.Id);
        }

        if (failure != null)
        {
            request.Data = new { failure.Error };
            throw failure;
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
        // TODO: limit number of attempts.
        var isUpdated = false;
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
                this.Logger.LogError(ex, "Failed to update report instance. ReportId:{reportId} InstanceId:{instanceId}", instance.ReportId, instance.Id);
            }
        }
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
        // TODO: limit number of attempts.
        var isUpdated = false;
        while (!isUpdated)
        {
            try
            {
                // We're getting optimistic errors here, most likely due to changes occurring from the UI.  Since we only need to change values related to status, we can get the latest and reapply.
                var latestInstanceModel = await this.Api.GetAVOverviewInstanceAsync(instance.Id) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
                if (latestInstanceModel.Version != instance.Version)
                {
                    // latestInstanceModel.Subject = instance.Subject;
                    // latestInstanceModel.Body = instance.Body;
                    // latestInstanceModel.SentOn = instance.SentOn;
                    // latestInstanceModel.Status = instance.Status;
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
                this.Logger.LogError(ex, "Failed to update AV overview report instance. InstanceId:{instanceId}", instance.Id);
            }
        }
    }

    /// <summary>
    /// Get users in distribution lists.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="email"></param>
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
            var emails = users.Select(u => new UserEmail(u.Id, u.GetEmail()));
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
        else
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

        return (to, cc, bcc);
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
    private async Task GenerateReportAsync(ReportRequestModel request, API.Areas.Services.Models.AVOverview.AVOverviewInstanceModel instance)
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
                to.AddRange(a.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
                cc.AddRange(b.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
                bcc.AddRange(c.Where(s => !userReportInstances.Any(uri => uri.UserId == s.UserId && _successfulEmailStatuses.Contains(uri.Status))));
            }

            if (to.Any() || !String.IsNullOrEmpty(request.To))
            {
                var subject = await this.ReportEngine.GenerateReportSubjectAsync(template, model, false);
                var body = await this.ReportEngine.GenerateReportBodyAsync(template, model, false);

                // Send the email.
                var response = await SendEmailAsync(request, to, cc, bcc, subject, body, $"{instance.TemplateType}-{instance.Id}", async (users, status, response) =>
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
    private async Task<EmailResponseModel[]> SendEmailAsync(
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

        if (!contexts.Any()) return Array.Empty<EmailResponseModel>();

        if (this.Options.UseMailMerge)
        {
            var merge = new EmailMergeModel(this.ChesOptions.From, contexts.Select(c => c.Context), subject, body)
            {
                // TODO: Extract values from report settings.
                Encoding = EmailEncodings.Utf8,
                BodyType = EmailBodyTypes.Html,
                Priority = EmailPriorities.Normal,
            };

            var response = await this.Ches.SendEmailAsync(merge);
            this.Logger.LogInformation("Report sent to CHES.  ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

            if (request.ReportInstanceId.HasValue)
            {
                var document = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
                await updateCallbackAsync(contexts.Select(c => c.User), ReportStatus.Accepted, document);
            }

            return new[] { response };
        }
        else
        {
            var responses = new List<EmailResponseModel>();
            foreach (var (user, context) in contexts)
            {
                var allUsers = new[] { user }.Concat(user.CC.Concat(user.BCC)).Distinct();
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

                    if (user.UserId != 0)
                    {
                        // Save the status of each email sent.
                        var document = JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions));
                        await updateCallbackAsync(allUsers, ReportStatus.Accepted, document);
                    }
                }
                catch (ChesException ex)
                {
                    if (user.UserId != 0)
                    {
                        // Save the status of each email sent.
                        var document = JsonDocument.Parse(JsonSerializer.Serialize(ex.Data["error"], _serializationOptions));
                        await updateCallbackAsync(allUsers, ReportStatus.Failed, document);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    if (user.UserId != 0)
                    {
                        // Save the status of each email sent.
                        var document = JsonDocument.Parse(JsonSerializer.Serialize(new { Error = ex.GetAllMessages() }, _serializationOptions));
                        await updateCallbackAsync(allUsers, ReportStatus.Failed, document);
                    }
                    throw;
                }
            }
            this.Logger.LogInformation("Report sent to CHES. ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

            return responses.ToArray();
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
