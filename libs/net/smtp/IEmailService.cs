using System.Net.Mail;
using MMI.SmtpEmail.Models;

namespace MMI.SmtpEmail;

/// <summary>
/// Defines an interface for an email service that can send emails with various options including subject, body, recipients, attachments, and support for mail merge functionality. The service provides both asynchronous and synchronous methods for sending emails, as well as a method for creating a MailMessage object based on the provided parameters and default options. The SendAsync methods allow for cancellation through a CancellationToken, and the mail merge functionality allows for dynamic generation of email content based on a set of variables provided in the context. The CreateMailMessage method can be used to create a MailMessage object that can be further customized or sent using the SendAsync method that accepts a MailMessage parameter.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email asynchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MailResponseModel> SendAsync(string subject, string body, IEnumerable<string> to, string? from = null, bool isHtml = true, IEnumerable<MMI.SmtpEmail.Models.AttachmentModel>? attachments = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email asynchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MailResponseModel> SendAsync(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc,
        IEnumerable<string>? bcc,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email asynchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// Each context provided will generate a separate email with the subject and body merged with the context using MailMergeModel.Merge. This allows for sending personalized emails to multiple recipients in a single call. The to, cc, and bcc parameters will be applied to all generated emails, but the subject and body can be customized for each email based on the context.
    /// Email merging is done by replacing placeholders in the subject and body with values from the context. Placeholders are defined as {{PropertyName}} where PropertyName is a property of the MailMergeModel. For example, if the context has a property called FirstName, you could use {{FirstName}} in the subject or body and it would be replaced with the value of that property for each email sent. This allows for dynamic generation of email content based on the provided contexts.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An array of tuples containing the context and any error that occurred while sending the email.</returns>
    Task<(bool success, MailResponseModel[] responses)> SendAsync(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email asynchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// Each context provided will generate a separate email with the subject and body merged with the context using MailMergeModel.Merge. This allows for sending personalized emails to multiple recipients in a single call. The to, cc, and bcc parameters will be applied to all generated emails, but the subject and body can be customized for each email based on the context.
    /// Email merging is done by replacing placeholders in the subject and body with values from the context. Placeholders are defined as {{PropertyName}} where PropertyName is a property of the MailMergeModel. For example, if the context has a property called FirstName, you could use {{FirstName}} in the subject or body and it would be replaced with the value of that property for each email sent. This allows for dynamic generation of email content based on the provided contexts.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An array of tuples containing the context and any error that occurred while sending the email.</returns>
    Task<(bool success, MailResponseModel[] responses)> TrySendAsync(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email asynchronously using a pre-constructed MailMessage object. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. The MailMessage should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<MailResponseModel> SendAsync(MailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple emails asynchronously using a collection of pre-constructed MailMessage objects. Uses the configured SmtpClient to send each email. If cancellation is requested, the operation will be cancelled. Each MailMessage in the collection should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method. If the messages collection is null, no action will be taken. If any individual message is null, it will be skipped.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An array of tuples containing the message and any error that occurred while sending each email.</returns>
    Task<(bool success, MailResponseModel[] responses)> SendAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends multiple emails asynchronously using a collection of pre-constructed MailMessage objects. Uses the configured SmtpClient to send each email. If cancellation is requested, the operation will be cancelled. Each MailMessage in the collection should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method. If the messages collection is null, no action will be taken. If any individual message is null, it will be skipped.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>An array of tuples containing the message and any error that occurred while sending each email.</returns>
    Task<(bool success, MailResponseModel[] responses)> TrySendAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <returns></returns>
    MailResponseModel Send(string subject, string body, IEnumerable<string> to, string? from = null, bool isHtml = true, IEnumerable<MMI.SmtpEmail.Models.AttachmentModel>? attachments = null);

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    MailResponseModel Send(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc,
        IEnumerable<string>? bcc,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null);

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns>An array of tuples containing the context and any error that occurred while sending each email.</returns>
    (bool success, MailResponseModel[] responses) Send(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null);

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns>An array of tuples containing the context and any error that occurred while sending each email.</returns>
    (bool success, MailResponseModel[] responses) TrySend(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null);

    /// <summary>
    /// Creates a MailMessage object based on the provided parameters and the default from address in options. If from address is not specified and no default from address is available, an exception will be thrown.
    /// </summary> <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="cc"></param>
    /// <param name="bcc"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    MailMessage CreateMailMessage(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null);
}
