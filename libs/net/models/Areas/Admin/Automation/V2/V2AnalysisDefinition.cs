namespace TNO.API.Areas.Admin.Models.Automation.V2;

/// <summary>
/// V2AnalysisDefinition class, one named LLM prompt with a declared result shape.
/// A step declares as many analyses as it needs: one covering several properties shares a single
/// call, one per property keeps a complex prompt isolated. Actions reference results by name
/// ('analysisName.key'). An analysis runs at most once per item, and only when a reachable
/// action consumes it.
/// </summary>
public class V2AnalysisDefinition
{
    #region Properties
    /// <summary>
    /// get/set - Analysis name (unique within the step); actions reference '<name>.<key>'.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - The prompt: a library reference, inline text, or a reference plus an override.
    /// </summary>
    public V2PromptDefinition Prompt { get; set; } = new();

    /// <summary>
    /// get/set - The name of an earlier analysis in this step to continue as a conversation
    /// (the model sees the earlier exchange). Unchained analyses are independent single calls.
    /// </summary>
    public string? Chain { get; set; }

    /// <summary>
    /// get/set - The declared result shape: key -> type spec. Supported specs: 'string',
    /// 'string?', 'string[]', 'bool', 'int', 'int(min..max)'. The engine requests structured
    /// JSON and validates the response against these keys.
    /// </summary>
    public Dictionary<string, string> Returns { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// get/set - Raw mode: the response is kept as plain text instead of parsed JSON, and actions
    /// gate on it with confirmation statements. Used by migrated v1 actions so a migrated profile
    /// issues the same prompts and parses the same way it did before.
    /// </summary>
    public bool Raw { get; set; }

    /// <summary>
    /// get/set - Optional LLM override for this analysis.
    /// </summary>
    public int? LlmId { get; set; }
    #endregion
}

/// <summary>
/// V2PromptDefinition class, resolves prompt text from the profile's prompt library and/or
/// inline text. When both Ref and Override are set the override text is appended to the
/// referenced library text, so a step stores only its delta from the shared prompt.
/// </summary>
public class V2PromptDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The name of a prompt library entry.
    /// </summary>
    public string? Ref { get; set; }

    /// <summary>
    /// get/set - Inline prompt text (used alone, without a library reference).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// get/set - Text layered onto the referenced library entry.
    /// </summary>
    public string? Override { get; set; }
    #endregion
}
