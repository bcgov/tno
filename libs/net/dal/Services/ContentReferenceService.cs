using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

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
    /// <summary>
    /// Make a request to the database fro the specified filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<ContentReference> Find(ContentReferenceFilter filter)
    {
        var query = this.Context.ContentReferences
            .AsNoTracking()
            .AsQueryable();

        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status);

        if (filter.Sources?.Any() == true)
            query = query.Where(c => filter.Sources.Contains(c.Source));
        if (!String.IsNullOrWhiteSpace(filter.Uid))
            query = query.Where(c => EF.Functions.Like(c.Uid.ToLower(), $"%{filter.Uid.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(c => EF.Functions.Like(c.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));
        if (filter.MediaTypeIds?.Any() == true)
            query = from cf in query
                    join c in this.Context.Contents on cf.Uid equals c.Uid
                    where filter.MediaTypeIds.Contains(c.MediaTypeId)
                    select cf;

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

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 50;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<ContentReference>();
        return new Paged<ContentReference>(items, page, quantity, total);
    }

    public override ContentReference AddAndSave(ContentReference entity)
    {
        return base.AddAndSave(entity);
    }

    /// <summary>
    /// Update and save the content reference in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override ContentReference UpdateAndSave(ContentReference entity)
    {
        var original = FindByKey(entity.Source, entity.Uid) ?? throw new NoContentException("Entity does not exist");
        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);
        return base.UpdateAndSave(original);
    }

    /// <summary>
    /// Find all the content ids for the specified 'uid'.
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public long[] FindContentIds(string uid)
    {
        return this.Context.Contents
            .AsNoTracking()
            .Where(c => c.Uid.ToLower() == uid.ToLower()).Select(c => c.Id)
            .ToArray();
    }
    #endregion
}
