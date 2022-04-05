using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

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
    #endregion
}
