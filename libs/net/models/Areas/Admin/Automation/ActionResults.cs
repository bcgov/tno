namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ActionResults class, the result every action publishes into the item scope under its own name
/// once it has been gated and dispatched. A later action in the same step reads it exactly like an
/// analysis answer: '&lt;action name&gt;.executed' as a boolean gate, or '&lt;action name&gt;.outcome'
/// compared to a specific value. An action that never ran (disabled, or after an abort) publishes
/// nothing, so a reference to it resolves to nothing and its gate fails.
/// </summary>
public static class ActionResults
{
    #region Keys
    /// <summary>The action's outcome, one of <see cref="OutcomeValues"/>.</summary>
    public const string Outcome = "outcome";

    /// <summary>True when the action's handler ran and did its work.</summary>
    public const string Executed = "executed";

    /// <summary>True when the handler ran but could not act (nothing matched, no value, disabled record).</summary>
    public const string Skipped = "skipped";

    /// <summary>True when the action threw.</summary>
    public const string Failed = "failed";

    /// <summary>True when a gate - the condition or the confirmation statement - stopped the action.</summary>
    public const string Blocked = "blocked";

    /// <summary>Every key an action publishes.</summary>
    public static readonly string[] Keys = { Outcome, Executed, Skipped, Failed, Blocked };

    /// <summary>The values '&lt;action name&gt;.outcome' can carry.</summary>
    public static readonly string[] OutcomeValues =
    {
        Outcomes.Executed, Outcomes.Skipped, Outcomes.Failed, Outcomes.ConditionFailed, Outcomes.NotConfirmed,
    };
    #endregion

    #region Methods
    /// <summary>
    /// Whether the reference names one of this result's keys ('publish.executed' against the
    /// action name 'publish').
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static bool IsKey(string key) => Keys.Contains(key, StringComparer.OrdinalIgnoreCase);
    #endregion
}
