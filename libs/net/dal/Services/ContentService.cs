using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// ContentService class, provides the Data Access Layer (DAL) service for interacting with content with the database.
/// </summary>
public class ContentService : BaseService<Content, long>, IContentService
{
    private readonly IElasticClient _client;

    #region Properties
    #endregion
    /// <summary>
    /// Creates a new instance of a ContentService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    #region Constructors
    public ContentService(TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        IElasticClient client,
        ILogger<ContentService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _client = client;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find content that matches the specified 'filter'.
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <param name="asNoTracking">Whether to load into context or not</param>
    /// <returns>A page of content items that match the filter.</returns>
    public IPaged<Content> FindWithDatabase(ContentFilter filter, bool asNoTracking = true)
    {
        var query = this.Context.Contents
            .Include(c => c.Product)
            .Include(c => c.Source)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.Owner)
            .AsQueryable();

        if (asNoTracking)
            query = query.AsNoTracking();

        if (!String.IsNullOrWhiteSpace(filter.OtherSource))
            query = query.Where(c => c.OtherSource.ToLower() == filter.OtherSource.ToLower());
        if (!String.IsNullOrWhiteSpace(filter.Headline))
            query = query.Where(c => EF.Functions.Like(c.Headline.ToLower(), $"%{filter.Headline.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.PageName))
            query = query.Where(c => EF.Functions.Like(c.Page.ToLower(), $"%{filter.PageName.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Section))
            query = query.Where(c => EF.Functions.Like(c.Section.ToLower(), $"%{filter.Section.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Product))
            query = query.Where(c => EF.Functions.Like(c.Product!.Name.ToLower(), $"%{filter.Product.ToLower()}%"));

        if (!String.IsNullOrWhiteSpace(filter.Edition))
            query = query.Where(c => EF.Functions.Like(c.Edition.ToLower(), $"%{filter.Edition.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Byline))
            query = query.Where(c => EF.Functions.Like(c.Byline.ToLower(), $"%{filter.Byline.ToLower()}%"));

        if (filter.ContentType.HasValue)
            query = query.Where(c => c.ContentType == filter.ContentType);
        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status);

        if (filter.IncludedInCategory.HasValue)
            query = query.Where(c => c.CategoriesManyToMany.Any());
        if (!filter.IncludeHidden.HasValue)
            query = query.Where(c => !c.IsHidden);
        else
            query = query.Where(c => c.IsHidden == filter.IncludeHidden);

        if (filter.ProductId.HasValue)
            query = query.Where(c => c.ProductId == filter.ProductId);
        if (filter.SourceId.HasValue)
            query = query.Where(c => c.SourceId == filter.SourceId);
        if (filter.OwnerId.HasValue)
            query = query.Where(c => c.OwnerId == filter.OwnerId);
        if (filter.UserId.HasValue)
        {
            var user = this.Context.Users.Find(filter.UserId);
            if (user != null)
                query = query.Where(c => c.OwnerId == user.Id || c.CreatedBy == user.Username || c.UpdatedBy == user.Username || c.Logs.Any(l => l.CreatedBy == user.Username));
        }

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

        if (filter.ContentIds.Any())
            query = query.Where(c => filter.ContentIds.Contains(c.Id));

        if (filter.ProductIds?.Any() == true)
            query = query.Where(c => filter.ProductIds.Contains(c.ProductId));

        if (filter.Actions.Any() == true)
            query = query.Where(c => c.ActionsManyToMany.Any(ca => filter.Actions.Contains(ca.Action!.Name)
                && ((ca.Action.ValueType == Entities.ValueType.Boolean && ca.Value == "true")
                    || (ca.Action.ValueType != Entities.ValueType.Boolean && !String.IsNullOrWhiteSpace(ca.Value)))));

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
            query = query.OrderByDescending(c => c.PublishedOn).ThenByDescending(c => c.Id);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<Content>();
        return new Paged<Content>(items, filter.Page, filter.Quantity, total);
    }

    /// <summary>
    /// Find content that matches the specified 'filter'.
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of content items that match the filter.</returns>
    public async Task<IPaged<Content>> FindWithElasticsearchAsync(ContentFilter filter)
    {
        var productQueries = new List<Func<QueryContainerDescriptor<Content>, QueryContainer>>();
        foreach (var productId in filter.ProductIds)
        {
            productQueries.Add(s => s.Term(t => t.ProductId, productId));
        }

        var contentQueries = new List<Func<QueryContainerDescriptor<Content>, QueryContainer>>();
        foreach (var contentId in filter.ContentIds)
        {
            contentQueries.Add(s => s.Term(t => t.Id, contentId));
        }

        var filterQueries = new List<Func<QueryContainerDescriptor<Content>, QueryContainer>>();

        if (!string.IsNullOrWhiteSpace(filter.OtherSource))
        {
            filterQueries.Add(s => s.Term(t => t.OtherSource, filter.OtherSource.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(filter.Headline))
        {
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Headline).Value($"*{filter.Headline.ToLower()}*")));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageName))
        {
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Page).Value($"*{filter.PageName.ToLower()}*")));
        }

        if (!string.IsNullOrWhiteSpace(filter.Section))
        {
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Section).Value($"*{filter.Section.ToLower()}*")));
        }

        if (!string.IsNullOrWhiteSpace(filter.Edition))
        {
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Edition).Value($"*{filter.Edition.ToLower()}*")));
        }

        if (!string.IsNullOrWhiteSpace(filter.Byline))
        {
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Byline).Value($"*{filter.Byline.ToLower()}*")));
        }

        if (filter.ContentType.HasValue)
        {
            filterQueries.Add(s => s.Match(m => m.Field(p => p.ContentType).Query(filter.ContentType.Value.ToString())));
        }

        if (filter.IncludeHidden.HasValue)
        {
            filterQueries.Add(s => s.Term(t => t.IsHidden, filter.IncludeHidden.Value));
        }

        if (filter.SourceId.HasValue)
        {
            filterQueries.Add(s => s.Term(t => t.SourceId, filter.SourceId.Value));
        }

        if (filter.OwnerId.HasValue)
        {
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.OwnerId.Value));
        }

        if (filter.IncludedInCategory.HasValue)
        {
            filterQueries.Add(s => s.Exists(e => e.Field(p => p.Categories.Any())));
        }

        foreach (var action in filter.Actions)
        {
            filterQueries.Add(s => s
                .Nested(n => n
                .Path(p => p.Actions)
                .Query(y => y
                .Match(m => m
                .Field("actions.name")
                .Query(action)) && ((y.Match(m => m
                .Field("actions.valueType")
                .Query("Boolean")) && y.Match(m => m
                .Field("actions.value")
                .Query("true"))) || (!y.Match(m => m
                .Field("actions.valueType")
                .Query("Boolean")) && y.Exists(m => m
                .Field("actions.value")))))));
        }

        if (filter.CreatedOn.HasValue)
        {
            var createdOn = filter.CreatedOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.Match(m => m.Field(p => p.CreatedOn).Query(createdOn)));
        }

        if (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue)
        {
            var startOn = filter.CreatedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            var endOn = filter.CreatedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .GreaterThanOrEquals(startOn)
                .LessThanOrEquals(endOn)));
        }
        else if (filter.CreatedStartOn.HasValue)
        {
            var startOn = filter.CreatedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .GreaterThanOrEquals(startOn)));
        }
        else if (filter.CreatedEndOn.HasValue)
        {
            var endOn = filter.CreatedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .LessThanOrEquals(endOn)));
        }

        if (filter.UpdatedOn.HasValue)
        {
            var updatedOn = filter.UpdatedOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.Match(m => m.Field(p => p.UpdatedOn).Query(updatedOn)));
        }

        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
        {
            var startOn = filter.UpdatedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            var endOn = filter.UpdatedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .GreaterThanOrEquals(startOn)
                .LessThanOrEquals(endOn)));
        }
        else if (filter.UpdatedStartOn.HasValue)
        {
            var startOn = filter.UpdatedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .GreaterThanOrEquals(startOn)));
        }
        else if (filter.UpdatedEndOn.HasValue)
        {
            var endOn = filter.UpdatedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .LessThanOrEquals(endOn)));
        }

        if (filter.PublishedOn.HasValue)
        {
            var publishedOn = filter.PublishedOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.Match(m => m.Field(p => p.PublishedOn).Query(publishedOn)));
        }

        if (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue)
        {
            var publishedStartOn = filter.PublishedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            var publishedEndOn = filter.PublishedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                .GreaterThanOrEquals(publishedStartOn)
                .LessThanOrEquals(publishedEndOn)));
        }
        else if (filter.PublishedStartOn.HasValue)
        {
            var publishedStartOn = filter.PublishedStartOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                .GreaterThanOrEquals(publishedStartOn)));
        }
        else if (filter.PublishedEndOn.HasValue)
        {
            var publishedEndOn = filter.PublishedEndOn.Value.ToUniversalTime().ToString("s") + "Z";
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                .LessThanOrEquals(publishedEndOn)));
        }

        if (filter.UserId.HasValue)
        {
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.UserId.Value));
        }

        if (filter.Status.HasValue)
        {
            filterQueries.Add(s => s.Match(m => m.Field(p => p.Status).Query(filter.Status.Value.ToString())));
        }

        var response = await _client.SearchAsync<Content>(s =>
        {
            var result = s
                .Pretty()
                .Index(_client.ConnectionSettings.DefaultIndex)
                .From((filter.Page - 1) * filter.Quantity)
                .Size(filter.Quantity);

            if (filterQueries.Any() && productQueries.Any() && contentQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Must(m =>
                    m.Bool(a => a.Must(filterQueries)) &&
                    m.Bool(a => a.Should(productQueries)) &&
                    m.Bool(a => a.Should(contentQueries)))));
            }
            else if (filterQueries.Any() && productQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Must(m =>
                    m.Bool(a => a.Must(filterQueries)) &&
                    m.Bool(a => a.Should(productQueries)))));
            }
            else if (filterQueries.Any() && contentQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Must(m =>
                    m.Bool(a => a.Must(filterQueries)) &&
                    m.Bool(a => a.Should(contentQueries)))));
            }
            else if (productQueries.Any() && contentQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Must(m =>
                    m.Bool(a => a.Should(productQueries)) &&
                    m.Bool(a => a.Should(contentQueries)))));
            }
            else if (filterQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Must(filterQueries)));
            }
            else if (productQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Should(productQueries)));
            }
            else if (contentQueries.Any())
            {
                result = result.Query(q => q.Bool(b => b.Should(contentQueries)));
            }

            if (filter.Sort.Any())
            {
                Expression<Func<Content, dynamic>>? objPath = null;
                var sorts = filter.Sort[0];
                var sort = sorts.Split(' ')[0];

                if (sort == "id") objPath = p => p.Id;
                if (sort == "productId") objPath = p => p.ProductId;
                if (sort == "ownerId") objPath = p => p.OwnerId!;
                if (sort == "publishedOn") objPath = p => p.PublishedOn!;
                if (sort == "otherSource") objPath = p => p.OtherSource;
                if (sort == "page") objPath = p => p.Page;
                if (sort == "status") objPath = p => p.Status;

                if (objPath != null) result = result.Sort(s => sorts.EndsWith(" desc") ? s.Descending(objPath) : s.Ascending(objPath));
            }
            else
            {
                result = result.Sort(s => s.Descending(p => p.PublishedOn).Descending(p => p.Id));
            }

            return result;
        });

        var items = response.IsValid ?
            response.Documents :
            throw new Exception($"Invalid Elasticsearch response: {response.ServerError?.Error?.Reason}");
        return new Paged<Content>(items, filter.Page, filter.Quantity, response.Total);
    }

    public override Content? FindById(long id)
    {
        return this.Context.Contents
            .Include(c => c.Product)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.Source)
            .Include(c => c.Owner)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.CategoriesManyToMany).ThenInclude(cc => cc.Category)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.Labels)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .FirstOrDefault(c => c.Id == id);
    }

    public Content? FindByUid(string uid, string? source)
    {
        var query = this.Context.Contents
            .Include(c => c.Product)
            .Include(c => c.Series)
            .Include(c => c.License)
            .Include(c => c.Source)
            .Include(c => c.Owner)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.CategoriesManyToMany).ThenInclude(cc => cc.Category)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.TagsManyToMany).ThenInclude(ct => ct.Tag)
            .Include(c => c.Labels)
            .Include(c => c.TimeTrackings)
            .Include(c => c.FileReferences)
            .Include(c => c.Links)
            .Where(c => c.Uid == uid);

        if (!String.IsNullOrWhiteSpace(source))
            query = query.Where(c => c.OtherSource == source);

        return query.FirstOrDefault();
    }

    /// <summary>
    /// Add the specified 'entity' to the database
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Content AddAndSave(Content entity)
    {
        entity.AddToContext(this.Context);
        base.AddAndSave(entity);

        // Ensure all content has a UID.
        if (entity.GuaranteeUid())
            base.UpdateAndSave(entity);

        return entity;
    }

    /// <summary>
    /// Update the specified 'entity' in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// TODO: Switch to not found exception throughout services.
    public override Content UpdateAndSave(Content entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity);
        entity.GuaranteeUid();
        base.UpdateAndSave(original);
        return original;
    }
    #endregion
}
