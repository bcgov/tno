using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class AVOverviewInstanceService : BaseService<AVOverviewInstance, int>, IAVOverviewInstanceService
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
    public override AVOverviewInstance? FindById(int id)
    {
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .Include(i => i.Template)
            .Include(i => i.Sections).ThenInclude(s => s.Source)
            .Include(i => i.Sections).ThenInclude(s => s.Series)
            .Include(i => i.Sections).ThenInclude(s => s.Items)
            .FirstOrDefault(i => i.Id == id);
    }

    /// <summary>
    /// Return the last evening overview instance for the specified date.
    /// </summary>
    /// <param name="publishedOn"></param>
    /// <returns></returns>
    public AVOverviewInstance? FindByDate(DateTime publishedOn)
    {
        var date = new DateTime(publishedOn.Year, publishedOn.Month, publishedOn.Day, 0, 0, 0, DateTimeKind.Utc);
        return this.Context.AVOverviewInstances
            .AsNoTracking()
            .Include(i => i.Template)
            .Include(i => i.Sections).ThenInclude(s => s.Source)
            .Include(i => i.Sections).ThenInclude(s => s.Series)
            .Include(i => i.Sections).ThenInclude(s => s.Items)
            .OrderByDescending(r => r.PublishedOn)
            .Where(i => i.PublishedOn == date)
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
        var originalSections = this.Context.AVOverviewSections.Where(s => s.InstanceId == entity.Id).ToArray();
        originalSections.Except(entity.Sections).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.Sections.ForEach(section =>
        {
            section.Instance = entity;
            if (section.Id == 0)
            {
                this.Context.Add(section);
                section.Items.ForEach(item =>
                {
                    item.Section = section;
                    this.Context.Add(item);
                });
            }
            else
            {
                var originalItems = this.Context.AVOverviewSectionItems.Where(s => s.SectionId == section.Id).ToArray();
                originalItems.Except(section.Items).ForEach(s =>
                {
                    this.Context.Entry(s).State = EntityState.Deleted;
                });
                this.Context.Entry(originalSections.First(x => x.Id == section.Id)).State = EntityState.Detached;
                this.Context.Entry(section).State = EntityState.Modified;
                section.Items.ForEach(item =>
                {
                    item.Section = section;
                    if (item.Id == 0)
                        this.Context.Add(item);
                    else
                    {
                        this.Context.Entry(originalItems.First(x => x.Id == item.Id)).State = EntityState.Detached;
                        this.Context.Entry(item).State = EntityState.Modified;
                    }
                });
            }
        });
        return base.Update(entity);
    }
    #endregion
}
