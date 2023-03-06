
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITopicScoreRuleService : IBaseService<TopicScoreRule, int>
{
    IEnumerable<TopicScoreRule> FindAll();

}
