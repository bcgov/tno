using System.Text.Json.Serialization;

namespace TNO.Elastic.Models;

public class ValidateReason
{
    #region Properties
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("stack_trace")]
    public string? StackTrace { get; set; }

    [JsonPropertyName("caused_by")]
    public object? CausedBy { get; set; }

    [JsonPropertyName("root_cause")]
    public object[]? RootCause { get; set; }

    [JsonPropertyName("suppressed")]
    public object[]? Suppressed { get; set; }
    #endregion
}
