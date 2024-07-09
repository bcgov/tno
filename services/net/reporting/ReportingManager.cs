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
    static ReportDistributionFormat[] LinkOnlyFormats = new[] { ReportDistributionFormat.LinkOnly, ReportDistributionFormat.ReceiveBoth };
    static ReportDistributionFormat[] FullTextFormats = new[] { ReportDistributionFormat.FullText, ReportDistributionFormat.ReceiveBoth };
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
        catch (Exception ex)
        {
            if (ex is HttpClientRequestException httpEx)
            {
                this.Logger.LogError(ex, "HTTP exception while consuming. {response}", httpEx.Data["body"] ?? "");
                await this.SendEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
                await this.SendEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
        finally
        {
            if (State.Status == ServiceStatus.Running)
            {
                // Inform Kafka this message is completed.
                // We do this regardless of whether it failed or succeeded.
                this.Listener.Commit(result);
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
                        this.Logger.LogWarning("Report instance does not exist.  Report Instance: {instance}", request.ReportInstanceId);
                }
                else
                {
                    var report = await this.Api.GetReportAsync(request.ReportId);
                    if (report != null)
                    {
                        await GenerateReportAsync(request, report);
                    }
                    else
                        this.Logger.LogWarning("Report does not exist.  Report: {report}", request.ReportId);
                }
            }
            else if (request.ReportType == Entities.ReportType.AVOverview)
            {
                var instance = await this.Api.GetAVOverviewInstanceAsync(request.ReportId);
                if (instance != null)
                {
                    await GenerateReportAsync(request, instance);
                }
                else
                    this.Logger.LogWarning("AV overview instance does not exist.  Instance: {report}", request.ReportId);
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
        API.Areas.Services.Models.ReportInstance.ReportInstanceModel? instanceModel;
        var sections = report.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s));
        var sectionContent = new Dictionary<string, ReportSectionModel>();
        var reportInstanceModel = await this.Api.GetCurrentReportInstanceAsync(report.Id, request.RequestorId);

        // If we don't clear the prior report we must append new content to it.
        var currentSectionContent = new Dictionary<string, ReportSectionModel>();
        if (reportInstanceModel != null && !report.Settings.Content.ClearOnStartNewReport)
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

        if (reportInstanceModel == null || reportInstanceModel.SentOn.HasValue)
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
                ?? throw new InvalidOperationException("Report instance failed to be returned by API")) : null;
        }
        else
        {
            var instanceContent = sectionContent.SelectMany(s => s.Value.Content.Select(c => new API.Areas.Services.Models.ReportInstance.ReportInstanceContentModel(reportInstanceModel.Id, c.Id, s.Key, c.SortOrder)));
            instanceModel = new API.Areas.Services.Models.ReportInstance.ReportInstanceModel((Entities.ReportInstance)reportInstanceModel, _serializationOptions)
            {
                OwnerId = request.RequestorId ?? report.OwnerId,
                PublishedOn = DateTime.UtcNow,
                Content = instanceContent,
            };
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
        (var instanceModel, var sectionContent) = await PopulateReportInstance(request, report);

        var subject = await this.ReportEngine.GenerateReportSubjectAsync(report, instanceModel, sectionContent, false, false);

        // Generate and send report to subscribers who want an email with a link to the website.
        // We do this first because we don't want to save the output of this in the instance.
        var linkOnlyFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && LinkOnlyFormats.Contains(s.Format)).ToArray();
        var linkOnlyFormatBody = linkOnlyFormatSubscribers.Any() ? await this.ReportEngine.GenerateReportBodyAsync(report, instanceModel, sectionContent, GetLinkedReportAsync, null, true, false) : "";
        var linkOnlyFormatTo = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.To).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var linkOnlyFormatCC = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.CC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var linkOnlyFormatBCC = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.BCC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();

        // Generate and send report to subscribers who want an email format.
        var fullTextFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && FullTextFormats.Contains(s.Format)).ToArray();
        var fullTextFormatBody = await this.ReportEngine.GenerateReportBodyAsync(report, instanceModel, sectionContent, GetLinkedReportAsync, null, false, false);
        var fullTextFormatTo = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.To).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var fullTextFormatCC = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.CC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var fullTextFormatBCC = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.BCC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();

        if (request.SendToSubscribers || !String.IsNullOrEmpty(request.To))
        {
            // TODO: This implementation can result in one set of emails being successful and the second failing.
            var responseModel = new ReportEmailResponseModel();
            try
            {
                if (!report.Settings.DoNotSendEmail && String.IsNullOrEmpty(request.To) && (linkOnlyFormatTo.Any() || linkOnlyFormatCC.Any() || linkOnlyFormatBCC.Any()))
                {
                    // Send the email.
                    var responseLinkOnly = await SendEmailAsync(request, linkOnlyFormatTo, linkOnlyFormatCC, linkOnlyFormatBCC, subject, linkOnlyFormatBody, $"{report.Name}-{report.Id}-linkOnly");
                    responseModel.LinkOnlyFormatResponse = responseLinkOnly != null ? JsonDocument.Parse(JsonSerializer.Serialize(responseLinkOnly, _serializationOptions)) : JsonDocument.Parse("{}");
                }

                if (!report.Settings.DoNotSendEmail && (fullTextFormatTo.Any() || fullTextFormatCC.Any() || fullTextFormatBCC.Any() || !String.IsNullOrEmpty(request.To)))
                {
                    // Send the email.
                    var responseFullText = await SendEmailAsync(request, fullTextFormatTo, fullTextFormatCC, fullTextFormatBCC, subject, fullTextFormatBody, $"{report.Name}-{report.Id}");
                    responseModel.FullTextFormatResponse = responseFullText != null ? JsonDocument.Parse(JsonSerializer.Serialize(responseFullText, _serializationOptions)) : JsonDocument.Parse("{}");
                }

                if (instanceModel != null)
                {
                    instanceModel.Status = ReportStatus.Accepted;
                    instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
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
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instanceModel?.ReportId, instanceModel?.Id);
                if (instanceModel != null)
                {
                    instanceModel.Status = ReportStatus.Failed;
                    instanceModel.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
                }
            }

            if (instanceModel != null && request.GenerateInstance)
            {
                instanceModel.Subject = subject;
                instanceModel.Body = fullTextFormatBody;
                // Update the report instance.
                if (request.SendToSubscribers)
                    instanceModel.SentOn = instanceModel.Status == ReportStatus.Accepted ? DateTime.UtcNow : null;
                if (instanceModel.PublishedOn == null) instanceModel.PublishedOn = DateTime.UtcNow;
            }
        }

        if (instanceModel != null && request.GenerateInstance)
        {
            // We're getting optimistic errors here, most likely due to changes occurring from the UI.  Since we only need to change values related to status, we can get the latest and reapply.
            var latestInstanceModel = await this.Api.GetReportInstanceAsync(instanceModel.Id) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
            if (latestInstanceModel.Version != instanceModel.Version)
            {
                latestInstanceModel.Subject = instanceModel.Subject;
                latestInstanceModel.Body = instanceModel.Body;
                latestInstanceModel.SentOn = instanceModel.SentOn;
                latestInstanceModel.Status = instanceModel.Status;
                instanceModel = await this.Api.UpdateReportInstanceAsync(latestInstanceModel) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
            }
            else
                instanceModel = await this.Api.UpdateReportInstanceAsync(instanceModel) ?? throw new InvalidOperationException("Report instance failed to be returned by API");
        }

        if (request.GenerateInstance && report.Settings.Content.ClearFolders && request.SendToSubscribers)
        {
            // Make a request to clear content from folders in this report.
            await this.Api.ClearFoldersInReport(report.Id);
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
        var resending = instance.SentOn.HasValue && !String.IsNullOrWhiteSpace(instance.Subject) && !String.IsNullOrWhiteSpace(instance.Body);
        // TODO: Control when a report is sent through configuration.
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

        var subject = !resending ? await this.ReportEngine.GenerateReportSubjectAsync(instance.Report, instance, sectionContent, false, false) : instance.Subject;

        // Generate and send report to subscribers who want an email with a link to the website.
        // We do this first because we don't want to save the output of this in the instance.
        var linkOnlyFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && LinkOnlyFormats.Contains(s.Format)).ToArray();
        var linkOnlyFormatTo = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.To).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var linkOnlyFormatCC = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.CC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var linkOnlyFormatBCC = linkOnlyFormatSubscribers.Where(s => s.SendTo == EmailSentTo.BCC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var linkOnlyFormatBody = linkOnlyFormatTo.Any() ? await this.ReportEngine.GenerateReportBodyAsync(instance.Report, instance, sectionContent, GetLinkedReportAsync, null, true, false) : "";

        // Generate and send report to subscribers who want an email format.
        var fullTextFormatSubscribers = report.Subscribers.Where(s => s.IsSubscribed && FullTextFormats.Contains(s.Format)).ToArray();
        var fullTextFormatTo = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.To).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var fullTextFormatCC = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.CC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var fullTextFormatBCC = fullTextFormatSubscribers.Where(s => s.SendTo == EmailSentTo.BCC).SelectMany(s => s.User!.GetEmails()).Distinct().ToArray();
        var fullTextFormatBody = !resending ? await this.ReportEngine.GenerateReportBodyAsync(instance.Report, instance, sectionContent, GetLinkedReportAsync, null, false, false) : instance.Body;

        if (request.SendToSubscribers || !String.IsNullOrWhiteSpace(request.To))
        {
            // TODO: This implementation can result in one set of emails being successful and the second failing.
            var responseModel = new ReportEmailResponseModel();
            try
            {
                if (!report.Settings.DoNotSendEmail && request.SendToSubscribers && (linkOnlyFormatTo.Any() || linkOnlyFormatCC.Any() || linkOnlyFormatBCC.Any()))
                {
                    // Send the email.
                    var responseLinkOnly = await SendEmailAsync(request, linkOnlyFormatTo, linkOnlyFormatCC, linkOnlyFormatBCC, subject, linkOnlyFormatBody, $"{report.Name}-{report.Id}-linkOnly");
                    responseModel.LinkOnlyFormatResponse = responseLinkOnly != null ? JsonDocument.Parse(JsonSerializer.Serialize(responseLinkOnly, _serializationOptions)) : JsonDocument.Parse("{}");
                }

                if (!report.Settings.DoNotSendEmail && (fullTextFormatTo.Any() || fullTextFormatCC.Any() || fullTextFormatBCC.Any() || !String.IsNullOrEmpty(request.To)))
                {
                    // Send the email.
                    var responseFullText = await SendEmailAsync(request, fullTextFormatTo, fullTextFormatCC, fullTextFormatBCC, subject, fullTextFormatBody, $"{report.Name}-{report.Id}");
                    responseModel.FullTextFormatResponse = responseFullText != null ? JsonDocument.Parse(JsonSerializer.Serialize(responseFullText, _serializationOptions)) : JsonDocument.Parse("{}");

                }

                instance.Status = ReportStatus.Accepted;
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
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
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to send report ID: {id}, InstanceId: {instance}", instance.ReportId, instance.Id);
                instance.Status = ReportStatus.Failed;
                instance.Response = JsonDocument.Parse(JsonSerializer.Serialize(responseModel, _serializationOptions));
            }

            if (request.GenerateInstance && !resending)
            {
                // Update the report instance.
                instance.Subject = subject;
                instance.Body = fullTextFormatBody;
                if (request.SendToSubscribers)
                    instance.SentOn = instance.Status == ReportStatus.Accepted ? DateTime.UtcNow : null;
                instance.Content = searchResults;
                if (instance.PublishedOn == null) instance.PublishedOn = DateTime.UtcNow;
                await this.Api.UpdateReportInstanceAsync(instance);
            }
        }

        if (report.Settings.Content.ClearFolders && request.SendToSubscribers && !resending)
        {
            // Make a request to clear content from folders in this report.
            await this.Api.ClearFoldersInReport(report.Id);
        }
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

        var subscribers = instance.Subscribers.Where(s => s.IsSubscribed).ToArray();
        var to = subscribers.Where(s => s.SendTo == EmailSentTo.To).SelectMany(s => s.GetEmails()).Distinct().ToArray();
        var cc = subscribers.Where(s => s.SendTo == EmailSentTo.CC).SelectMany(s => s.GetEmails()).Distinct().ToArray();
        var bcc = subscribers.Where(s => s.SendTo == EmailSentTo.BCC).SelectMany(s => s.GetEmails()).Distinct().ToArray();

        // No need to send an email if there are no subscribers.
        if (request.SendToSubscribers)
        {
            if (to.Length > 0 || !String.IsNullOrEmpty(request.To))
            {
                var subject = await this.ReportEngine.GenerateReportSubjectAsync(template, model, false);
                var body = await this.ReportEngine.GenerateReportBodyAsync(template, model, false);

                // Send the email.
                var response = await SendEmailAsync(request, to, cc, bcc, subject, body, $"{instance.TemplateType}-{instance.Id}");

                // Update the report instance with the email response.
                instance.Response = response != null ? JsonDocument.Parse(JsonSerializer.Serialize(response, _serializationOptions)) : JsonDocument.Parse("{}");
            }

            instance.IsPublished = true;
            await this.Api.UpdateAVOverviewInstanceAsync(instance);
        }
    }

    /// <summary>
    /// Send an email to CHES.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="tag"></param>
    /// <returns></returns>
    private async Task<EmailResponseModel?> SendEmailAsync(ReportRequestModel request, IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc, string subject, string body, string tag)
    {
        await HandleChesEmailOverrideAsync(request.RequestorId);

        var contexts = new List<EmailContextModel>();
        if (!String.IsNullOrWhiteSpace(request.To) && request.To.IsValidEmail())
        {
            // Add a context for the requested list of users.
            var toAddresses = request.To.Split(",").Select(v => v.Trim()).ToArray();
            contexts.Add(new EmailContextModel(toAddresses, new Dictionary<string, object>(), DateTime.Now)
            {
                Tag = tag,
            });
        }
        else
        {
            var sendToRequestorSeparately = request.RequestorId.HasValue && (cc.Any() || bcc.Any());
            var requestor = sendToRequestorSeparately ? await this.Api.GetUserAsync(request.RequestorId!.Value) : null;
            var requestorEmail = !String.IsNullOrWhiteSpace(requestor?.PreferredEmail) ? requestor.PreferredEmail : requestor?.Email;

            // Do not send multiple emails to the requestor if there is also a CC/BCC list.
            var toAddresses = String.IsNullOrWhiteSpace(requestorEmail) ? to : to.Where(v => v != requestorEmail);
            var toContexts = toAddresses
                .Where(v => v.IsValidEmail())
                .Select(v => new EmailContextModel(new[] { v }, new Dictionary<string, object>(), DateTime.Now)
                {
                    Tag = tag,
                }).ToArray();
            contexts.AddRange(toContexts);

            // If there are BCC and CC options then they must be sent with an email that is sent to the requestor.
            if (sendToRequestorSeparately && !String.IsNullOrWhiteSpace(requestorEmail))
            {
                contexts.Add(new EmailContextModel(new[] { requestorEmail }, new Dictionary<string, object>(), DateTime.Now)
                {
                    Cc = cc,
                    Bcc = bcc,
                    Tag = tag,
                });
            }
        }

        if (!contexts.Any()) return null;

        var merge = new EmailMergeModel(this.ChesOptions.From, contexts, subject, body)
        {
            // TODO: Extract values from report settings.
            Encoding = EmailEncodings.Utf8,
            BodyType = EmailBodyTypes.Html,
            Priority = EmailPriorities.Normal,
        };

        var response = await this.Ches.SendEmailAsync(merge);
        this.Logger.LogInformation("Report sent to CHES.  ReportId:{report}, InstanceId:{instance}", request.ReportId, request.ReportInstanceId);

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
            if (user != null) email = user.GetEmail();
        }
        var identity = _user.Identity as ClaimsIdentity ?? throw new ConfigurationException("CHES requires an active ClaimsPrincipal");
        identity.RemoveClaim(_user.FindFirst(ClaimTypes.Email));
        identity.AddClaim(new Claim(ClaimTypes.Email, email));
    }
    #endregion
}
