using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class MediaTypeService : BaseService<MediaType, int>, IMediaTypeService
{
    #region Properties
    #endregion

    #region Constructors
    public MediaTypeService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<MediaTypeService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<MediaType> FindAll()
    {
        return this.Context.MediaTypes.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public IPaged<MediaType> Find(MediaTypeFilter filter)
    {
        var query = this.Context.MediaTypes
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name, $"%{filter.Name}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => c.Description == filter.Description);

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
            query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<MediaType>();
        return new Paged<MediaType>(items, filter.Page, filter.Quantity, total);
    }

    public override MediaType? FindById(int id)
    {
        return this.Context.MediaTypes.FirstOrDefault(c => c.Id == id);
    }

    public MediaType? FindByName(string name)
    {
        return this.Context.MediaTypes.FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }
    #endregion
}
