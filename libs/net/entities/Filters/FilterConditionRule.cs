namespace TNO.Entities.Filters;

/// <summary>
/// FilterConditionRule class, provides a way to express a conditional filter rule.
/// </summary>
public class FilterConditionRule
{
    #region Properties
    /// <summary>
    /// get/set - The path to the field (i.e. Headline) or (i.e. Topics.Name)
    /// </summary>
    public string Field { get; set; } = "";

    /// <summary>
    /// get/set - The operator to perform the comparison between the field value and the rule value.
    /// </summary>
    public ConditionalOperator ComparisonOperator { get; set; }

    /// <summary>
    /// get/set - The values to compare against.
    /// </summary>
    public IEnumerable<string?> Values { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructor
    /// <summary>
    /// Creates a new instance of a FilterConditionRule object.
    /// </summary>
    public FilterConditionRule() { }

    /// <summary>
    /// Creates a new instance of a FilterConditionRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="comparison"></param>
    /// <param name="value"></param>
    public FilterConditionRule(string field, ConditionalOperator comparison, string? value)
    {
        this.Field = field ?? throw new ArgumentNullException(nameof(field));
        this.ComparisonOperator = comparison;
        this.Values = new List<string?>(new[] { value });
    }

    /// <summary>
    /// Creates a new instance of a FilterConditionRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="field"></param>
    /// <param name="comparison"></param>
    /// <param name="values"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public FilterConditionRule(string field, ConditionalOperator comparison, IEnumerable<string?> values)
    {
        this.Field = field ?? throw new ArgumentNullException(nameof(field));
        this.ComparisonOperator = comparison;
        this.Values = values;
    }
    #endregion
}
