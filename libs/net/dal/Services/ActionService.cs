using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class ActionService : BaseService<Entities.Action, int>, IActionService
{
    #region Properties
    #endregion

    #region Constructors
    public ActionService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ActionService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Action> FindAll()
    {
        return this.Context.Actions
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public Entities.Action? FindByName(string name)
    {
        return this.Context.Actions
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }

    public IPaged<Entities.Action> Find(ActionFilter filter)
    {
        var query = this.Context.Actions
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

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Entities.Action>();
        return new Paged<Entities.Action>(items, filter.Page, filter.Quantity, total);
    }
    #endregion
}
