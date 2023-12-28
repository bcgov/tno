using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TopicScoreRuleService : BaseService<TopicScoreRule, int>, ITopicScoreRuleService
{
    #region Properties
    ILogger<TopicScoreRuleService> logger;
    #endregion

    #region Constructors
    public TopicScoreRuleService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TopicScoreRuleService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        this.logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Update the specified 'entity' in the context so that it can be updated in the database.
    /// This operation does not commit the transaction to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override TopicScoreRule Update(TopicScoreRule entity) {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");

        if (!original.Equals(entity)) {
            logger.LogDebug($"Entity {entity.Id} unchanged, no need to update.");
            return base.Update(entity);
        }
        else
            return entity;
    }

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
