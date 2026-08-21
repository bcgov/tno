using System.Text.Json;

namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// ConditionDefinition class, a declarative gate on whether an action runs.
/// One of three shapes:
/// - a leaf: { field, op, value } evaluated against the item's working copy;
/// - a combinator: { all: [...] }, { any: [...] }, or { not: {...} };
/// - an analysis-result gate: { from: "analysisName.key" } (a boolean the LLM answered).
/// A failing condition prevents any prompt associated with the action from being sent.
/// There is no expression language - only these shapes.
/// </summary>
public class ConditionDefinition
{
    #region Properties
    /// <summary>
    /// get/set - The working-copy field a leaf condition reads (e.g. 'body', 'page', 'section').
    /// </summary>
    public string? Field { get; set; }

    /// <summary>
    /// get/set - The leaf operator; see <see cref="ConditionOps"/>.
    /// </summary>
    public string? Op { get; set; }

    /// <summary>
    /// get/set - The compared value: a string, number, boolean, or array (for in/notIn).
    /// </summary>
    public JsonElement? Value { get; set; }

    /// <summary>
    /// get/set - Every child must pass.
    /// </summary>
    public List<ConditionDefinition>? All { get; set; }

    /// <summary>
    /// get/set - At least one child must pass.
    /// </summary>
    public List<ConditionDefinition>? Any { get; set; }

    /// <summary>
    /// get/set - The child must fail.
    /// </summary>
    public ConditionDefinition? Not { get; set; }

    /// <summary>
    /// get/set - An analysis-result gate: 'analysisName.key' naming a boolean the LLM answered.
    /// </summary>
    public string? From { get; set; }
    #endregion
}

/// <summary>
/// The operators a leaf condition supports.
/// </summary>
public static class ConditionOps
{
    public const string Exists = "exists";
    public const string IsEmpty = "isEmpty";
    public const string Equals_ = "equals";
    public const string NotEquals = "notEquals";
    public const string In = "in";
    public const string NotIn = "notIn";
    public const string Contains = "contains";
    public const string StartsWith = "startsWith";
    public const string Matches = "matches";
    public const string LengthLessThan = "lengthLessThan";
    public const string LengthGreaterThan = "lengthGreaterThan";
    public const string GreaterThan = "greaterThan";
    public const string LessThan = "lessThan";
    public const string HasTag = "hasTag";
    public const string HasAction = "hasAction";
    public const string StatusIs = "statusIs";

    public static readonly string[] All =
    {
        Exists, IsEmpty, Equals_, NotEquals, In, NotIn, Contains, StartsWith, Matches,
        LengthLessThan, LengthGreaterThan, GreaterThan, LessThan, HasTag, HasAction, StatusIs,
    };
}
