using System.Text.Json.Serialization;
using TNO.Core.Converters;

namespace TNO.Ches.Models
{
    /// <summary>
    /// EmailMergeModel class, provides a way to generate multiple emails with a single template.
    /// </summary>
    public class EmailMergeModel : IEmailMerge
    {
        #region Properties
        /// <summary>
        /// get/set - Who the emails will be from (i.e. First Last <first.last@email.com>).
        /// </summary>
        public string From { get; set; } = "";

        /// <summary>
        /// get/set - The email encoding.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailEncodings>))]
        public EmailEncodings Encoding { get; set; } = EmailEncodings.Utf8;

        /// <summary>
        /// get/set - The email priority.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailPriorities>))]
        public EmailPriorities Priority { get; set; } = EmailPriorities.Normal;

        /// <summary>
        /// get/set - The email body type.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailBodyTypes>))]
        public EmailBodyTypes BodyType { get; set; } = EmailBodyTypes.Html;

        /// <summary>
        /// get/set - The email subject (template).
        /// </summary>
        public string Subject { get; set; } = "";

        /// <summary>
        /// get/set - The email body (template).
        /// </summary>
        public string Body { get; set; } = "";

        /// <summary>
        /// get/set - A way to identify related emails.
        /// </summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// get/set - The context provides the template variables for each individual email.
        /// </summary>
        public IEnumerable<IEmailContext> Contexts { get; set; } = new List<EmailContextModel>();

        /// <summary>
        /// get/set - Attachments to include with the email.
        /// </summary>
        public IEnumerable<IAttachment> Attachments { get; set; } = new List<AttachmentModel>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an EmailMergeModel object.
        /// </summary>
        public EmailMergeModel() { }

        /// <summary>
        /// Creates a new instance of an EmailMergeModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="contexts"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public EmailMergeModel(string from, IEnumerable<IEmailContext> contexts, string subject, string body)
        {
            this.From = from;
            this.Contexts = contexts;
            this.Subject = subject;
            this.Body = body;
        }
        #endregion
    }
}
