using TNO.API.Areas.Services.Models.TopicScore;
using TNO.DAL.Models;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ITopicScoreService interface, calculates topic scores from the topic score rules. The API,
/// ingest, Content-Analysis acceptance, the rule tester, and bulk rescore share this one
/// implementation.
/// </summary>
public interface ITopicScoreService : IBaseService
{
    /// <summary>
    /// get - The time zone rule times are expressed in.
    /// </summary>
    TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// Calculate the score for the specified input using its source's rules and default score.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="evaluateAll">Explain every rule, not only those before the match.</param>
    /// <returns></returns>
    TopicScoreResult Calculate(TopicScoreInput input, bool evaluateAll = false);

    /// <summary>
    /// Build the scoring input for saved content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns>Null when the content does not exist.</returns>
    TopicScoreInput? GetInput(long contentId);

    /// <summary>
    /// Whether saved content is scored: its source or series uses topics and it is not Image content.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    bool IsEligible(long contentId);

    /// <summary>
    /// Whether content with these values is scored.
    /// </summary>
    /// <param name="contentType"></param>
    /// <param name="sourceId"></param>
    /// <param name="seriesId"></param>
    /// <returns></returns>
    bool IsEligible(ContentType contentType, int? sourceId, int? seriesId);

    /// <summary>
    /// Prepare the topics of content that is about to be added. When 'incomingScoresAreOverrides'
    /// (ingest), a topic arriving with a non-zero score keeps it as an override; every other score
    /// is calculated when the content is saved. Scored content that has no topics is given the
    /// system "Not Applicable" topic, so it counts in Event of the Day totals.
    /// </summary>
    /// <param name="content"></param>
    /// <param name="incomingScoresAreOverrides"></param>
    void PrepareNewContentTopics(Content content, bool incomingScoresAreOverrides);

    /// <summary>
    /// Give eligible content that has no topics the system "Not Applicable" topic when a rule or
    /// its source default scores it. The score itself is calculated when the content is saved.
    /// </summary>
    /// <param name="content"></param>
    void AddSystemTopicWhenScored(Content content);

    /// <summary>
    /// Calculate the score of each of the specified content items. Ineligible content scores 0.
    /// </summary>
    /// <param name="contentIds"></param>
    /// <returns>The calculated score per content ID.</returns>
    IDictionary<long, int> CalculateScores(IEnumerable<long> contentIds);

    /// <summary>
    /// Sources that use topics (the source or one of its series has 'UseInTopics'), with their
    /// rule count and default score.
    /// </summary>
    /// <returns></returns>
    IEnumerable<TopicScoreSourceSummary> FindSourceSummaries();

    /// <summary>
    /// Sections known for a source: those its rules use and those its content used in the past year.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    IEnumerable<string> FindKnownSections(int sourceId);

    /// <summary>
    /// Set the score used when none of a source's rules match.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="score">Null scores unmatched content 0.</param>
    /// <returns></returns>
    TopicScoreSourceSummary UpdateSourceDefaultScore(int sourceId, int? score);

    /// <summary>
    /// Disable topic scoring for a source and its series, preserving its rules and default score.
    /// </summary>
    /// <param name="sourceId"></param>
    void RemoveSource(int sourceId);

    /// <summary>
    /// Clear the override on a content topic so its score is recalculated.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="topicId"></param>
    /// <returns>The recalculated content topic.</returns>
    ContentTopic ResetOverride(long contentId, int topicId);

    /// <summary>
    /// Count the content in the range whose calculated score would change. Overridden scores are
    /// excluded.
    /// </summary>
    /// <param name="startOn">Published on or after (UTC).</param>
    /// <param name="endOn">Published before (UTC).</param>
    /// <param name="sourceIds">Limit to these sources; empty includes every source.</param>
    /// <returns></returns>
    TopicRescorePreview PreviewRescore(DateTime startOn, DateTime endOn, IEnumerable<int>? sourceIds);

    /// <summary>
    /// Record a new bulk rescore work order, counting the content in its range now. The Event
    /// Handler runs it a page at a time.
    /// </summary>
    /// <param name="startOn">Published on or after (UTC).</param>
    /// <param name="endOn">Published before (UTC).</param>
    /// <param name="sourceIds">Limit to these sources; empty includes every source.</param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    WorkOrder AddRescore(DateTime startOn, DateTime endOn, IEnumerable<int>? sourceIds, int? requestorId);

    /// <summary>
    /// Find a bulk rescore work order.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    WorkOrder? FindRescore(long id);

    /// <summary>
    /// The most recent bulk rescore work orders.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    IEnumerable<WorkOrder> FindRescores(int qty = 10);

    /// <summary>
    /// Recalculate the calculated scores of the next page of a bulk rescore, after the specified
    /// content ID. Content whose score changed is re-indexed once the page commits.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="afterContentId"></param>
    /// <param name="quantity"></param>
    /// <returns></returns>
    TopicRescorePageModel RescorePage(TopicRescoreConfigurationModel configuration, long afterContentId, int quantity = 200);
}
