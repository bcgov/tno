using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// ContributorService class, provides a way to perform CRUD methods on contributor in the database.
/// </summary>
public class ContributorService : BaseService<Contributor, int>, IContributorService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContributorService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public ContributorService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ContributorService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all contributor.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Contributor> FindAll()
    {
        return this.Context.Contributors
            .AsNoTracking()
            .Include(s => s.Source)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToArray();
    }

    /// <summary>
    /// Find the contributor for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Contributor? FindById(int id)
    {
        return this.Context.Contributors
            .Include(s => s.Source)
            .FirstOrDefault(s => s.Id == id);
    }
    #endregion
}
