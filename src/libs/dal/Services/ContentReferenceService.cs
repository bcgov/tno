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
        // An ingest posts all its content to one topic, so the topic identifies the ingest's references.
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(c => c.Topic == filter.Topic);
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

        // Created is when the ingest discovered the content, an end date is exclusive.
        if (filter.CreatedStartOn.HasValue)
            query = query.Where(c => c.CreatedOn >= filter.CreatedStartOn.Value.ToUniversalTime());
        if (filter.CreatedEndOn.HasValue)
            query = query.Where(c => c.CreatedOn < filter.CreatedEndOn.Value.ToUniversalTime());

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

    /// <summary>
    /// Find the content imported for each of the specified 'references'.
    /// Content is matched on its source code and uid.
    /// </summary>
    /// <param name="references"></param>
    /// <returns>The content id and headline for each reference that has been imported, keyed by source and uid.</returns>
    public Dictionary<(string Source, string Uid), (long Id, string Headline)> FindContents(IEnumerable<ContentReference> references)
    {
        var keys = references.Select(r => (r.Source, r.Uid)).Distinct().ToArray();
        if (keys.Length == 0) return new Dictionary<(string Source, string Uid), (long Id, string Headline)>();

        var sources = keys.Select(k => k.Source).Distinct().ToArray();
        var uids = keys.Select(k => k.Uid).Distinct().ToArray();
        // Content is only indexed on (content_type, source, uid), so include every content type to use the index.
        var contentTypes = Enum.GetValues<ContentType>();
        var contents = this.Context.Contents
            .AsNoTracking()
            .Where(c => contentTypes.Contains(c.ContentType) && sources.Contains(c.OtherSource) && uids.Contains(c.Uid))
            .Select(c => new { c.OtherSource, c.Uid, c.Id, c.Headline })
            .ToArray();

        var result = new Dictionary<(string Source, string Uid), (long Id, string Headline)>();
        foreach (var content in contents.OrderBy(c => c.Id))
            result.TryAdd((content.OtherSource, content.Uid), (content.Id, content.Headline));
        return result;
    }
    #endregion
}
