using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MMI.SmtpEmail.Models;
using TNO.Core.Extensions;

namespace MMI.SmtpEmail;

/// <summary>
/// EmailService class provides methods to send emails using SMTP. It can be configured with SmtpOptions and can use an injected SmtpClient or create its own. Implements IDisposable to dispose the SmtpClient if it owns it.
/// </summary>
public partial class EmailService : IEmailService, IDisposable
{
    #region Variables
    [GeneratedRegex("src=\\\"data:(image\\/[a-zA-Z]*);base64,([^\\\"]*)\\\"", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-CA")]
    private static partial Regex Base64InlineImageRegex();
    private static readonly Regex ExtractBase64InlineImageRegex = Base64InlineImageRegex();
    private SmtpClient? _client;
    private readonly bool _ownsClient;
    private readonly ILogger _logger;
    private readonly ClaimsPrincipal _user;
    #endregion

    #region Properties
    /// <summary>
    /// get - The SmtpOptions used to configure the EmailService and its SmtpClient. This is exposed as a property so it can be accessed when creating MailMessage objects to apply any relevant options such as OverrideTo and BccUser. The options are set through the constructor and are typically provided via dependency injection using IOptions<SmtpOptions>. If options is null, default SmtpOptions will be used.
    /// </summary>
    protected SmtpOptions Options { get; }
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor initializes with default SmtpOptions and creates its own SmtpClient.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="logger"></param>
    public EmailService(ClaimsPrincipal user, ILogger<EmailService> logger)
    {
        _user = user;
        this.Options = new SmtpOptions();
        _ownsClient = true;
        _logger = logger;
    }

    /// <summary>
    /// Constructor that accepts IOptions<SmtpOptions> to configure the SmtpClient. The EmailService will create and own the SmtpClient instance. If options is null, default SmtpOptions will be used.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public EmailService(ClaimsPrincipal user, IOptions<SmtpOptions> options, ILogger<EmailService> logger)
    {
        _user = user;
        this.Options = options?.Value ?? new SmtpOptions();
        _ownsClient = true;
        _logger = logger;
    }

    /// <summary>
    /// Constructor that accepts an existing SmtpClient and optional IOptions<SmtpOptions> to configure the EmailService. The EmailService will not own the SmtpClient instance.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="client"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public EmailService(ClaimsPrincipal user, SmtpClient client, IOptions<SmtpOptions>? options = null, ILogger<EmailService> logger = null!)
    {
        _user = user;
        this.Options = options?.Value ?? new SmtpOptions();
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = false;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Sends an email asynchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MailResponseModel> SendAsync(
        string subject,
        string body,
        IEnumerable<string> to,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        await SendAsync(subject, body, to, null, null, from, isHtml, attachments, cancellationToken).ConfigureAwait(false);

        return new MailResponseModel(to);
    }

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
    public async Task<MailResponseModel> SendAsync(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc,
        IEnumerable<string>? bcc,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        _client ??= CreateSmtpClient();

        try
        {
            using var message = CreateMailMessage(subject, body, to, cc, bcc, from, isHtml, attachments);
            if (this.Options.EmailEnabled)
            {
                await _client.SendMailAsync(message, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
                return new MailResponseModel(to, cc, bcc);
            }

            _logger.LogWarning("Email sending is disabled. Email content: {EmailContent}", new
            {
                Subject = message.Subject,
                To = message.To.Select(r => r.Address).ToArray(),
                From = message.From?.Address,
            });
            return new MailResponseModel(to, cc, bcc)
            {
                StatusCode = SmtpStatusCode.CommandNotImplemented
            };
        }
        finally
        {
            if (_ownsClient)
            {
                _client?.Dispose();
                _client = null;
            }
        }
    }

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
    public async Task<(bool success, MailResponseModel[] responses)> SendAsync(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var context in contexts)
        {
            // For each context, create a
            var mergedSubject = context.Merge(subject);
            var mergedBody = context.Merge(body);
            await SendAsync(mergedSubject, mergedBody, context.To, context.CC, context.Bcc, from, isHtml, attachments, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
            results.Add(new MailResponseModel(context));
        }
        return (success, results.ToArray());
    }

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
    public async Task<(bool success, MailResponseModel[] responses)> TrySendAsync(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var context in contexts)
        {
            // For each context, create a
            var mergedSubject = context.Merge(subject);
            var mergedBody = context.Merge(body);
            try
            {
                await SendAsync(mergedSubject, mergedBody, context.To, context.CC, context.Bcc, from, isHtml, attachments, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
                results.Add(new MailResponseModel(context));
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error sending email for context {context}", String.Join(',', context.To));
                success = false;
                results.Add(new MailResponseModel(context, ex));
            }
        }
        return (success, results.ToArray());
    }

    /// <summary>
    /// Sends an email asynchronously using a pre-constructed MailMessage object. Uses the configured SmtpClient to send the email. If cancellation is requested, the operation will be cancelled. The MailMessage should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<MailResponseModel> SendAsync(MailMessage message, CancellationToken cancellationToken = default)
    {
        _client ??= CreateSmtpClient();

        try
        {
            if (this.Options.EmailEnabled)
            {
                await _client.SendMailAsync(message, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
                return new MailResponseModel(message);
            }

            _logger.LogWarning("Email sending is disabled. Email content: {EmailContent}", new
            {
                Subject = message.Subject,
                To = message.To.Select(r => r.Address).ToArray(),
                From = message.From?.Address,
            });
            return new MailResponseModel(message)
            {
                StatusCode = SmtpStatusCode.CommandNotImplemented
            };
        }
        finally
        {
            if (_ownsClient)
            {
                _client?.Dispose();
                _client = null;
            }
        }
    }

    /// <summary>
    /// Sends multiple emails asynchronously using a collection of pre-constructed MailMessage objects. Uses the configured SmtpClient to send each email. If cancellation is requested, the operation will be cancelled. Each MailMessage in the collection should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method. If the messages collection is null, no action will be taken. If any individual message is null, it will be skipped.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing the message and every response for each email.</returns>
    public async Task<(bool success, MailResponseModel[] responses)> SendAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        if (messages == null) return (false, []);
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var msg in messages)
        {
            if (msg == null) continue;
            await SendAsync(msg, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
            results.Add(new MailResponseModel(msg));
        }
        return (success, [.. results]);
    }

    /// <summary>
    /// Sends multiple emails asynchronously using a collection of pre-constructed MailMessage objects. Uses the configured SmtpClient to send each email. If cancellation is requested, the operation will be cancelled. Each MailMessage in the collection should be fully constructed with from, to, subject, body, and any other necessary properties before calling this method. If the messages collection is null, no action will be taken. If any individual message is null, it will be skipped.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A tuple containing the message and every response for each email.</returns>
    public async Task<(bool success, MailResponseModel[] responses)> TrySendAsync(IEnumerable<MailMessage> messages, CancellationToken cancellationToken = default)
    {
        if (messages == null) return (false, []);
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var msg in messages)
        {
            if (msg == null) continue;
            try
            {
                await SendAsync(msg, cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
                results.Add(new MailResponseModel(msg));
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error sending email for context {message}", msg.To.ToString());
                success = false;
                results.Add(new MailResponseModel(msg, ex));
            }
        }
        return (success, results.ToArray());
    }

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    public MailResponseModel Send(
        string subject,
        string body,
        IEnumerable<string> to,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null)
    {
        Send(subject, body, to, null, null, from, isHtml, attachments);
        return new MailResponseModel(to);
    }

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
    public MailResponseModel Send(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc,
        IEnumerable<string>? bcc,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null)
    {
        _client ??= CreateSmtpClient();

        try
        {
            using var message = CreateMailMessage(subject, body, to, cc, bcc, from, isHtml, attachments);

            if (this.Options.EmailEnabled)
            {
                _client.Send(message);
                return new MailResponseModel(message);
            }

            _logger.LogWarning("Email sending is disabled. Email content: {EmailContent}", new
            {
                Subject = message.Subject,
                To = message.To.Select(r => r.Address).ToArray(),
                From = message.From?.Address,
            });
            return new MailResponseModel(message)
            {
                StatusCode = SmtpStatusCode.CommandNotImplemented
            };
        }
        finally
        {
            if (_ownsClient)
            {
                _client?.Dispose();
                _client = null;
            }
        }
    }

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    /// <returns></returns>
    public (bool success, MailResponseModel[] responses) Send(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null)
    {
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var context in contexts)
        {
            // For each context, create a
            var mergedSubject = context.Merge(subject);
            var mergedBody = context.Merge(body);
            Send(mergedSubject, mergedBody, context.To, context.CC, context.Bcc, from, isHtml, attachments);
            results.Add(new MailResponseModel(context));
        }
        return (success, [.. results]);
    }

    /// <summary>
    /// Sends an email synchronously with the specified subject, body, recipients, and optional from address and HTML flag. Uses the configured SmtpClient to send the email. If from address is not specified, it will use the default from address from options. If no from address is available, an exception will be thrown.
    /// </summary>
    /// <param name="contexts">An array of MailMergeModel objects containing the context for each email to be sent.</param>
    /// <param name="subject"></param>
    /// <param name="body"></param>
    /// <param name="from"></param>
    /// <param name="isHtml"></param>
    /// <param name="attachments"></param>
    public (bool success, MailResponseModel[] responses) TrySend(
        IEnumerable<MailMergeModel> contexts,
        string subject,
        string body,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null)
    {
        var success = true;
        var results = new List<MailResponseModel>();
        foreach (var context in contexts)
        {
            // For each context, create a
            var mergedSubject = context.Merge(subject);
            var mergedBody = context.Merge(body);
            try
            {
                Send(mergedSubject, mergedBody, context.To, context.CC, context.Bcc, from, isHtml, attachments);
                results.Add(new MailResponseModel(context));
            }
            catch (SmtpException ex)
            {
                _logger.LogError(ex, "Error sending email for context {context}", String.Join(',', context.To));
                success = false;
                results.Add(new MailResponseModel(context, ex));
            }
        }
        return (success, [.. results]);
    }

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
    public MailMessage CreateMailMessage(
        string subject,
        string body,
        IEnumerable<string> to,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        string? from = null,
        bool isHtml = true,
        IEnumerable<AttachmentModel>? attachments = null)
    {
        var message = new MailMessage();
        var fromAddress = string.IsNullOrWhiteSpace(from) ? this.Options.From : from!;
        if (string.IsNullOrWhiteSpace(fromAddress)) throw new ArgumentException("No from address specified.");
        message.From = new MailAddress(fromAddress);
        foreach (var recipient in to.NotNullOrWhiteSpace())
        {
            if (!string.IsNullOrWhiteSpace(recipient)) message.To.Add(recipient);
        }
        if (cc != null)
        {
            foreach (var recipient in cc.NotNullOrWhiteSpace())
            {
                if (!string.IsNullOrWhiteSpace(recipient)) message.CC.Add(recipient);
            }
        }
        if (bcc != null)
        {
            foreach (var recipient in bcc.NotNullOrWhiteSpace())
            {
                if (!string.IsNullOrWhiteSpace(recipient)) message.Bcc.Add(recipient);
            }
        }
        if (this.Options.BccUser)
        {
            // Always BCC the user who generated the email, if the option is enabled and the user's email can be determined. This is done by checking Thread.CurrentPrincipal.Identity.Name for an email address, so it will only work if the application is using authentication and the user's identity name is set to their email address.
            var userEmail = _user.GetEmail();
            if (!string.IsNullOrWhiteSpace(userEmail))
                message.Bcc.Add(userEmail);
        }

        if (!String.IsNullOrWhiteSpace(this.Options.AlwaysBcc))
        {
            // Always BCC the specified email address, if the option is set. This will add the email address(es) specified in AlwaysBcc to the BCC of every email sent. The AlwaysBcc option can contain multiple email addresses separated by commas or semicolons, and they will be parsed and added individually to the BCC.
            var alwaysBcc = this.Options.AlwaysBcc.Split([',', ';']).Select(e => e?.Trim()).NotNullOrWhiteSpace();
            foreach (var recipient in alwaysBcc)
            {
                if (!string.IsNullOrWhiteSpace(recipient) && !message.Bcc.Any(r => r.Address.Equals(recipient, StringComparison.OrdinalIgnoreCase)))
                    message.Bcc.Add(recipient);
            }
        }
        if (!String.IsNullOrWhiteSpace(this.Options.OverrideTo) || !this.Options.EmailAuthorized)
        {
            // If OverrideTo is set, send all emails to the specified address(es) instead of the original recipients. This is typically used in development or testing environments to prevent emails from being sent to real recipients while still allowing the email sending code to be exercised and the email content to be reviewed by sending it to a test address. If EmailAuthorized is false, only send emails to the user who generated the email (determined by Thread.CurrentPrincipal.Identity.Name) and to the email address specified in AlwaysBcc, regardless of what is specified in the To and CC fields. This can be used in production environments to prevent users from sending emails to unintended recipients while still allowing them to receive a copy of the emails they generate.
            var overrideTo = !String.IsNullOrWhiteSpace(this.Options.OverrideTo)
                ? this.Options.OverrideTo.Split(";").NotNullOrWhiteSpace().Select(e => e.Trim())
                : new[] { _user.GetEmail() }.Where(e => !String.IsNullOrWhiteSpace(e)).Select(e => e!);
            foreach (var recipient in overrideTo)
            {
                if (!string.IsNullOrWhiteSpace(recipient) && !message.To.Any(r => r.Address.Equals(recipient, StringComparison.OrdinalIgnoreCase)))
                    message.To.Add(recipient);
            }
            message.CC.Clear();
            message.Bcc.Clear();
        }

        message.Subject = subject ?? string.Empty;
        message.Body = body ?? string.Empty;
        message.IsBodyHtml = isHtml;

        // convert any embedded base64 images into attachments
        Dictionary<string, AttachmentModel> inlineImageMatches = GetImagesFromEmailBody(message.Body);
        if (inlineImageMatches.Count != 0)
        {
            foreach (KeyValuePair<string, AttachmentModel> m in inlineImageMatches)
            {
                message.Body = message.Body.Replace(m.Key, $"src=\"cid:{m.Value.Filename}\"");
            }
            // Add inline images as attachments with ContentId set to the filename so they can be referenced via cid:filename in the HTML body.
            foreach (var am in inlineImageMatches.Values)
            {
                try
                {
                    var bytes = Convert.FromBase64String(am.Content);
                    var ms = new System.IO.MemoryStream(bytes);
                    var attachment = new Attachment(ms, am.Filename)
                    {
                        ContentId = am.Filename,
                        ContentType = new System.Net.Mime.ContentType(am.ContentType ?? "application/octet-stream"),
                    };
                    if (attachment.ContentDisposition != null)
                    {
                        attachment.ContentDisposition.Inline = true;
                        attachment.ContentDisposition.DispositionType = System.Net.Mime.DispositionTypeNames.Inline;
                    }
                    message.Attachments.Add(attachment);
                }
                catch
                {
                    // ignore invalid inline images
                    _logger.LogWarning("Failed to parse inline image for email. Content will be ignored. ContentType: {ContentType}, Filename: {Filename}", am.ContentType, am.Filename);
                }
            }
        }

        // Add explicit attachments passed in by the caller.
        if (attachments != null)
        {
            foreach (var am in attachments)
            {
                if (am == null) continue;
                try
                {
                    var bytes = string.IsNullOrWhiteSpace(am.Content) ? Array.Empty<byte>() : Convert.FromBase64String(am.Content);
                    var ms = new System.IO.MemoryStream(bytes);
                    var attachment = new Attachment(ms, am.Filename ?? string.Empty)
                    {
                        ContentType = new System.Net.Mime.ContentType(am.ContentType ?? "application/octet-stream")
                    };
                    message.Attachments.Add(attachment);
                }
                catch
                {
                    // ignore attachment errors
                    _logger.LogWarning("Failed to parse attachment for email. Attachment will be ignored. ContentType: {ContentType}, Filename: {Filename}", am.ContentType, am.Filename);
                }
            }
        }

        return message;
    }

    /// <summary>
    /// Creates and configures an SmtpClient based on the SmtpOptions. If username is specified in options, it will set the credentials for the client. If timeout is specified, it will set the timeout for the client. The client is configured to use Network delivery method and SSL and default credentials based on the options.
    /// </summary>
    /// <returns></returns>
    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(this.Options.Host, this.Options.Port)
        {
            EnableSsl = this.Options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = this.Options.UseDefaultCredentials
        };

        if (!string.IsNullOrWhiteSpace(this.Options.Username))
        {
            client.Credentials = new NetworkCredential(this.Options.Username, this.Options.Password);
        }

        if (this.Options.Timeout.HasValue)
            client.Timeout = (int)this.Options.Timeout.Value.TotalMilliseconds;

        return client;
    }

    /// <summary>
    /// parses email markup string checking for base64 encoded images
    /// </summary>
    /// <param name="emailBody">email body as html markup - possibly containing base64 encoded images</param>
    /// <returns>dictionary of the images as attachments and the 'key' to use to search and replace them in the markup</returns>
    private static Dictionary<string, AttachmentModel> GetImagesFromEmailBody(string emailBody)
    {
        var imageDictionary = new Dictionary<string, AttachmentModel>();

        var inlineImageMatches = ExtractBase64InlineImageRegex.Matches(emailBody);
        if (inlineImageMatches.Count != 0)
        {
            foreach (var m in inlineImageMatches.Cast<Match>())
            {
                var imageMediaType = m.Groups[1].Value;
                var base64Image = m.Groups[2].Value;
                var attachment = new AttachmentModel
                {
                    ContentType = imageMediaType,
                    Encoding = "base64",
                    Filename = Guid.NewGuid().ToString(),
                    Content = base64Image
                };
                imageDictionary.TryAdd(m.Value, attachment);
            }
        }
        return imageDictionary;
    }

    /// <summary>
    /// Disposes the SmtpClient if this EmailService instance owns it. If the SmtpClient was injected and is not owned by this instance, it will not be disposed. Suppresses finalization after disposing.
    /// </summary>
    public void Dispose()
    {
        if (_ownsClient)
        {
            try { _client?.Dispose(); } catch { }
        }
        GC.SuppressFinalize(this);
    }
    #endregion
}

