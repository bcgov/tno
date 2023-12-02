using TNO.Core.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TNO.Ches.Models
{
    public interface IEmail: IEmailBase
    {
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
        /// get/set - if set, will delay sending until the set time
        /// </summary>
        [JsonConverter(typeof(MicrosecondEpochJsonConverter))]
        [JsonPropertyName("delayTS")]
        DateTime SendOn { get; set; }
    }
}
