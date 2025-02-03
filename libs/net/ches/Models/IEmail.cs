using System.Text.Json.Serialization;
using TNO.Core.Converters;

namespace TNO.Ches.Models
{
    public interface IEmail
    {
        /// <summary>
        /// get/set - Who the email are from (i.e. First Last <first.last@email.com>).
        /// </summary>
        string From { get; set; }

        /// <summary>
        /// get/set - Email addresses to go in the To: field
        /// </summary>
        IEnumerable<string> To { get; set; }

        /// <summary>
        /// get/set - Email addresses to go in the CC: field
        /// </summary>
        IEnumerable<string> Cc { get; set; }

        /// <summary>
        /// get/set - Email addresses to go in the BCC: field
        /// </summary>
        IEnumerable<string> Bcc { get; set; }

        /// <summary>
        /// get/set - The email body type.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailBodyTypes>))]
        EmailBodyTypes BodyType { get; set; }

        /// <summary>
        /// get/set - The email encoding.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailEncodings>))]
        EmailEncodings Encoding { get; set; }

        /// <summary>
        /// get/set - The email priority.
        /// </summary>
        [JsonConverter(typeof(EnumValueJsonConverter<EmailPriorities>))]
        EmailPriorities Priority { get; set; }

        /// <summary>
        /// get/set - The email subject (template).
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// get/set - The email body (template).
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// get/set - A way to identify related email.
        /// </summary>
        string Tag { get; set; }

        /// <summary>
        /// get/set - if set, will delay sending until the set time
        /// </summary>
        [JsonConverter(typeof(MicrosecondEpochJsonConverter))]
        [JsonPropertyName("delayTS")]
        DateTime SendOn { get; set; }

        /// <summary>
        /// get/set - An array of attachments.
        /// </summary>
        IEnumerable<IAttachment> Attachments { get; set; }
    }
}
