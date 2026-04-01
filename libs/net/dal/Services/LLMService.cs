using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class LLMService : BaseService<Entities.LLM, int>, ILLMService
{
    #region Properties
    #endregion

    #region Constructors
    public LLMService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<LLMService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.LLM> FindAll()
    {
        return this.Context.LLM
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public Entities.LLM? FindByName(string name)
    {
        return this.Context.LLM
            .FirstOrDefault(c => c.Name.ToLower() == name.ToLower());
    }

    public override Entities.LLM? FindById(int id)
    {
        return this.Context.LLM
            .FirstOrDefault(c => c.Id == id);
    }

    public IPaged<Entities.LLM> Find(LLMFilter filter)
    {
        var query = this.Context.LLM
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));
        if (filter.IsPublic.HasValue)
            query = query.Where(c => c.IsPublic == filter.IsPublic);

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

        var items = query?.ToArray() ?? Array.Empty<Entities.LLM>();
        return new Paged<Entities.LLM>(items, page, quantity, total);
    }

    /// <summary>
    /// Add the specified data aiModel to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Entities.LLM AddAndSave(Entities.LLM entity)
    {
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data aiModel in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Entities.LLM UpdateAndSave(Entities.LLM entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
        return base.UpdateAndSave(entity);
    }
    #endregion
}
