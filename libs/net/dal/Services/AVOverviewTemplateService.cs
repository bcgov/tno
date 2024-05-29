using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class AVOverviewTemplateService : BaseService<AVOverviewTemplate, AVOverviewTemplateType>, IAVOverviewTemplateService
{
    #region Variables
    #endregion

    #region Constructors
    public AVOverviewTemplateService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<ReportService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<AVOverviewTemplate> FindAll()
    {
        return this.Context.AVOverviewTemplates
            .AsNoTracking()
            .Include(t => t.Sections).ThenInclude(s => s.Source)
            .Include(t => t.Sections).ThenInclude(s => s.Series)
            .Include(t => t.Sections).ThenInclude(s => s.Items)
            .ToArray();
    }

    public override AVOverviewTemplate? FindById(AVOverviewTemplateType id)
    {
        return this.Context.AVOverviewTemplates
            .Include(t => t.Template)
            .Include(t => t.SubscribersManyToMany).ThenInclude(s => s.User)
            .Include(t => t.Sections).ThenInclude(s => s.Source)
            .Include(t => t.Sections).ThenInclude(s => s.Series)
            .Include(t => t.Sections).ThenInclude(s => s.Items)
            .FirstOrDefault(t => t.TemplateType == id);
    }

    public override AVOverviewTemplate Add(AVOverviewTemplate entity)
    {
        entity.SubscribersManyToMany.ForEach(user =>
        {
            user.Template = entity;
            user.TemplateType = entity.TemplateType;
            this.Context.UserAVOverviews.Add(user);
        });
        entity.Sections.ForEach(section =>
        {
            section.Template = entity;
            this.Context.Add(section);
            section.Items.ForEach(item =>
            {
                item.Section = section;
                this.Context.Add(item);
            });
        });
        return base.Add(entity);
    }

    public override AVOverviewTemplate Update(AVOverviewTemplate entity)
    {
        var originalSubscribers = this.Context.UserAVOverviews.Where(u => u.TemplateType == entity.TemplateType).ToArray();
        originalSubscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(s =>
        {
            var originalSubscriber = originalSubscribers.FirstOrDefault(rs => rs.UserId == s.UserId);
            if (originalSubscriber == null)
            {
                this.Context.UserAVOverviews.Add(s);
            }
            else if (originalSubscriber.IsSubscribed != s.IsSubscribed || originalSubscriber.SendTo != s.SendTo)
            {
                originalSubscriber.IsSubscribed = s.IsSubscribed;
                originalSubscriber.SendTo = s.SendTo;
            }
        });

        var originalSections = this.Context.AVOverviewTemplateSections.Where(s => s.TemplateType == entity.TemplateType).ToArray();
        originalSections.Except(entity.Sections).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.Sections.ForEach(section =>
        {
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
                this.Context.Update(section);
                section.Items.ForEach(item =>
                {
                    item.Section = section;
                    if (item.Id == 0)
                        this.Context.Add(item);
                    else
                        this.Context.Update(item);

                });
            }
        });
        return base.Update(entity);
    }
    #endregion
}
