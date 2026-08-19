using System.Text.Json;
using System.Text.Json.Serialization;

namespace TNO.API.Areas.Admin.Models.Automation.V2;

/// <summary>
/// AutomationDefinition class, the root of a v2 profile definition document.
/// Stored as a JSON document on the profile (automation_profile.definition) and executed by the
/// v2 engine when the profile's SchemaVersion is 2 or higher.
/// </summary>
public class AutomationDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The prompt library: named prompt text shared by steps and analyses via
    /// prompt.ref. Shared text lives once; steps store only their overrides.
    /// </summary>
    public Dictionary<string, string> Prompts { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// get/set - When accumulated content changes are written to the database:
    /// 'end-of-run' (default; one update+index per dirty item) or 'end-of-step'.
    /// Steps may override.
    /// </summary>
    public string SaveMode { get; set; } = V2SaveModes.EndOfRun;

    /// <summary>
    /// get/set - The ordered steps of the profile, grouped by lifecycle phase
    /// (init runs once first, process steps iterate content, complete runs once last).
    /// </summary>
    public List<V2StepDefinition> Steps { get; set; } = new();
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
/// The lifecycle phases a step can declare.
/// </summary>
public static class V2Phases
{
    public const string Init = "init";
    public const string Process = "process";
    public const string Complete = "complete";
    public static readonly string[] All = { Init, Process, Complete };
}

/// <summary>
/// The flush modes for accumulated content changes.
/// </summary>
public static class V2SaveModes
{
    public const string EndOfRun = "end-of-run";
    public const string EndOfStep = "end-of-step";
    public static readonly string[] All = { EndOfRun, EndOfStep };
}

/// <summary>
/// The outcomes a run log entry can record.
/// </summary>
public static class V2Outcomes
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
