using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class IngestTypeService : BaseService<IngestType, int>, IIngestTypeService
{
    #region Properties
    #endregion

    #region Constructors
    public IngestTypeService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<IngestTypeService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<IngestType> FindAll()
    {
        return this.Context.IngestTypes
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public IPaged<IngestType> Find(IngestTypeFilter filter)
    {
        var query = this.Context.IngestTypes
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));

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

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<IngestType>();
        return new Paged<IngestType>(items, page, quantity, total);
    }

    public override IngestType? FindById(int id)
    {
        return this.Context.IngestTypes.FirstOrDefault(c => c.Id == id);
    }

    public IngestType? FindByName(string name)
    {
        return this.Context.IngestTypes.FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }
    #endregion
}
