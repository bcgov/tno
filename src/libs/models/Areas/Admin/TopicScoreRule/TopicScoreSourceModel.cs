namespace TNO.API.Areas.Admin.Models.TopicScoreRule;

/// <summary>
/// TopicScoreSourceModel class, a source that uses topics with its topic score rule summary.
/// </summary>
public class TopicScoreSourceModel
{
    #region Properties
    /// <summary>
    /// get/set - The source.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The source code.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - The source name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Whether the source itself uses topics.
    /// </summary>
    public bool UseInTopics { get; set; }

    /// <summary>
    /// get/set - Whether one of the source's series uses topics.
    /// </summary>
    public bool SeriesUseInTopics { get; set; }

    /// <summary>
    /// get/set - The number of rules.
    /// </summary>
    public int RuleCount { get; set; }

    /// <summary>
    /// get/set - The score used when no rule matches. Null scores unmatched content 0.
    /// </summary>
    public int? TopicDefaultScore { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreSourceModel.
    /// </summary>
    public TopicScoreSourceModel() { }

    #endregion
}
