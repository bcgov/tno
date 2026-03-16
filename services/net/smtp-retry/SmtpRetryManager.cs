using System.Net.Mail;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.Services.SmtpRetry.Config;
using MMI.SmtpEmail;
using TNO.Core.Exceptions;
using TNO.Entities;
using TNO.Services;
using TNO.Services.Managers;

namespace MMI.Services.SmtpRetry;

/// <summary>
/// SmtpRetryManager class, provides a service which checks if there are any emails that have not been successfully sent..
/// </summary>
public class SmtpRetryManager : ServiceManager<SmtpRetryOptions>
{
    #region Variables
    private readonly JsonSerializerOptions _serializationOptions;
    private readonly ClaimsPrincipal _user;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChesRetryManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="notificationEngine"></param>
    /// <param name="emailService"></param>
    /// <param name="smtpOptions"></param>
    /// <param name="chesService"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="notificationOptions"></param>
    /// <param name="reportingOptions"></param>
    /// <param name="notificationValidator"></param>
    /// <param name="logger"></param>
    public SmtpRetryManager(
        IApiService api,
        ClaimsPrincipal user,
        IEmailService emailService,
        IOptions<SmtpOptions> smtpOptions,
        IOptions<JsonSerializerOptions> serializationOptions,
        IOptions<SmtpRetryOptions> notificationOptions,
        ILogger<SmtpRetryManager> logger)
        : base(api, emailService, smtpOptions, notificationOptions, logger)
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
            try
            {
                if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
                {
                    // An API request or failures have requested the service to stop.
                    this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                    this.State.Stop();
                }
                else if (this.State.Status == ServiceStatus.Failed && this.Options.AutoRestartAfterCriticalFailure)
                {
                    await Task.Delay(this.Options.RetryAfterCriticalFailureDelayMS);
                    this.State.Resume();
                    continue;
                }
                else if (this.State.Status != ServiceStatus.Running)
                {
                    this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
                }
                else
                {
                    if (this.Options.RetryReports) await RetryReportsAsync();
                    if (this.Options.RetryNotifications) await RetryNotificationsAsync();
                }

                // The delay ensures we don't have a run away thread.
                this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Service had an unexpected failure.");
                this.State.RecordFailure();
                await this.SendErrorEmailAsync("Service had an Unexpected Failure", ex);
                await Task.Delay(delay);
            }
        }
    }

    /// <summary>
    /// Make a request to fetch the report instance and user report instance for the message.
    /// Try to promote the email to force it to be sent.
    /// </summary>
    /// <param name="reportMessage"></param>
    /// <returns></returns>
    private async Task<bool> RetryReportInstanceAsync(TNO.API.Areas.Services.Models.Report.SmtpReportMessagesModel reportMessage)
    {
        var reportInstance = await this.Api.GetReportInstanceAsync(reportMessage.InstanceId);
        if (reportInstance == null) return false;

        // If there is a userId, get the record for the user report instance to check the status of the email.
        var userReportInstance = reportMessage.UserId.HasValue ? await this.Api.GetUserReportInstanceAsync(reportMessage.InstanceId, reportMessage.UserId.Value) : null;
        if (reportMessage.UserId.HasValue && userReportInstance == null) return false;

        // If there is a userId, then this is a message for an individual user report instance, otherwise it is for the whole report.
        if (userReportInstance != null)
        {
            if (reportMessage.Format == ReportDistributionFormat.FullText || reportMessage.Format == ReportDistributionFormat.ReceiveBoth && !String.IsNullOrWhiteSpace(reportInstance.Body))
            {
                var response = JsonSerializer.Deserialize<SmtpEmail.Models.MailResponseModel>(userReportInstance.TextResponse, _serializationOptions);
                if (response == null)
                {
                    this.Logger.LogWarning("Failed to deserialize user report instance text response.  Report ID: {reportId}, Instance ID: {instanceId}, User Id: {userId}",
                        reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);
                    return false;
                }

                // The message is possibly stuck, we need to ask CHES to promote.
                // Promoting doesn't change the status.
                // Which means it may get picked up again in the Accepted status until it gets sent.
                this.Logger.LogDebug("Retry email request.  Report ID: {reportId}, Instance ID: {instanceId}, User Id: {userId}",
                    reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);

                var message = this.EmailService.CreateMailMessage(reportInstance.Subject, reportInstance.Body, response.To, response.CC, response.Bcc);
                response = await this.EmailService.SendAsync(message);

                if (response.StatusCode == SmtpStatusCode.Ok)
                {
                    response.Data = $"This email was sent by the SMTP Retry Service.";
                    userReportInstance.TextResponse = response.ToJsonDocument(_serializationOptions);
                    userReportInstance.TextStatus = ReportStatus.Completed;
                    reportMessage.Status = ReportStatus.Completed;
                    await this.Api.AddOrUpdateUserReportInstanceAsync(userReportInstance);
                }
            }
            else if (reportMessage.Format == ReportDistributionFormat.LinkOnly || reportMessage.Format == ReportDistributionFormat.ReceiveBoth && !String.IsNullOrWhiteSpace(reportInstance.LinkOnlyBody))
            {
                var response = JsonSerializer.Deserialize<SmtpEmail.Models.MailResponseModel>(userReportInstance.LinkResponse, _serializationOptions);
                if (response == null)
                {
                    this.Logger.LogWarning("Failed to deserialize user report instance link only response.  Report ID: {reportId}, Instance ID: {instanceId}, User Id: {userId}",
                        reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);
                    return false;
                }

                // The message is possibly stuck, we need to ask CHES to promote.
                // Promoting doesn't change the status.
                // Which means it may get picked up again in the Accepted status until it gets sent.
                this.Logger.LogDebug("Retry email request.  Report ID: {reportId}, Instance ID: {instanceId}, User Id: {userId}",
                    reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);

                var message = this.EmailService.CreateMailMessage(reportInstance.Subject, reportInstance.LinkOnlyBody, response.To, response.CC, response.Bcc);
                response = await this.EmailService.SendAsync(message);

                if (response.StatusCode == SmtpStatusCode.Ok)
                {
                    response.Data = $"This email was sent by the SMTP Retry Service.";
                    userReportInstance.LinkResponse = response.ToJsonDocument(_serializationOptions);
                    userReportInstance.LinkStatus = ReportStatus.Completed;
                    reportMessage.Status = ReportStatus.Completed;
                    await this.Api.AddOrUpdateUserReportInstanceAsync(userReportInstance);
                }
            }
            return true;
        }
        else
        {
            this.Logger.LogWarning("Unable to retry sending this report.  Report ID: {reportId}, Instance ID: {instanceId}", reportMessage.ReportId, reportMessage.InstanceId);
            return false;
        }
    }


    /// <summary>
    /// Make a request to fetch the AV overview report instance and user report instance for the message.
    /// Try to promote the email to force it to be sent.
    /// </summary>
    /// <param name="reportMessage"></param>
    /// <returns></returns>
    private async Task<bool> RetryAVOverviewReportInstanceAsync(TNO.API.Areas.Services.Models.Report.SmtpReportMessagesModel reportMessage)
    {

        var reportInstance = await this.Api.GetAVOverviewInstanceAsync(reportMessage.InstanceId);
        if (reportInstance == null) return false;

        // If there is a userId, get the record for the user report instance to check the status of the email.
        var userReportInstance = reportMessage.UserId.HasValue ? await this.Api.GetUserAVOverviewInstanceAsync(reportMessage.InstanceId, reportMessage.UserId.Value) : null;
        if (reportMessage.UserId.HasValue && userReportInstance == null) return false;

        if (userReportInstance != null)
        {
            var response = JsonSerializer.Deserialize<SmtpEmail.Models.MailResponseModel>(userReportInstance.Response, _serializationOptions);
            if (response == null)
            {
                this.Logger.LogWarning("Failed to deserialize AV user report instance text response.  Report ID: {reportId}, Instance ID: {instanceId}, User Id: {userId}",
                    reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);
                return false;
            }

            var message = this.EmailService.CreateMailMessage(reportInstance.Subject, reportInstance.Body, response.To, response.CC, response.Bcc);
            response = await this.EmailService.SendAsync(message);

            if (response.StatusCode == SmtpStatusCode.Ok)
            {
                response.Data = $"This email was sent by the SMTP Retry Service.";
                userReportInstance.Response = response.ToJsonDocument(_serializationOptions);
                userReportInstance.Status = ReportStatus.Completed;
                reportMessage.Status = ReportStatus.Completed;
                await this.Api.AddOrUpdateUserAVOverviewInstanceAsync(userReportInstance);
            }
            return true;
        }
        else
        {
            this.Logger.LogWarning("Unable to retry sending this AV overview report.  Report ID: {reportId}, Instance ID: {instanceId}", reportMessage.ReportId, reportMessage.InstanceId);
            return false;
        }
    }

    /// <summary>
    /// Make a request to fetch all reports that have emails in accepted status.
    /// Try to promote these emails to force them to be sent.
    /// </summary>
    /// <returns></returns>
    private async Task RetryReportsAsync()
    {
        // Request all reports for the last hour that have failed to send.
        var now = DateTime.UtcNow;

        // This returns an array of report user instances that have failed to send emails.
        var reportMessages = await this.Api.GetSmtpMessagesAsync(ReportStatus.Failed, now.AddMinutes(-1 * this.Options.RetryTimeScope)) ?? [];
        foreach (var reportMessage in reportMessages)
        {
            try
            {
                // Only check the status of the SMTP message if a report was sent more than 5 minutes ago.
                if (reportMessage.SentOn.HasValue && reportMessage.SentOn.Value.AddMinutes(this.Options.RetryTimeLimit) <= now)
                {
                    if (reportMessage.ReportType == ReportType.Content)
                    {
                        var updated = await RetryReportInstanceAsync(reportMessage);
                        if (!updated) continue;
                    }
                    else
                    {
                        var updated = await RetryAVOverviewReportInstanceAsync(reportMessage);
                        if (!updated) continue;
                    }
                }

                // Slow down the number of email requests.
                if (this.Options.ArtificialDelayMs > 0)
                    await Task.Delay(this.Options.ArtificialDelayMs);
            }
            catch (SmtpException ex)
            {
                this.Logger.LogError(ex, "Failed to send email.  Report ID: {reportId}, Instance Id: {instanceId}, User Id: {userId}", reportMessage.ReportId, reportMessage.InstanceId, reportMessage.UserId);
                continue;
            }
        }

        var reportGroups = reportMessages.GroupBy(r => new { r.InstanceId, r.ReportType });
        foreach (var group in reportGroups)
        {
            // If all user report instances have been updated then the report instance should be updated as well.
            if (group.All(r => r.Status == ReportStatus.Completed))
            {
                try
                {
                    if (group.Key.ReportType == ReportType.Content)
                    {
                        var reportInstance = await this.Api.GetReportInstanceAsync(group.Key.InstanceId);
                        if (reportInstance == null) continue;
                        // We should update the full response for the report instance, but we can at least update the status to completed so that it doesn't get picked up again in the retry.
                        this.Logger.LogInformation("Update report status to completed. Instance ID: {instanceId}", group.Key);
                        reportInstance.Status = ReportStatus.Completed;
                        reportInstance.Response = JsonDocument.Parse($"{{ \"Data\": \"This report was marked as completed by the SMTP Retry Service.\" }}");
                        await this.Api.UpdateReportInstanceAsync(reportInstance, false);
                    }
                    else
                    {
                        var reportInstance = await this.Api.GetAVOverviewInstanceAsync(group.Key.InstanceId);
                        if (reportInstance == null) continue;
                        this.Logger.LogInformation("Update AV overview report status to completed. Instance ID: {instanceId}", group.Key);
                        reportInstance.Status = ReportStatus.Completed;
                        reportInstance.Response = JsonDocument.Parse($"{{ \"Data\": \"This report was marked as completed by the SMTP Retry Service.\" }}");
                        await this.Api.UpdateAVOverviewInstanceAsync(reportInstance);
                    }
                }
                catch (NoContentException ex)
                {
                    this.Logger.LogError(ex, "Report instance does not exist.  Instance ID: {instanceId}", group.Key);
                }
            }
        }
    }

    /// <summary>
    /// Make a request to fetch all notifications that have emails in accepted status.
    /// Try to resend these emails to force them to be sent.
    /// </summary>
    /// <returns></returns>
    private async Task RetryNotificationsAsync()
    {
        // Request all notifications for the last hour that SMTP has not confirmed the email has been sent.
        var now = DateTime.UtcNow;
        var notificationMessages = await this.Api.GetSmtpMessagesAsync(NotificationStatus.Accepted, now.AddMinutes(-1 * this.Options.RetryTimeScope)) ?? [];

        foreach (var notificationMessage in notificationMessages)
        {
            // Only check the status of the SMTP message if a notification was sent more than 5 minutes ago.
            if (notificationMessage.SentOn.HasValue && notificationMessage.SentOn.Value.AddMinutes(this.Options.RetryTimeLimit) <= now)
            {
                var responses = JsonSerializer.Deserialize<SmtpEmail.Models.MailResponseModel[]>(notificationMessage.Responses, _serializationOptions);
                if (responses == null) break;

                var completed = 0;
                foreach (var response in responses)
                {
                    // Don't try to resend if the email was sent successfully.
                    if (response == null || response.StatusCode == SmtpStatusCode.Ok) break;

                    try
                    {
                        var notificationInstance = await this.Api.GetNotificationInstanceAsync(notificationMessage.InstanceId);
                        if (notificationInstance == null) break;

                        var message = this.EmailService.CreateMailMessage(notificationInstance.Subject, notificationInstance.Body, response.To, response.CC, response.Bcc);
                        var responseUpdate = await this.EmailService.SendAsync(message);

                        this.Logger.LogInformation("Sent email. Notification ID: {notificationId}, Instance ID: {instanceId}", notificationMessage.NotificationId, notificationMessage.InstanceId);

                        // Keep track of how many of the emails have been sent.
                        if (responseUpdate.StatusCode == SmtpStatusCode.Ok) completed++;
                    }
                    catch (SmtpException ex)
                    {
                        this.Logger.LogError(ex, "Failed to send email.  Notification ID: {notificationId}, Instance Id: {instanceId}", notificationMessage.NotificationId, notificationMessage.InstanceId);
                        continue;
                    }

                    // Slow down the number of email requests.
                    if (this.Options.ArtificialDelayMs > 0)
                        await Task.Delay(this.Options.ArtificialDelayMs);
                }

                // If all messages for this notification instance have been sent, update the status.
                if (notificationMessage.Status != NotificationStatus.Completed && responses.Length == completed)
                {
                    this.Logger.LogInformation("Update notification status.  Notification ID: {notificationId}, Instance ID: {instanceId}, Status: {status}", notificationMessage.NotificationId, notificationMessage.InstanceId, NotificationStatus.Completed);
                    await this.Api.UpdateNotificationInstanceAsync(notificationMessage.InstanceId, NotificationStatus.Completed);
                }
            }
        }
    }
    #endregion
}
