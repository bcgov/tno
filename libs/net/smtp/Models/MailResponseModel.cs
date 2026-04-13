using System.Net.Mail;
using System.Text.Json;
using TNO.Core.Extensions;

namespace MMI.SmtpEmail.Models;

/// <summary>
/// MailResponseModel class, provides a model to pass SMTP response details back to the caller. This includes the email addresses the message was sent to, the status code of the SMTP response, any error message from the SMTP response, and any additional details about the error that may be helpful for troubleshooting. This model is used to determine if a message should be retried or not. If the status code indicates a transient error or if the error message indicates a transient error then it should be retried, but if the status code indicates a permanent error or if the error message indicates a permanent error then it should not be retried. Additionally, if the email was sent more than a certain amount of time ago (e.g. 24 hours) then it should not be retried as it may have been updated since the email was sent.
/// </summary>
public class MailResponseModel
{
    #region Properties
    /// <summary>
    /// get/set - Who the email will be sent to.
    /// </summary>
    public string[] To { get; set; } = [];

    /// <summary>
    /// get/set - Who the email will be sent to as a carbon copy (CC). This is optional and can be used to send a copy of the email to additional recipients while revealing their email addresses to the primary recipients. If specified, the email addresses in this field will receive a copy of the email, and their addresses will be visible to the primary recipients and other CC recipients.
    /// </summary>
    public string[]? CC { get; set; }

    /// <summary>
    /// get/set - Who the email will be sent to as a blind carbon copy (BCC). This is optional and can be used to send a copy of the email to additional recipients without revealing their email addresses to the primary recipients. If specified, the email addresses in this field will receive a copy of the email, but their addresses will not be visible to the primary recipients or other BCC recipients.
    /// </summary>
    public string[]? Bcc { get; set; }

    /// <summary>
    /// get/set - The status code of the SMTP response.
    /// </summary>
    public SmtpStatusCode StatusCode { get; set; }

    /// <summary>
    /// get/set - Data that may be helpful for troubleshooting. This could include information such as whether the error was a transient error or a permanent error, or any other relevant details that may help determine whether the message should be retried or not.
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// get/set - The error message from the SMTP response. This is used to determine if the message should be retried or not. If the error message indicates a transient error then it should be retried, but if it indicates a permanent error then it should not be retried.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// get/set - Any additional details about the error that may be helpful for troubleshooting. This could include information such as whether the error was a transient error or a permanent error, or any other relevant details that may help determine whether the message should be retried or not.
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// get/set - When the email was sent. This is used to determine if the message should be retried or not. If the email was sent more than a certain amount of time ago (e.g. 24 hours) then it should not be retried as it may have been updated since the email was sent.
    /// </summary>
    public DateTimeOffset SentOn { get; set; } = DateTimeOffset.UtcNow;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    public MailResponseModel() { }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="exception"></param>
    public MailResponseModel(IEnumerable<string> to, SmtpException? exception = null)
    {
        this.To = [.. to];
        this.StatusCode = exception?.StatusCode ?? SmtpStatusCode.Ok;
        this.ErrorMessage = exception?.Message;
        this.ErrorDetails = exception?.GetAllMessages();
    }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="exception"></param>
    public MailResponseModel(IEnumerable<string> to, IEnumerable<string>? cc, IEnumerable<string>? bcc, SmtpException? exception = null)
    {
        this.To = [.. to];
        this.CC = cc?.ToArray();
        this.Bcc = bcc?.ToArray();
        this.StatusCode = exception?.StatusCode ?? SmtpStatusCode.Ok;
        this.ErrorMessage = exception?.Message;
        this.ErrorDetails = exception?.GetAllMessages();
    }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    public MailResponseModel(MailMergeModel context)
    {
        this.To = context.To;
        this.CC = context.CC?.ToArray();
        this.Bcc = context.Bcc?.ToArray();
        this.StatusCode = SmtpStatusCode.Ok;
    }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    public MailResponseModel(MailMergeModel context, SmtpException exception)
    {
        this.To = context.To;
        this.CC = context.CC?.ToArray();
        this.Bcc = context.Bcc?.ToArray();
        this.StatusCode = exception.StatusCode;
        this.ErrorMessage = exception.Message;
        this.ErrorDetails = exception.GetAllMessages();
    }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    public MailResponseModel(MailMergeModel context, Exception exception)
    {
        this.To = context.To;
        this.CC = context.CC?.ToArray();
        this.Bcc = context.Bcc?.ToArray();
        this.StatusCode = SmtpStatusCode.GeneralFailure;
        this.ErrorMessage = exception.Message;
        this.ErrorDetails = exception.GetAllMessages();
    }

    /// <summary>
    /// Creates a new instance of a MailResponseModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="exception"></param>
    public MailResponseModel(MailMessage message, SmtpException? exception = null)
    {
        this.To = [.. message.To.Select(t => t.Address)];
        this.CC = message.CC?.Select(t => t.Address).ToArray();
        this.Bcc = message.Bcc?.Select(t => t.Address).ToArray();
        this.StatusCode = exception?.StatusCode ?? SmtpStatusCode.Ok;
        this.ErrorMessage = exception?.Message;
        this.ErrorDetails = exception?.GetAllMessages();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Converts this MailResponseModel object to a JsonDocument. This is used to pass the response details back to the caller in a structured format that can be easily parsed and used to determine if a message should be retried or not.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public JsonDocument ToJsonDocument(JsonSerializerOptions? options = null)
    {
        return JsonDocument.Parse(JsonSerializer.Serialize(this, options));
    }
    #endregion
}
