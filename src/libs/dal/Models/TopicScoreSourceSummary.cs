namespace TNO.DAL.Models;

/// <summary>
/// TopicScoreSourceSummary record, a source that uses topics with its topic score rule summary.
/// </summary>
/// <param name="Id">The source.</param>
/// <param name="Code">The source code.</param>
/// <param name="Name">The source name.</param>
/// <param name="UseInTopics">Whether the source itself uses topics.</param>
/// <param name="SeriesUseInTopics">Whether one of the source's series uses topics.</param>
/// <param name="RuleCount">The number of rules.</param>
/// <param name="TopicDefaultScore">The score used when no rule matches.</param>
public record TopicScoreSourceSummary(int Id, string Code, string Name, bool UseInTopics, bool SeriesUseInTopics, int RuleCount, int? TopicDefaultScore);

/// <summary>
/// TopicRescorePreview record, what a rescore would change.
/// </summary>
/// <param name="Total">Content in the range.</param>
/// <param name="Changed">Content whose calculated score would change.</param>
public record TopicRescorePreview(int Total, int Changed);
