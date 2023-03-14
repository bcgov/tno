using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TopicScoreRuleService : BaseService<TopicScoreRule, int>, ITopicScoreRuleService
{
    #region Properties
    #endregion

    #region Constructors
    public TopicScoreRuleService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TopicScoreRuleService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<TopicScoreRule> FindAll()
    {
        return this.Context.TopicScoreRules
            .AsNoTracking()
            .OrderBy(a => a.Source!.Code)
            .ThenBy(a => a.Source!.Name)
            .ThenBy(a => a.SortOrder)
            .ThenBy(a => a.PageMin)
            .ThenBy(a => a.PageMax)
            .ThenBy(a => a.TimeMin)
            .ThenBy(a => a.TimeMax).ToArray();
    }
    #endregion
}
