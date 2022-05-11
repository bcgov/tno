using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class ContentReferenceService : BaseService<ContentReference, string[]>, IContentReferenceService
{
    #region Properties
    #endregion

    #region Constructors
    public ContentReferenceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<ContentReferenceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods

    public IPaged<ContentReference> Find(ContentReferenceFilter filter)
    {
        var query = this.Context.ContentReferences
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Source))
            query = query.Where(c => c.Source == filter.Source);
        if (!String.IsNullOrWhiteSpace(filter.Uid))
            query = query.Where(c => EF.Functions.Like(c.Uid.ToLower(), $"%{filter.Uid.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(c => EF.Functions.Like(c.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));

        if (filter.Offset.HasValue)
            query = query.Where(c => c.Offset == filter.Offset);
        if (filter.Partition.HasValue)
            query = query.Where(c => c.Partition == filter.Partition);

        if (filter.PublishedOn.HasValue)
            query = query.Where(c => c.PublishedOn == filter.PublishedOn.Value.ToUniversalTime());
        if (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime() && c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());
        else if (filter.PublishedStartOn.HasValue)
            query = query.Where(c => c.PublishedOn >= filter.PublishedStartOn.Value.ToUniversalTime());
        else if (filter.PublishedEndOn.HasValue)
            query = query.Where(c => c.PublishedOn <= filter.PublishedEndOn.Value.ToUniversalTime());

        if (filter.UpdatedOn.HasValue)
            query = query.Where(c => c.SourceUpdateOn == filter.UpdatedOn.Value.ToUniversalTime());
        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.SourceUpdateOn >= filter.UpdatedStartOn.Value.ToUniversalTime() && c.SourceUpdateOn <= filter.UpdatedEndOn.Value.ToUniversalTime());
        else if (filter.UpdatedStartOn.HasValue)
            query = query.Where(c => c.SourceUpdateOn >= filter.UpdatedStartOn.Value.ToUniversalTime());
        else if (filter.UpdatedEndOn.HasValue)
            query = query.Where(c => c.SourceUpdateOn <= filter.UpdatedEndOn.Value.ToUniversalTime());

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderByDescending(c => c.PublishedOn).ThenBy(c => c.Source);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<ContentReference>();
        return new Paged<ContentReference>(items, filter.Page, filter.Quantity, total);
    }

    public override ContentReference Add(ContentReference entity)
    {
        base.Add(entity);
        return entity;
    }

    public override ContentReference Update(ContentReference entity)
    {
        var original = FindByKey(entity.Source, entity.Uid) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);
        base.Update(original);
        return original;
    }
    #endregion
}
