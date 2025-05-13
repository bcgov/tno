using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class MediaAnalyticsService : BaseService<MediaAnalytics, int>,IMediaAnalyticsService
{
    #region Properties
    #endregion

    #region Constructors
    public MediaAnalyticsService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public async Task<IEnumerable<MediaAnalytics>> GetAllInformation()
    {
        return await this.Context.MediaAnalyticsInfo.ToListAsync();
    }
    public IPaged<MediaAnalytics> Find(MediaAnalyticsFilter filter)
    {
        var query = this.Context.MediaAnalyticsInfo
            .Include(c => c.MediaTypeId)
            .Include(c => c.SourceId)
            
            .AsQueryable();

        var predicate = PredicateBuilder.New<MediaAnalytics>(true);

        if (filter.MediaTypeId.HasValue)
            query = query.Where(c => c.MediaTypeId == filter.MediaTypeId);
        if (filter.SourceId.HasValue)
            query = query.Where(c => c.SourceId == filter.SourceId);
       

        query = query.Where(predicate);
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
            query = query.OrderBy(u => u.PublishedOn).OrderBy(u => u.PublishedOn).ThenBy(u => u.PublishedOn).ThenBy(u => u.PublishedOn);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query
            .Skip(skip)
            .Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<MediaAnalytics>();
        return new Paged<MediaAnalytics>(items, page, quantity, total);
    }

    #endregion
}
