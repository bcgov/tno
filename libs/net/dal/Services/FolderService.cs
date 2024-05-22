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
        return FindById(id, false);
    }

    public Folder? FindById(int id, bool includeContent = false)
    {
        var query = this.Context.Folders
            .Include(f => f.Owner)
            .Include(f => f.Filter)
            .Include(f => f.Events).ThenInclude(f => f.Schedule)
            .Where(f => f.Id == id);

        if (includeContent)
            query = query.Include(i => i.ContentManyToMany).ThenInclude(c => c.Content).ThenInclude(c => c!.Topics);
        else
            query = query.Include(i => i.ContentManyToMany);

        return query.FirstOrDefault();
    }

    public IEnumerable<Folder> FindMyFolders(int userId)
    {
        return this.Context.Folders
            .Include(f => f.Owner)
            .Include(f => f.ContentManyToMany)
            .Include(f => f.Events).ThenInclude(f => f.Schedule)
            .Where(f => f.OwnerId == userId)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public IEnumerable<FolderContent> GetContentInFolder(int folderId)
    {
        return this.Context.FolderContents
            .Include(fc => fc.Content!.Source)
            .Include(fc => fc.Content!.MediaType)
            .Include(fc => fc.Content!.Series)
            .Include(fc => fc.Content!.Contributor)
            .Include(fc => fc.Content!.Owner)
            .Include(fc => fc.Content!.License)
            .Include(fc => fc.Content!.TonePools)
            .Include(fc => fc.Content!.Topics)
            .Where(fc => fc.FolderId == folderId)
            .OrderBy(fc => fc.SortOrder).ThenByDescending(fc => fc.Content!.PublishedOn)
            .ToArray();
    }

    public override Folder Add(Folder entity)
    {
        if (entity.Events != null)
        {
            foreach (var folderEvent in entity.Events)
            {
                if (folderEvent.Schedule != null)
                {
                    folderEvent.Folder = entity;
                    this.Context.Add(folderEvent.Schedule);
                    this.Context.Add(folderEvent);
                }
            }
        }

        return base.Add(entity);
    }

    public Folder UpdateAndSave(Folder entity, bool updateContent)
    {
        var folder = Update(entity, updateContent);
        this.CommitTransaction();
        return folder;
    }

    public Folder Update(Folder entity, bool updateContent)
    {
        var original = this.Context.Folders.FirstOrDefault(f => f.Id == entity.Id) ?? throw new NoContentException();

        original.Name = entity.Name;
        original.Description = entity.Description;
        original.SortOrder = entity.SortOrder;
        original.IsEnabled = entity.IsEnabled;
        original.FilterId = entity.FilterId;
        original.OwnerId = entity.OwnerId;
        original.Settings = entity.Settings;
        original.UpdatedBy = entity.UpdatedBy;
        original.UpdatedOn = entity.UpdatedOn;
        original.Version = entity.Version;
        if (entity.Events.Any())
        {
			foreach (var folderEvent in entity.Events)
            {
                folderEvent.FolderId = entity.Id;
                if (folderEvent.Id == 0)
                {
					if (folderEvent.ScheduleId == 0 && folderEvent.Schedule != null)
                    {
                        this.Context.Add(folderEvent.Schedule);
                    }
                    this.Context.Add(folderEvent);
                }
                else
                {
                    if (folderEvent.Schedule != null)
                    {
                        this.Context.Update(folderEvent.Schedule);
                    }
                    this.Context.Update(folderEvent);
                }
            }
        }


        if (updateContent)
        {
            var originalContents = this.Context.FolderContents.Where(fc => fc.FolderId == entity.Id).ToArray();
            var removeContent = originalContents.Except(entity.ContentManyToMany).ToArray();
            var removeContentIds = removeContent.Select(c => c.ContentId).ToArray();
            removeContent.ForEach(s =>
            {
                this.Context.Entry(s).State = EntityState.Deleted;
            });
            entity.ContentManyToMany.ForEach(folderContent =>
            {
                var originalContent = originalContents.FirstOrDefault(rs => rs.ContentId == folderContent.ContentId);
                if (originalContent == null)
                {
                    this.Context.Add(folderContent);
                }
                else
                {
                    originalContent.SortOrder = folderContent.SortOrder;
                    this.Context.Update(originalContent);
                }
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
        }

        return base.Update(original);
    }

    public override Folder Update(Folder entity)
    {
        return Update(entity, true);
    }

    /// <summary>
    /// Remove the specified content from all folders.
    /// </summary>
    /// <param name="contentId"></param>
    public void RemoveContentFromFolders(long contentId)
    {
        this.Context.Database.ExecuteSql($"DELETE FROM public.folder_content WHERE content_id={contentId};");
    }

    /// <summary>
    /// Get all folders that have enabled filters.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Folder> GetFoldersWithFilters()
    {
        return this.Context.Folders
            .Include(f => f.Filter)
            .Include(f => f.Events).ThenInclude(f => f.Schedule)
            .Where(f => f.FilterId != null && f.IsEnabled && f.Filter!.IsEnabled)
            .ToArray();
    }


    /// <summary>
    /// Add the specified content to the specified folder.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="folderId"></param>
    /// <param name="toBottom"></param>
    public void AddContentToFolder(long contentId, int folderId, bool toBottom = true)
    {
        var folderContent = this.Context.FolderContents.Where(fc => fc.FolderId == folderId).ToArray();
        if (!folderContent.Any(c => c.ContentId == contentId))
        {
            var sortOrder = toBottom && folderContent.Length > 0 ? folderContent.Max(c => c.SortOrder) + 1 : 0;
            this.Context.Add(new FolderContent(folderId, contentId, sortOrder));

            // When content is added to the top everything needs to be moved down.
            if (!toBottom)
            {
                foreach (var content in folderContent)
                {
                    content.SortOrder++;
                }
                this.Context.UpdateRange(folderContent);
            }
            this.CommitTransaction();
        }
    }

    /// <summary>
    /// Clean content from the folder based on the folder's configuration settings.
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="NoContentException"></exception>
    public void CleanFolder(int id)
    {
        var folder = this.Context.Folders.Find(id) ?? throw new NoContentException();
        var keepAgeLimit = folder.Settings.GetElementValue<int>(".keepAgeLimit", 0);
        var now = DateTime.UtcNow.AddDays(keepAgeLimit * -1);

        if (keepAgeLimit == 0)
            this.Context.Database.ExecuteSql($"DELETE FROM public.folder_content WHERE \"folder_id\"={id};");
        else
        {
            var sqlParams = new object[] {
                new Npgsql.NpgsqlParameter("folderId", id),
                new Npgsql.NpgsqlParameter("publishedOn", now),
            };
            this.Context.Database.ExecuteSqlRaw("DELETE FROM public.folder_content WHERE \"content_id\" IN (SELECT c.\"id\" FROM public.content c JOIN public.folder_content fc ON fc.\"folder_id\" = @folderId AND c.\"published_on\" < @publishedOn);", sqlParams);
        }
    }
    #endregion

}
