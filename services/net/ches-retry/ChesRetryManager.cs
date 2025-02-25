using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Services.ChesRetry.Config;
using TNO.Services.Managers;

namespace TNO.Services.ChesRetry;

/// <summary>
/// ChesRetryManager class, provides a service which checks if there are any emails that have not been successfully sent..
/// </summary>
public class ChesRetryManager : ServiceManager<ChesRetryOptions>
{
    #region Variables
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly ClaimsPrincipal _user;
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChesRetryManager object, initializes with specified parameters.
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
    public ChesRetryManager(
        IApiService api,
        ClaimsPrincipal user,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<ChesRetryOptions> notificationOptions,
        ILogger<ChesRetryManager> logger)
        : base(api, chesService, chesOptions, notificationOptions, logger)
    {
        _user = user;
        _serializationOptions = serializationOptions.Value;
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
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // Request all reports for the last hour that CHES has not confirmed the email has been sent.
                    var now = DateTime.UtcNow;
                    var reportMessages = await this.Api.GetChesMessagesAsync(Entities.ReportStatus.Accepted, now.AddMinutes(-1 * this.Options.RetryTimeScope)) ?? [];

                    foreach (var report in reportMessages)
                    {
                        // Only check the status of the CHES message if a report was sent more than 5 minutes ago.
                        if (report.SentOn.HasValue && report.SentOn.Value.AddMinutes(this.Options.RetryTimeLimit) <= now)
                        {
                            foreach (var messageId in report.MessageIds)
                            {
                                // For each message get the latest status.
                                var statusResponse = await this.Ches.GetStatusAsync(messageId);
                                if (Enum.TryParse<Entities.ReportStatus>(statusResponse.Status, true, out Entities.ReportStatus status)
                                    && status == Entities.ReportStatus.Accepted)
                                {
                                    try
                                    {
                                        // The message is possibly stuck, we need to ask CHES to promote.
                                        // Promoting doesn't change the status.
                                        // Which means it may get picked up again in the Accepted status until it gets sent.
                                        this.Logger.LogDebug("Promote email request.  Report Instance ID: {reportId}, User Id: {userId}, Message Id: {messageId}", report.ReportInstanceId, report.UserId, messageId);
                                        await this.Ches.PromoteAsync(messageId);
                                    }
                                    catch (ChesException ex)
                                    {
                                        if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                                        {
                                            // The status changed between the first call and an attempt to promote.
                                            statusResponse = await this.Ches.GetStatusAsync(messageId);
                                            this.Logger.LogWarning("Email status changed.  Report Instance ID: {reportId}, User Id: {userId}, Message Id: {messageId}, Status: {status}", report.ReportInstanceId, report.UserId, messageId, statusResponse.Status);
                                            status = Enum.Parse<Entities.ReportStatus>(statusResponse.Status);
                                        }
                                        else
                                        {
                                            // Ignore other errors.
                                            this.Logger.LogError(ex, "Failed to promote email.  Report instance Id: {id} User Id: {userId}", report.ReportInstanceId, report.UserId);
                                        }
                                    }
                                }

                                if (report.Status != status)
                                {
                                    this.Logger.LogInformation("Email status changed, update report.  Report Instance ID: {reportId}, User Id: {userId}, Message Id: {messageId}, status: {status}", report.ReportInstanceId, report.UserId, messageId, status);
                                    // Update the report with the latest status.
                                    if (report.ReportType == ReportType.Content)
                                    {
                                        await this.Api.UpdateUserReportInstanceAsync(report.ReportInstanceId, report.Format, report.UserId, status);
                                    }
                                    else if (report.ReportType == ReportType.AVOverview)
                                    {
                                        await this.Api.UpdateAVReportInstanceAsync(report.ReportInstanceId, report.UserId, status);
                                    }
                                    report.Status = status;
                                }
                            }
                        }
                    }

                    var reportGroups = reportMessages.GroupBy(r => r.ReportInstanceId);
                    foreach (var group in reportGroups)
                    {
                        // If all user report instances have been updated then the report instance should be updated to.
                        if (group.All(r => r.Status == Entities.ReportStatus.Completed))
                        {
                            this.Logger.LogInformation("Update report status to completed.  Report Instance ID: {reportId}", group.Key);
                            await this.Api.UpdateReportInstanceAsync(group.Key, Entities.ReportStatus.Completed);
                        }
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
    #endregion
}
