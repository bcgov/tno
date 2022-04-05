using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class ContentService : BaseService<Content, long>, IContentService
{
    #region Properties
    #endregion

    #region Constructors
    public ContentService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ContentService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods

    public IPaged<Content> Find(ContentFilter filter)
    {
        var query = this.Context.Contents
            .Include(c => c.ContentType)
            .Include(c => c.MediaType)
            .Include(c => c.DataSource)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.Owner)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Source))
            query = query.Where(c => c.Source == filter.Source);
        if (!String.IsNullOrWhiteSpace(filter.Headline))
            query = query.Where(c => EF.Functions.Like(c.Headline, $"{filter.Headline}"));
        if (!String.IsNullOrWhiteSpace(filter.Section))
            query = query.Where(c => c.PrintContent != null && c.PrintContent.Section == filter.Section);
        if (!String.IsNullOrWhiteSpace(filter.Edition))
            query = query.Where(c => c.PrintContent != null && c.PrintContent.Edition == filter.Edition);
        if (!String.IsNullOrWhiteSpace(filter.StoryType))
            query = query.Where(c => c.PrintContent != null && c.PrintContent.StoryType == filter.StoryType);
        if (!String.IsNullOrWhiteSpace(filter.Byline))
            query = query.Where(c => c.PrintContent != null && EF.Functions.Like(c.PrintContent.Byline, $"{filter.Byline}"));

        if (filter.ContentTypeId.HasValue)
            query = query.Where(c => c.ContentTypeId == filter.ContentTypeId);
        if (filter.MediaTypeId.HasValue)
            query = query.Where(c => c.MediaTypeId == filter.MediaTypeId);
        if (filter.DataSourceId.HasValue)
            query = query.Where(c => c.DataSourceId == filter.DataSourceId);
        if (filter.OwnerId.HasValue)
            query = query.Where(c => c.OwnerId == filter.OwnerId);

        if (filter.CreatedOn.HasValue)
            query = query.Where(c => c.CreatedOn == filter.CreatedOn.Value.ToUniversalTime());
        if (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime() && c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());
        else if (filter.CreatedStartOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime());
        else if (filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn <= filter.CreatedEndOn.Value.ToUniversalTime());

        if (filter.UpdatedOn.HasValue)
            query = query.Where(c => c.UpdatedOn == filter.UpdatedOn.Value.ToUniversalTime());
        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime() && c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());
        else if (filter.UpdatedStartOn.HasValue)
            query = query.Where(c => c.UpdatedOn >= filter.UpdatedStartOn.Value.ToUniversalTime());
        else if (filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.UpdatedOn <= filter.UpdatedEndOn.Value.ToUniversalTime());

        if (filter.PublishedOn.HasValue)
            query = query.Where(c => c.PublishedOn == filter.PublishedOn.Value.ToUniversalTime());
        if (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime() && c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());
        else if (filter.PublishedStartOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime());
        else if (filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());

        if (filter.Actions.Any() == true)
            query = query.Where(c => c.ActionsManyToMany.Any(ca => filter.Actions.Contains(ca.Action!.Name)
                && ((ca.Action.ValueType == Entities.ValueType.Boolean && ca.Value == "true")
                    || (ca.Action.ValueType != Entities.ValueType.Boolean && !String.IsNullOrWhiteSpace(ca.Value)))));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderByDescending(c => c.Id);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Content>();
        return new Paged<Content>(items, filter.Page, filter.Quantity, total);
    }

    public override Content? FindById(long id)
    {
        return this.Context.Contents
            .Include(c => c.ContentType)
            .Include(c => c.MediaType)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.DataSource)
            .Include(c => c.Owner)
            .Include(c => c.PrintContent)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.CategoriesManyToMany).ThenInclude(cc => cc.Category)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .FirstOrDefault(c => c.Id == id);
    }

    public override Content Add(Content entity)
    {
        entity.AddToContext(this.Context);
        base.Add(entity);
        return entity;
    }

    public override Content Update(Content entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity);
        base.Update(original);
        return original;
    }
    #endregion
}
