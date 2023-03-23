using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

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
    public IEnumerable<Topic> FindAll()
    {
        return this.Context.Topics
            .AsNoTracking()
            .OrderBy(a => a.TopicType)
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

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Topic>();
        return new Paged<Topic>(items, filter.Page, filter.Quantity, total);
    }
    #endregion
}
