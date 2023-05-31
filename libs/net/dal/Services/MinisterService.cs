using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;

namespace TNO.DAL.Services;


public class MinisterService : BaseService<Minister, int>, IMinisterService {

    #region Constructors
    public MinisterService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Entities.Minister> FindAll()
    {
        return this.Context.Ministers
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion

}