using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationDefinition class, the root of a profile definition document.
/// Stored as a JSON document on the profile (automation_profile.definition) and executed by the
/// v2 engine when the profile's SchemaVersion is 2 or higher.
/// </summary>
public class AutomationDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The prompt library: named prompt entries shared by steps and analyses via
    /// prompt.ref. Shared text lives once; steps store only their overrides. Each entry
    /// serializes as a bare string when it has no description, so legacy documents stay valid.
    /// </summary>
    public Dictionary<string, PromptEntry> Prompts { get; set; } = new(StringComparer.OrdinalIgnoreCase);


    /// <summary>
    /// get/set - The ordered steps of the profile, grouped by lifecycle phase
    /// (init runs once first, process steps iterate content, complete runs once last).
    /// </summary>
    public List<StepDefinition> Steps { get; set; } = new();
    #endregion

    #region Methods
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    /// <summary>
    /// Parse a definition document from raw JSON.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static AutomationDefinition Parse(string json)
    {
        return JsonSerializer.Deserialize<AutomationDefinition>(json, _options)
            ?? throw new JsonException("The definition document is empty.");
    }

    /// <summary>
    /// Serialize this definition to JSON (camelCase, nulls omitted).
    /// </summary>
    /// <returns></returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, _options);
    }
    #endregion
}

/// <summary>
/// PromptEntry class, one prompt library entry: its text and an optional description shown in
/// the editor's library table. Reads/writes as a bare JSON string when there is no description.
/// </summary>
[JsonConverter(typeof(PromptEntryConverter))]
public class PromptEntry
{
    #region Properties
    /// <summary>
    /// get/set - The prompt text.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// get/set - What the prompt is for, shown in the library table.
    /// </summary>
    public string? Description { get; set; }
    #endregion
}

/// <summary>
/// PromptEntryConverter class, accepts both entry shapes - a bare string (legacy) or an object
/// with text/description - and writes the compact string form back when possible.
/// </summary>
public class PromptEntryConverter : JsonConverter<PromptEntry>
{
    public override PromptEntry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return new PromptEntry { Text = reader.GetString() ?? "" };

        var entry = new PromptEntry();
        if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("A prompt entry must be a string or an object.");
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName) continue;
            var property = reader.GetString() ?? "";
            reader.Read();
            if (property.Equals("text", StringComparison.OrdinalIgnoreCase))
                entry.Text = reader.TokenType == JsonTokenType.String ? reader.GetString() ?? "" : "";
            else if (property.Equals("description", StringComparison.OrdinalIgnoreCase))
                entry.Description = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
            else reader.Skip();
        }
        return entry;
    }

    public override void Write(Utf8JsonWriter writer, PromptEntry value, JsonSerializerOptions options)
    {
        if (string.IsNullOrWhiteSpace(value.Description))
        {
            writer.WriteStringValue(value.Text);
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("text", value.Text);
        writer.WriteString("description", value.Description);
        writer.WriteEndObject();
    }
}

/// <summary>
/// The lifecycle phases a step can declare.
/// </summary>
public static class AutomationPhases
{
    public const string Init = "init";
    public const string Process = "process";
    public const string Complete = "complete";
    public static readonly string[] All = { Init, Process, Complete };
}

/// <summary>
/// The outcomes a run log entry can record.
/// </summary>
public static class Outcomes
{
    public const string Confirmed = "confirmed";
    public const string NotConfirmed = "not-confirmed";
    public const string ConditionPassed = "condition-passed";
    public const string ConditionFailed = "condition-failed";
    public const string Executed = "executed";
    public const string Skipped = "skipped";
    public const string Excluded = "excluded";
    public const string Aborted = "aborted";
    public const string Failed = "failed";
    public const string Flushed = "flushed";
    public const string Explain = "explain";
    public const string Info = "info";
}
