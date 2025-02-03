using System.Text.Json.Serialization;
using TNO.Core.Converters;

namespace TNO.Ches.Models
{
    /// <summary>
    /// EmailModel class, provides a model that represents and controls an email that will be sent.
    /// </summary>
    public class EmailModel : IEmail
    {
        #region Properties
        /// <summary>
        /// get/set - The email address that the message will be sent from.
        /// </summary>
        public string From { get; set; } = "";

        /// <summary>
        /// get/set - An array of email addresses to send the message to.
        /// </summary>
        public IEnumerable<string> To { get; set; } = [];

        /// <summary>
        /// get/set - An array of email addresses to send the message to.
        /// </summary>
        public IEnumerable<string> Bcc { get; set; } = [];

        /// <summary>
        /// get/set - An array of email addresses to send the message to.
        /// </summary>
        public IEnumerable<string> Cc { get; set; } = [];

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
        /// get/set - The email subject.
        /// </summary>
        public string Subject { get; set; } = "";

        /// <summary>
        /// get/set - The email body type.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailBodyTypes>))]
        public EmailBodyTypes BodyType { get; set; } = EmailBodyTypes.Html;

        /// <summary>
        /// get/set - The email body.
        /// </summary>
        public string Body { get; set; } = "";

        /// <summary>
        /// get/set - A tag to identify related messages.
        /// </summary>
        public string Tag { get; set; } = "";

        /// <summary>
        /// get/set - When the message will be sent.
        /// </summary>
        [JsonConverter(typeof(MicrosecondEpochJsonConverter))]
        [JsonPropertyName("delayTS")]
        public DateTime SendOn { get; set; }

        /// <summary>
        /// get/set - An array of attachments.
        /// </summary>
        public IEnumerable<IAttachment> Attachments { get; set; } = new List<AttachmentModel>();
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an EmailModel object.
        /// </summary>
        public EmailModel() { }

        /// <summary>
        /// Creates a new instance of an EmailModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public EmailModel(string from, string to, string subject, string body)
            : this(from, [to], subject, body)
        {
        }

        /// <summary>
        /// Creates a new instance of an EmailModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        public EmailModel(string from, string[] to, string subject, string body)
        {
            this.From = from;
            this.To = to;
            this.Subject = subject;
            this.Body = body;
        }
        #endregion
    }
}
