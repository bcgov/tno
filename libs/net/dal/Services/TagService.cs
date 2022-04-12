using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TagService : BaseService<Tag, string>, ITagService
{
    #region Properties
    #endregion

    #region Constructors
    public TagService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Tag> FindAll()
    {
        return this.Context.Tags.OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion
}
