using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITopicScoreRuleService : IBaseService<TopicScoreRule, int>
{
    IEnumerable<TopicScoreRule> FindAll();

    /// <summary>
    /// Find the rules for the specified source in evaluation order.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    IEnumerable<TopicScoreRule> FindForSource(int sourceId);

    /// <summary>
    /// Save a new order for one source's rules.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="ruleIds">Every rule of the source, in the new order.</param>
    /// <returns></returns>
    IEnumerable<TopicScoreRule> Reorder(int sourceId, IEnumerable<int> ruleIds);
}
