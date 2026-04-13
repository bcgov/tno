using Fluid;

namespace MMI.SmtpEmail.Models;

public class MailMergeModel
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
    /// get/set - A JSON object containing the template variables and their values to be used for generating the email subject and body. The keys in this dictionary should correspond to the variable names used in the email templates, and the values will be substituted into the templates when generating the final email content. This allows for dynamic generation of email content based on a single template and a set of variable values, enabling mail merge functionality where multiple emails can be generated with different content from the same template.
    /// </summary>
    public Dictionary<string, object> Context { get; set; } = [];
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MailMergeModel object, initializes with specified parameters.
    /// </summary>
    public MailMergeModel() { }

    /// <summary>
    /// Creates a new instance of a MailMergeModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="to"></param>
    public MailMergeModel(IEnumerable<string> to)
    {
        this.To = [.. to];
    }

    /// <summary>
    /// Creates a new instance of a MailMergeModel object, initializes with specified parameters.
    /// </summary>
    /// <param name="to"></param>
    /// <param name="context"></param>
    public MailMergeModel(IEnumerable<string> to, Dictionary<string, object> context)
    {
        this.To = [.. to];
        this.Context = context;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Populates the Context dictionary with values from the specified UserModel. This is used to provide user-specific context for generating email content from templates. The UserModel properties are added to the Context dictionary with keys corresponding to the property names, allowing for easy substitution of user-specific values in email templates using Liquid syntax.
    /// </summary>
    /// <param name="user"></param>
    public MailMergeModel PopulateContextFromUser(TNO.API.Areas.Services.Models.User.UserModel? user)
    {
        var context = new Dictionary<string, object>() {
            { "id", user?.Id ?? 0 },
            { "firstName", user?.FirstName ?? "" },
            { "lastName", user?.LastName ?? "" },
        };
        this.Context = context;
        return this;
    }

    /// <summary>
    /// Populates the Context dictionary with values from the specified UserModel. This is used to provide user-specific context for generating email content from templates. The UserModel properties are added to the Context dictionary with keys corresponding to the property names, allowing for easy substitution of user-specific values in email templates using Liquid syntax.
    /// </summary>
    /// <param name="user"></param>
    public MailMergeModel PopulateContextFromUser(TNO.API.Areas.Services.Models.Notification.UserModel? user)
    {
        var context = new Dictionary<string, object>() {
            { "id", user?.Id ?? 0 },
            { "firstName", user?.FirstName ?? "" },
            { "lastName", user?.LastName ?? "" },
        };
        this.Context = context;
        return this;
    }

    /// <summary>
    /// Merges placeholders in the template string with values from the Context dictionary using Fluid Liquid template syntax. Supports placeholders in the format {{key}} where key is a key in the Context dictionary. Complex Liquid syntax including filters, loops, and conditionals are also supported. If the template cannot be parsed, the original template string is returned.
    /// </summary>
    /// <param name="template">The template string containing Liquid placeholders in the format {{key}}.</param>
    /// <returns>The template string with placeholders replaced by corresponding context values.</returns>
    public string Merge(string template)
    {
        if (string.IsNullOrEmpty(template) || Context == null || Context.Count == 0)
            return template ?? string.Empty;

        try
        {
            var fluidParser = new FluidParser();
            if (!fluidParser.TryParse(template, out var fluidTemplate, out var errors))
            {
                // If parsing fails, return the original template
                return template;
            }

            var templateContext = new TemplateContext(Context);
            var result = fluidTemplate.Render(templateContext);

            return result ?? string.Empty;
        }
        catch
        {
            // If rendering fails, return the original template
            return template ?? string.Empty;
        }
    }
    #endregion
}
