namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ActionResults class, the result every action publishes into the item scope under its own name
/// once it has been gated and dispatched. A later action in the same step reads it exactly like an
/// analysis answer: '&lt;action name&gt;.ran' or '&lt;action name&gt;.failed' as a boolean gate, or
/// '&lt;action name&gt;.value' compared with an operator. An action that never ran (disabled, or
/// after an abort) publishes nothing, so a reference to it resolves to nothing: a positive gate on
/// it fails and a negated one passes.
/// </summary>
public static class ActionResults
{
    #region Keys
    /// <summary>
    /// True when the action did its work: its condition and confirmation passed and the handler
    /// acted. False when a gate stopped it, when its turn came and it had nothing to act on (no
    /// target, no resolved value, nothing matched), or when it failed.
    /// </summary>
    public const string Ran = "ran";

    /// <summary>True when the action threw or the work it attempted failed.</summary>
    public const string Failed = "failed";

    /// <summary>
    /// What the action produced, when it produced something: the value a content action wrote,
    /// a score, a dedupe's matched content id, the count a collection or search action ended with.
    /// Null when the action produced nothing or did not run.
    /// </summary>
    public const string Value = "value";

    /// <summary>Every key an action publishes.</summary>
    public static readonly string[] Keys = { Ran, Failed, Value };

    /// <summary>The keys that read as a yes/no gate on their own; the rest need an operator.</summary>
    public static readonly string[] BooleanKeys = { Ran, Failed };
    #endregion

    #region Methods
    /// <summary>
    /// Whether the reference names one of this result's keys ('publish.ran' against the
    /// action name 'publish').
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool IsKey(string key) => Keys.Contains(key, StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether the key reads as a yes/no gate without an operator.</summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool IsBooleanKey(string key) => BooleanKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
    #endregion
}
