namespace MMI.SmtpEmail;

/// <summary>
/// Represents the configuration options for the SMTP email service. This class contains properties for configuring the SMTP server connection, default from address, email sending behavior, and recipient overrides. These options can be configured through dependency injection using the ServicesExtensions methods, allowing for flexible configuration of the email service in different environments and scenarios.
/// </summary>
public class SmtpOptions
{
    /// <summary>
    /// get/set - The SMTP server host. Default is "localhost".
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// get/set - The SMTP server port. Default is 25.
    /// </summary>
    public int Port { get; set; } = 25;

    /// <summary>
    /// get/set - Whether SSL should be used when connecting to the SMTP server. Default is false.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// get/set - Whether the default credentials should be used when connecting to the SMTP server. Default is false.
    /// </summary>
    public bool UseDefaultCredentials { get; set; } = false;

    /// <summary>
    /// get/set - The username to use when connecting to the SMTP server. Default is empty string.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// get/set - The password to use when connecting to the SMTP server. Default is empty string.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// get/set - The default from address to use when sending email. Default is empty string, which will require the from address to be specified on each email. If set, this will be used as the from address for any email that does not specify a from address.
    /// </summary>
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// get/set - The timeout to use when sending email. Default is null, which uses the SmtpClient default timeout of 100 seconds.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// get/set - Whether email sending is enabled. Default is true. If false, the EmailService will not send emails and will instead log the email content at the Information level. This can be used to disable email sending in development or testing environments without having to change the code that sends emails.
    /// </summary>
    public bool EmailEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Whether email sending is authorized. Default is true. If false, the EmailService will only send emails to the user who initiaded the email (determined by Thread.CurrentPrincipal.Identity.Name) and to the email address specified in AlwaysBcc. This can be used to prevent users from sending emails to unintended recipients while still allowing them to receive a copy of the emails they generate.
    /// </summary>
    public bool EmailAuthorized { get; set; } = true;

    /// <summary>
    /// get/set - Always BCC the user who generated the email. Default is false. If true, the EmailService will attempt to add the current user to the BCC of every email sent. The current user is determined by checking Thread.CurrentPrincipal.Identity.Name, so this will only work if the application is using authentication and the user's identity name is set to their email address. This can be used to ensure that the user receives a copy of every email they generate, even if they are not in the To or CC fields.
    /// </summary>
    public bool BccUser { get; set; }

    /// <summary>
    /// get/set - Always BCC the specified email address. Default is null. If set, the EmailService will add this email address to the BCC of every email sent. This can be used to ensure that a specific email address receives a copy of every email sent, regardless of the recipients specified in the To and CC fields.
    /// </summary>
    public string? AlwaysBcc { get; set; }

    /// <summary>
    /// get/set - Send all emails to this email address instead of their original recipients. Default is null. If set, the EmailService will ignore the To and CC fields of the email and instead send the email to this address. This can be used in development or testing environments to prevent emails from being sent to real recipients while still allowing the email sending code to be exercised and the email content to be reviewed by sending it to a test address.
    /// </summary>
    public string? OverrideTo { get; set; }
}
