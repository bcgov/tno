using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using TNO.Core.Converters;

namespace TNO.Ches.Models
{
    /// <summary>
    /// EmailContextModel class, provides a way to generate multiple emails from the same template.
    /// </summary>
    public class EmailContextModel : IEmailContext
    {
        #region Properties
        /// <summary>
        /// get/set - An array of email addresses the email will be sent to.
        /// </summary>
        public IEnumerable<string> To { get; set; } = [];

        /// <summary>
        /// get/set - An array of email addresses that the email will be carbon-copied.
        /// </summary>
        public IEnumerable<string> Cc { get; set; } = [];

        /// <summary>
        /// get/set - An array of email addresses that the email will be blind carbon-copied.
        /// </summary>
        public IEnumerable<string> Bcc { get; set; } = [];

        /// <summary>
        /// get/set - A structure that provides the template variables values.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = [];

        /// <summary>
        /// get/set - When the email will be sent.
        /// </summary>
        [JsonConverter(typeof(MicrosecondEpochJsonConverter))]
        [JsonPropertyName("delayTS")]
        public DateTime SendOn { get; set; }

        /// <summary>
        /// get/set - A way to identify related emails.
        /// </summary>
        public string Tag { get; set; } = "";
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an EmailContextModel object.
        /// </summary>
        public EmailContextModel() { }

        /// <summary>
        /// Creates a new instance of an EmailContextModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="context"></param>
        /// <param name="sendOn"></param>
        public EmailContextModel(IEnumerable<string> to, Dictionary<string, object> context, DateTime sendOn)
        {
            this.To = to;
            this.Context = context;
            this.SendOn = sendOn;
        }

        /// <summary>
        /// Creates a new instance of an EmailContextModel object, initializes with specified parameters.
        /// </summary>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="context"></param>
        /// <param name="sendOn"></param>
        public EmailContextModel(IEnumerable<string> to, IEnumerable<string> cc, IEnumerable<string> bcc, Dictionary<string, object> context, DateTime sendOn)
            : this(to, context, sendOn)
        {
            this.Cc = cc;
            this.Bcc = bcc;
        }
        #endregion
    }
}
