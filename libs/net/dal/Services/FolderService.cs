using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
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
            .Include(f => f.ContentManyToMany)
            .Where(f => f.OwnerId == userId)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public override Folder Update(Folder entity)
    {
        var originalContents = this.Context.FolderContents.Where(fc => fc.FolderId == entity.Id).ToArray();
        var removeContent = originalContents.Except(entity.ContentManyToMany).ToArray();
        var removeContentIds = removeContent.Select(c => c.ContentId).ToArray();
        removeContent.ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        var addContent = new List<FolderContent>();
        entity.ContentManyToMany.ForEach(folderContent =>
        {
            var originalContent = originalContents.FirstOrDefault(rs => rs.ContentId == folderContent.ContentId);
            if (originalContent == null)
            {
                this.Context.Add(folderContent);
                addContent.Add(folderContent);
            }
            else
                originalContent.SortOrder = folderContent.SortOrder;
        });

        // Update all report instances that have not been sent, and that reference this folder.
        var sections = this.Context.ReportSections
            .Include(rs => rs.Report)
            .Where(rs => rs.FolderId == entity.Id &&
                rs.Report!.Instances.Any(i => i.SentOn == null))
            .ToArray();
        foreach (var section in sections)
        {
            // Find all report instances that have not been sent.
            var instances = this.Context.ReportInstances
                .Include(i => i.ContentManyToMany)
                .Where(i => i.ReportId == section.ReportId &&
                    i.SentOn == null)
                .ToArray();

            foreach (var instance in instances)
            {
                instance.ContentManyToMany.ForEach(ic =>
                {
                    // If content was removed from the folder, remove it from the instance section.
                    if (ic.SectionName == section.Name && removeContentIds.Contains(ic.ContentId))
                        this.Context.Remove(ic);
                });
                entity.ContentManyToMany.ForEach(fc =>
                {
                    // If new content has been added to the folder and it doesn't exist in the instance, then add it.
                    var add = !instance.ContentManyToMany.Any(ic => ic.ContentId == fc.ContentId && ic.SectionName == section.Name);
                    if (add) this.Context.Add(new ReportInstanceContent(instance.Id, fc.ContentId, section.Name, fc.SortOrder));
                });
            }
        }

        return base.Update(entity);
    }
    #endregion

}
