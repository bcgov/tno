using TNO.Core.Json;

namespace TNO.Ches.Models
{
    /// <summary>
    /// EmailBodyTypes enum, provides email body type options.
    /// </summary>
    public enum EmailBodyTypes
    {
        [EnumValue("html")]
        Html = 0,
        [EnumValue("text")]
        Text = 1
    }
}
