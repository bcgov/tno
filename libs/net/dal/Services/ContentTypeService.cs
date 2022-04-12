using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class ContentTypeService : BaseService<ContentType, int>, IContentTypeService
{
    #region Properties
    #endregion

    #region Constructors
    public ContentTypeService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ContentTypeService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<ContentType> FindAll()
    {
        return this.Context.ContentTypes.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
