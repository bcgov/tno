namespace TNO.Entities.Filters;

/// <summary>
/// FilterRule class, provides a way to either add a comparison rule, or a group of rules to a filter.
/// </summary>
public class FilterRule
{
    #region Properties
    /// <summary>
    /// get/set - The comparison rule to validate.
    /// Must have either a Comparison or an array of Rules.
    /// </summary>
    public FilterConditionRule? Comparison { get; set; }

    /// <summary>
    /// get/set - The logical operator to apply to the array of rules.
    /// </summary>
    public LogicalOperator? GroupOperator { get; set; }

    /// <summary>
    /// get/set - An array of rules to validate.
    /// </summary>
    public IEnumerable<FilterRule> Rules { get; set; } = Array.Empty<FilterRule>();
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of a FilterRule object.
    /// </summary>
    public FilterRule() { }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="groupOperator"></param>
    /// <param name="rule"></param>
    public FilterRule(LogicalOperator groupOperator, FilterRule rule)
    {
        this.GroupOperator = groupOperator;
        this.Rules = new List<FilterRule>(new[] { rule });
    }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="groupOperator"></param>
    /// <param name="rules"></param>
    public FilterRule(LogicalOperator groupOperator, IEnumerable<FilterRule> rules)
    {
        this.GroupOperator = groupOperator;
        this.Rules = rules;
    }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="rule"></param>
    public FilterRule(FilterConditionRule rule)
    {
        this.Comparison = rule;
    }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="comparison"></param>
    /// <param name="value"></param>
    public FilterRule(string field, ConditionalOperator comparison, string? value)
    {
        this.Comparison = new FilterConditionRule(field, comparison, value);
    }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="comparison"></param>
    /// <param name="values"></param>
    public FilterRule(string field, ConditionalOperator comparison, IEnumerable<string?> values)
    {
        this.Comparison = new FilterConditionRule(field, comparison, values);
    }

    /// <summary>
    /// Creates a new instance of a FilterRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="rule"></param>
    /// <param name="groupOperator"></param>
    /// <param name="rules"></param>
    public FilterRule(FilterConditionRule rule, LogicalOperator groupOperator, IEnumerable<FilterRule> rules)
    {
        this.Comparison = rule;
        this.GroupOperator = groupOperator;
        this.Rules = rules;
    }
    #endregion
}
