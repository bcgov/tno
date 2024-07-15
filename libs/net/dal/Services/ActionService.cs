using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities.Models;
using TNO.Models.Filters;

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
            .Include(m => m.ContentTypes)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public Entities.Action? FindByName(string name)
    {
        return this.Context.Actions
            .Include(m => m.ContentTypes)
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }

    public override Entities.Action? FindById(int id)
    {
        return this.Context.Actions
            .Include(m => m.ContentTypes)
            .FirstOrDefault(c => c.Id == id);
    }

    public IPaged<Entities.Action> Find(ActionFilter filter)
    {
        var query = this.Context.Actions
            .Include(m => m.ContentTypes)
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

        var items = query?.ToArray() ?? Array.Empty<Entities.Action>();
        return new Paged<Entities.Action>(items, page, quantity, total);
    }

    /// <summary>
    /// Add the specified data action to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Entities.Action AddAndSave(Entities.Action entity)
    {
        entity.AddToContext(this.Context);
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data action in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Entities.Action UpdateAndSave(Entities.Action entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
        this.Context.UpdateContext(original, entity);
        return base.UpdateAndSave(entity);
    }
    #endregion
}
