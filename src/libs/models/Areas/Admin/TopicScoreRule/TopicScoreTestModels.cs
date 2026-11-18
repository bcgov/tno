namespace TNO.API.Areas.Admin.Models.TopicScoreRule;

/// <summary>
/// TopicScoreTestRequestModel class, the content to test the topic score rules against: either a
/// saved content item, or the values to score.
/// </summary>
public class TopicScoreTestRequestModel
{
    #region Properties
    /// <summary>
    /// get/set - Test a saved content item. When set, the other values are ignored.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The series.
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The print section.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set - The print page ("A1").
    /// </summary>
    public string? Page { get; set; }

    /// <summary>
    /// get/set - Whether the content has an image.
    /// </summary>
    public bool HasImage { get; set; }

    /// <summary>
    /// get/set - The publish date and time (UTC).
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - The plain-text character count of the body.
    /// </summary>
    public int CharacterCount { get; set; }
    #endregion
}

/// <summary>
/// TopicScoreRuleEvaluationModel class, whether one rule matched and why not.
/// </summary>
public class TopicScoreRuleEvaluationModel
{
    #region Properties
    /// <summary>
    /// get/set - The rule.
    /// </summary>
    public int RuleId { get; set; }

    /// <summary>
    /// get/set - The rule's position within its source.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The rule's score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// get/set - Whether the rule is the one that matched.
    /// </summary>
    public bool IsMatch { get; set; }

    /// <summary>
    /// get/set - The first condition that failed (Series, Section, Page, Image, Time, Characters).
    /// </summary>
    public string? FailedCondition { get; set; }

    /// <summary>
    /// get/set - Why the rule did not match.
    /// </summary>
    public string? Reason { get; set; }
    #endregion
}

/// <summary>
/// TopicScoreTestResultModel class, the score the topic score rules give the tested content.
/// </summary>
public class TopicScoreTestResultModel
{
    #region Properties
    /// <summary>
    /// get/set - Whether the content is scored (its source or series uses topics, and it is not Image content).
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// get/set - The calculated score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// get/set - The rule that matched, or null.
    /// </summary>
    public int? RuleId { get; set; }

    /// <summary>
    /// get/set - No rule matched and the source default applied.
    /// </summary>
    public bool IsSourceDefault { get; set; }

    /// <summary>
    /// get/set - The values the rules were evaluated against.
    /// </summary>
    public TopicScoreTestRequestModel Input { get; set; } = new();

    /// <summary>
    /// get/set - Every rule of the source, in order.
    /// </summary>
    public IEnumerable<TopicScoreRuleEvaluationModel> Evaluations { get; set; } = Array.Empty<TopicScoreRuleEvaluationModel>();
    #endregion
}
