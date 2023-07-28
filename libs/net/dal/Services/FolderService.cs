using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;


public class FolderService : BaseService<Folder, int>, IFolderService
{
    #region Constructors
    public FolderService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TagService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Folder> FindAll()
    {
        return this.Context.Folders
            .AsNoTracking()
            .Include(f => f.Owner)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public override Folder? FindById(int id)
    {
        return this.Context.Folders
            .Include(f => f.Owner)
            .Include(f => f.ContentManyToMany).ThenInclude(f => f.Content)
            .FirstOrDefault(f => f.Id == id);
    }

    public IEnumerable<Folder> FindMyFolders(int userId)
    {
        return this.Context.Folders
            .Include(f => f.Owner)
            .Include(f => f.ContentManyToMany).ThenInclude(f => f.Content)
            .Include(f => f.Content)
            .Where(f => f.OwnerId == userId)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }
    #endregion

}
