using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class AVOverviewInstanceService : BaseService<AVOverviewInstance, long>, IAVOverviewInstanceService
{
    #region Variables
    #endregion

    #region Constructors
    public AVOverviewInstanceService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public override AVOverviewInstance? FindById(long id)
    {
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .Include(i => i.Template)
            .Include(i => i.Sections).ThenInclude(s => s.Source)
            .Include(i => i.Sections).ThenInclude(s => s.Series)
            .Include(i => i.Sections).ThenInclude(s => s.Items).ThenInclude(i => i.Content).ThenInclude(c => c!.FileReferences)
            .FirstOrDefault(i => i.Id == id);
    }

    /// <summary>
    /// Return the last evening overview instance for the specified date.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    public AVOverviewInstance? FindByDate(DateTime publishedOn)
    {
        var startDate = new DateTime(publishedOn.Year, publishedOn.Month, publishedOn.Day, 0, 0, 0, DateTimeKind.Utc);
        var endDate = new DateTime(publishedOn.Year, publishedOn.Month, publishedOn.Day, 23, 59, 59, DateTimeKind.Utc);
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .Include(i => i.Template)
            .Include(i => i.Sections).ThenInclude(s => s.Source)
            .Include(i => i.Sections).ThenInclude(s => s.Series)
            .Include(i => i.Sections).ThenInclude(s => s.Items).ThenInclude(i => i.Content).ThenInclude(c => c!.FileReferences)
            .OrderByDescending(r => r.PublishedOn)
            .Where(i => i.PublishedOn >= startDate && i.PublishedOn <= endDate)
            .FirstOrDefault();
    }

    /// <summary>
    /// Return the latest published evening overview instance.
    /// </summary>
    /// <returns></returns>
    public AVOverviewInstance? FindLatest()
    {
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .Include(i => i.Template)
            .Include(i => i.Sections).ThenInclude(s => s.Source)
            .Include(i => i.Sections).ThenInclude(s => s.Series)
            .Include(i => i.Sections).ThenInclude(s => s.Items).ThenInclude(i => i.Content).ThenInclude(c => c!.FileReferences)
            .OrderByDescending(r => r.PublishedOn)
            .Where(i => i.IsPublished == true)
            .FirstOrDefault();
    }

    public override AVOverviewInstance Add(AVOverviewInstance entity)
    {
        entity.Sections.ForEach(section =>
        {
            section.Instance = entity;
            this.Context.Add(section);
            section.Items.ForEach(item =>
            {
                item.Section = section;
                this.Context.Add(item);
            });
        });
        return base.Add(entity);
    }

    public override AVOverviewInstance Update(AVOverviewInstance entity)
    {
        // Make a request for the original sections so that they can be compared and updated correctly.
        var originalSections = this.Context.AVOverviewSections
            .AsNoTracking()
            .Include(s => s.Items)
            .Where(s => s.InstanceId == entity.Id).ToArray();
        originalSections.Except(entity.Sections).ForEach(s =>
        {
            // Delete sections that are no longer referenced in this update.
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.Sections.ForEach(section =>
        {
            // Add or update sections that are referenced in this update.
            section.Instance = entity;
            if (section.Id == 0)
            {
                // Add the new section and items in the section.
                this.Context.Add(section);
                section.Items.ForEach(item =>
                {
                    item.Section = section;
                    this.Context.Add(item);
                });
            }
            else
            {
                // Update the section and remove/add/update items in the section.
                var originalSection = originalSections.FirstOrDefault(s => s.Id == section.Id);
                if (originalSection != null)
                {
                    originalSection.Items.Except(section.Items).ForEach(s =>
                    {
                        this.Context.Entry(s).State = EntityState.Deleted;
                    });
                    section.Items.ForEach(item =>
                    {
                        item.Section = section;
                        if (item.Id == 0)
                            this.Context.Add(item);
                        else
                            this.Context.Update(item);
                    });
                    this.Context.Update(section);
                }
            }
        });
        return base.Update(entity);
    }

    public UserAVOverviewInstance? GetUserAVOverviewInstance(long instanceId, int userId)
    {
        return this.Context.UserAVOverviewInstances
            .AsNoTracking()
            .FirstOrDefault(uri => uri.InstanceId == instanceId && uri.UserId == userId);
    }

    public IEnumerable<UserAVOverviewInstance> GetUserAVOverviewInstances(long instanceId)
    {
        return this.Context.UserAVOverviewInstances
            .AsNoTracking()
            .Include(uri => uri.User)
            .Where(uri => uri.InstanceId == instanceId)
            .ToArray();
    }

    public UserAVOverviewInstance Add(UserAVOverviewInstance entity)
    {
        this.Context.Add(entity);
        return entity;
    }

    public UserAVOverviewInstance AddAndSave(UserAVOverviewInstance entity)
    {
        this.Add(entity);
        this.CommitTransaction();
        return entity;
    }

    public UserAVOverviewInstance Update(UserAVOverviewInstance entity)
    {
        this.Context.Update(entity);
        return entity;
    }

    public UserAVOverviewInstance UpdateAndSave(UserAVOverviewInstance entity)
    {
        this.Update(entity);
        this.CommitTransaction();
        return entity;
    }

    public IEnumerable<UserAVOverviewInstance> UpdateAndSave(IEnumerable<UserAVOverviewInstance> entities)
    {
        var instanceIds = entities.Select(e => e.InstanceId).Distinct().ToArray();
        var originalInstances = this.Context.UserAVOverviewInstances.Where(uri => instanceIds.Contains(uri.InstanceId)).ToArray();
        foreach (var entity in entities)
        {
            var original = originalInstances.FirstOrDefault(uri => uri.UserId == entity.UserId && uri.InstanceId == entity.InstanceId);
            if (original == null)
            {
                this.Context.Add(entity);
            }
            else
            {
                original.Status = entity.Status;
                original.SentOn = entity.SentOn;
                original.Response = entity.Response;
                this.Context.Update(original);
            }
        }
        this.CommitTransaction();
        return entities;
    }

    public UserAVOverviewInstance Delete(UserAVOverviewInstance entity)
    {
        this.Context.Remove(entity);
        return entity;
    }

    public UserAVOverviewInstance DeleteAndSave(UserAVOverviewInstance entity)
    {
        this.Delete(entity);
        this.CommitTransaction();
        return entity;
    }
    #endregion
}
