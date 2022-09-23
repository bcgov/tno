using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class CategoryService : BaseService<Category, int>, ICategoryService
{
    #region Properties
    #endregion

    #region Constructors
    public CategoryService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<CategoryService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Category> FindAll()
    {
        return this.Context.Categories.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public IPaged<Category> Find(CategoryFilter filter)
    {
        var query = this.Context.Categories
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));

        if (filter.CategoryType.HasValue)
            query = query.Where(c => c.CategoryType == filter.CategoryType);

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

        var items = query?.ToArray() ?? Array.Empty<Category>();
        return new Paged<Category>(items, filter.Page, filter.Quantity, total);
    }
    #endregion
}
