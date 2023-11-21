using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

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
        return this.Context.MediaTypes
            .AsNoTracking()
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
