using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class TopicService : BaseService<Topic, int>, ITopicService
{
    #region Properties
    #endregion

    #region Constructors
    public TopicService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TopicService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Add a topic. Only the migration marks the system topic.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Topic Add(Topic entity)
    {
        entity.IsSystem = false;
        return base.Add(entity);
    }

    /// <summary>
    /// Update a topic, keeping its system flag.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Topic Update(Topic entity)
    {
        entity.IsSystem = this.Context.Topics.AsNoTracking().Where(t => t.Id == entity.Id).Select(t => t.IsSystem).FirstOrDefault();
        return base.Update(entity);
    }

    /// <summary>
    /// Delete a topic. The system topic cannot be deleted.
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(Topic entity)
    {
        if (this.Context.Topics.AsNoTracking().Any(t => t.Id == entity.Id && t.IsSystem))
            throw new InvalidOperationException("The system topic cannot be deleted.");
        base.Delete(entity);
    }

    public IEnumerable<Topic> FindAll()
    {
        return this.Context.Topics
            .AsNoTracking()
            .OrderByDescending(a => a.TopicType)
            .ThenBy(a => a.SortOrder)
            .ThenBy(a => a.Name).ToArray();
    }

    public IPaged<Topic> Find(TopicFilter filter)
    {
        var query = this.Context.Topics
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));

        if (filter.TopicType.HasValue)
            query = query.Where(c => c.TopicType == filter.TopicType);

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(a => a.TopicType).ThenBy(c => c.SortOrder).ThenBy(c => c.Name);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<Topic>();
        return new Paged<Topic>(items, page, quantity, total);
    }
    #endregion
}
