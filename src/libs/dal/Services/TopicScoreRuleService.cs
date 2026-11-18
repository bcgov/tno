using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.DAL.Scoring;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TopicScoreRuleService : BaseService<TopicScoreRule, int>, ITopicScoreRuleService
{
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
            .ThenBy(a => a.Id).ToArray();
    }

    /// <summary>
    /// Find the rules for the specified source in evaluation order.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    public IEnumerable<TopicScoreRule> FindForSource(int sourceId)
    {
        return this.Context.TopicScoreRules
            .AsNoTracking()
            .Include(r => r.Series)
            .Where(r => r.SourceId == sourceId)
            .OrderBy(r => r.SortOrder)
            .ThenBy(r => r.Id)
            .ToArray();
    }

    /// <summary>
    /// Add a rule to the end of its source's rules.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override TopicScoreRule Add(TopicScoreRule entity)
    {
        Validate(entity);
        entity.SortOrder = NextSortOrder(entity.SourceId);
        return base.Add(entity);
    }

    /// <summary>
    /// Update a rule. A rule moved to another source goes to the end of that source's rules.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override TopicScoreRule Update(TopicScoreRule entity)
    {
        var original = this.Context.TopicScoreRules.AsNoTracking().FirstOrDefault(r => r.Id == entity.Id) ?? throw new NoContentException("Entity does not exist");
        Validate(entity);
        entity.SortOrder = original.SourceId == entity.SourceId ? original.SortOrder : NextSortOrder(entity.SourceId);
        if (original.Equals(entity)) return entity;
        return base.Update(entity);
    }

    /// <summary>
    /// Save a new order for one source's rules.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="ruleIds">Every rule of the source, in the new order.</param>
    /// <returns></returns>
    public IEnumerable<TopicScoreRule> Reorder(int sourceId, IEnumerable<int> ruleIds)
    {
        var order = ruleIds.ToArray();
        var rules = this.Context.TopicScoreRules.Where(r => r.SourceId == sourceId).ToArray();
        if (order.Length != rules.Length || order.Distinct().Count() != order.Length || !rules.All(r => order.Contains(r.Id)))
            throw new ArgumentException("The order must list every rule of the source exactly once.");

        for (var i = 0; i < order.Length; i++)
        {
            var rule = rules.First(r => r.Id == order[i]);
            if (rule.SortOrder != i) rule.SortOrder = i;
        }
        this.Context.UpdateCache<TopicScoreRule>();
        this.Context.CommitTransaction();
        return FindForSource(sourceId);
    }

    /// <summary>
    /// Validate a rule before it is saved.
    /// </summary>
    /// <param name="entity"></param>
    /// <exception cref="ArgumentException"></exception>
    private void Validate(TopicScoreRule entity)
    {
        var errors = TopicScoreCalculator.Validate(entity).ToList();
        if (!this.Context.Sources.Any(s => s.Id == entity.SourceId)) errors.Add("The source does not exist.");
        if (entity.SeriesId.HasValue)
        {
            var seriesSourceId = this.Context.Series.Where(s => s.Id == entity.SeriesId).Select(s => new { s.SourceId }).FirstOrDefault();
            if (seriesSourceId == null) errors.Add("The series does not exist.");
            else if (seriesSourceId.SourceId.HasValue && seriesSourceId.SourceId != entity.SourceId) errors.Add("The series does not belong to the source.");
        }
        if (errors.Count > 0) throw new ArgumentException(String.Join(" ", errors));
    }

    private int NextSortOrder(int sourceId)
    {
        return (this.Context.TopicScoreRules.Where(r => r.SourceId == sourceId).Max(r => (int?)r.SortOrder) ?? -1) + 1;
    }
    #endregion
}
