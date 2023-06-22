using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TNO.Core.Extensions;
using TNO.Elastic;
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
    #region Variables
    private readonly ITNOElasticClient _client;
    private readonly ElasticOptions _elasticOptions;
    private static readonly ContentStatus[] _onlyPublished = new[] { ContentStatus.Publish, ContentStatus.Published };
    #endregion

    #region Properties
    #endregion
    /// <summary>
    /// Creates a new instance of a ContentService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="logger"></param>
    #region Constructors
    public ContentService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ITNOElasticClient client,
        IOptions<ElasticOptions> elasticOptions,
        ILogger<ContentService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _client = client;
        _elasticOptions = elasticOptions.Value;
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
            .Include(c => c.Contributor)
            .Include(c => c.License)
            .Include(c => c.TonePoolsManyToMany).ThenInclude(ct => ct.TonePool)
            .Include(c => c.Owner)
            .AsQueryable();

        if (asNoTracking)
            query = query.AsNoTracking();

        if (!String.IsNullOrWhiteSpace(filter.OtherSource))
            query = query.Where(c => c.OtherSource.ToLower() == filter.OtherSource.ToLower());
        if (!String.IsNullOrWhiteSpace(filter.Headline))
            query = query.Where(c => EF.Functions.Like(c.Headline.ToLower(), $"%{filter.Headline.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Keyword))
            query = query.Where(c => EF.Functions.Like(c.Headline.ToLower(), $"%{filter.Keyword.ToLower()}%") || EF.Functions.Like(c.Body.ToLower(), $"%{filter.Keyword.ToLower()}%"));
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

        if (filter.ContentTypes.Any())
            query = query.Where(c => filter.ContentTypes.Contains(c.ContentType));
        if (filter.Status.HasValue)
            query = query.Where(c => c.Status == filter.Status);

        if (filter.HasTopic.HasValue)
            query = query.Where(c => c.TopicsManyToMany.Any());
        if (!filter.IncludeHidden.HasValue || !filter.IncludeHidden.Value)
            query = query.Where(c => !c.IsHidden); // Do not return hidden content.
        if (filter.OnlyHidden == true)
            query = query.Where(c => c.IsHidden); // Only Hidden.
        if (filter.OnlyPublished.HasValue && filter.OnlyPublished.Value)
            query = query.Where(c => _onlyPublished.Contains(c.Status));

        if (filter.ProductId.HasValue)
            query = query.Where(c => c.ProductId == filter.ProductId);
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

        if (filter.SourceIds?.Any() == true)
            query = query.Where(c => filter.SourceIds.Contains((int)c.SourceId!));

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
    /// Find most recent front pages (5 of them).
    /// </summary>
    /// <returns> Up to 5 front pages.</returns>
    public async Task<IPaged<API.Areas.Services.Models.Content.ContentModel>> FindFrontPages(string index)
    {
        var productQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        var today = DateTime.Today.ToUniversalTime();
        productQueries.Add(q => q.Raw(@"{""match"": {""productId"": 11}}"));
        var response = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(s =>
       {
           var result = s
               .Pretty()
               .Index(index)
               .Size(5);
           result = result.Query(q =>
               productQueries.Any() ? q.Bool(b => b.Should(productQueries)) : new QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>()
           );
           result = result.Sort(s => s.Descending(p => p.PublishedOn).Descending(p => p.Id));
           return result;
       });

        var items = response.IsValid ?
            response.Documents :
            throw new Exception($"Invalid Elasticsearch response: {response.ServerError?.Error?.Reason}");
        return new Paged<API.Areas.Services.Models.Content.ContentModel>(items, 1, 5, response.Total);

    }

    /// <summary>
    /// Find content that matches the specified 'filter'.
    /// TODO: Change to a raw JSON query.
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of content items that match the filter.</returns>
    public async Task<IPaged<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, ContentFilter filter)
    {
        var contentQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        if (filter.ContentIds.Any())
            contentQueries.Add(s => s.Terms(t => t.Field(f => f.Id).Terms(filter.ContentIds)));

        var filterQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        if (filter.SourceIds.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.SourceId).Terms(filter.SourceIds)));
        if (filter.ExcludeSourceIds.Any())
            filterQueries.Add(s => !s.Terms(t => t.Field(f => f.SourceId).Terms(filter.ExcludeSourceIds)));

        if (filter.ProductIds.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.ProductId).Terms(filter.ProductIds)));

        if (filter.ContentTypes.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.ContentType).Terms(filter.ContentTypes.Select(ct => ct.GetName()))));

        filterQueries.Add(s => s.Bool(b => b.Should(contentQueries)));

        var actionQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        foreach (var action in filter.Actions)
            actionQueries.Add(s => s.Nested(n =>
                    n.Path(p => p.Actions)
                    .Query(y =>
                        y.Match(m => m.Field("actions.name").Query(action)
                        ) && (
                            (
                                y.Match(m => m.Field("actions.valueType").Query("Boolean").Field("actions.value").Query("true"))
                            ) || (
                                !y.Match(m => m.Field("actions.valueType").Query("Boolean")) &&
                                y.Wildcard(m => m.Field("actions.value").Value("*"))
                            )
                        )
                    )
                ));
        if (actionQueries.Any())
            filterQueries.Add(s => s.Bool(b => b.Should(actionQueries)));

        if (!string.IsNullOrWhiteSpace(filter.Headline))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Headline).Value($"*{filter.Headline.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
            filterQueries.Add(s => s.MultiMatch(m => m
                .Fields(f => f
                    .Field(p => p.Headline)
                    .Field(p => p.Body)
                    .Field(p => p.Summary)
                    .Field(p => p.Byline)
                )
                .Query(filter.Keyword.ToLower())
            ));

        if (!string.IsNullOrWhiteSpace(filter.PageName))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Page).Value($"*{filter.PageName.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Section))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Section).Value($"*{filter.Section.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Edition))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Edition).Value($"*{filter.Edition.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Byline))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Byline).Value($"*{filter.Byline.ToLower()}*")));

        if (filter.OwnerId.HasValue)
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.OwnerId.Value));

        if (!filter.IncludeHidden.HasValue || !filter.IncludeHidden.Value)
            filterQueries.Add(s => s.Term(t => t.IsHidden, false)); // Do not include hidden content.

        if (filter.OnlyHidden == true)
            filterQueries.Add(s => s.Term(t => t.IsHidden, true)); // Only hidden content.

        if (filter.HasTopic == true)
            filterQueries.Add(s => s.Nested(n => n.IgnoreUnmapped().Path(f => f.Topics).Query(q => q.MatchAll())));

        if (filter.OnlyPublished == true)
            filterQueries.Add(s => s.Terms(m => m.Field("status").Terms(_onlyPublished.Select(s => s.GetName()))));

        if (filter.UserId.HasValue)
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.UserId.Value));

        if (filter.Status.HasValue)
            filterQueries.Add(s => s.Match(m => m.Field(p => p.Status).Query(filter.Status.Value.GetName())));

        if (filter.CreatedOn.HasValue)
            filterQueries.Add(s => s.Match(m => m.Field(p => p.CreatedOn).Query(filter.CreatedOn.Value.ToUniversalTime().ToString("s") + "Z")));

        if (filter.CreatedStartOn.HasValue && filter.CreatedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .GreaterThanOrEquals(filter.CreatedStartOn.Value.ToUniversalTime().ToString("s") + "Z")
                .LessThanOrEquals(filter.CreatedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.CreatedStartOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .GreaterThanOrEquals(filter.CreatedStartOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.CreatedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.CreatedOn)
                .LessThanOrEquals(filter.CreatedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));

        if (filter.UpdatedOn.HasValue)
            filterQueries.Add(s => s.Match(m => m.Field(p => p.UpdatedOn).Query(filter.UpdatedOn.Value.ToUniversalTime().ToString("s") + "Z")));

        if (filter.UpdatedStartOn.HasValue && filter.UpdatedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .GreaterThanOrEquals(filter.UpdatedStartOn.Value.ToUniversalTime().ToString("s") + "Z")
                .LessThanOrEquals(filter.UpdatedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.UpdatedStartOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .GreaterThanOrEquals(filter.UpdatedStartOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.UpdatedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.UpdatedOn)
                .LessThanOrEquals(filter.UpdatedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));

        if (filter.PublishedOn.HasValue)
            filterQueries.Add(s => s.Match(m => m.Field(p => p.PublishedOn).Query(filter.PublishedOn.Value.ToUniversalTime().ToString("s") + "Z")));

        if (filter.PublishedStartOn.HasValue && filter.PublishedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                        .GreaterThanOrEquals(filter.PublishedStartOn.Value.ToUniversalTime().ToString("s") + "Z")
                        .LessThanOrEquals(filter.PublishedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.PublishedStartOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                .GreaterThanOrEquals(filter.PublishedStartOn.Value.ToUniversalTime().ToString("s") + "Z")));
        else if (filter.PublishedEndOn.HasValue)
            filterQueries.Add(s => s.DateRange(m => m
                .Field(p => p.PublishedOn)
                .LessThanOrEquals(filter.PublishedEndOn.Value.ToUniversalTime().ToString("s") + "Z")));

        var response = await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(s =>
        {
            var result = s
                .Pretty()
                .Index(index)
                .From((filter.Page - 1) * filter.Quantity)
                .Size(filter.Quantity);

            if (contentQueries.Any())
                result = result.Query(q => q.Bool(b => b.Must(contentQueries)));
            else if (filterQueries.Any())
                result = result.Query(q => q.Bool(b => b.Must(filterQueries)));

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
                if (sort == "sourceSort") objPath = p => p.Source!.SortOrder;

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
        return new Paged<API.Areas.Services.Models.Content.ContentModel>(items, filter.Page, filter.Quantity, response.Total);
    }

    /// <summary>
    /// Make a raw JSON query to Elasticsearch and return content.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, JsonDocument filter)
    {
        return await _client.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(index, filter);
    }

    public override Content? FindById(long id)
    {
        return this.Context.Contents
            .Include(c => c.Product)
            .Include(c => c.Series)
            .Include(c => c.Contributor)
            .Include(c => c.License)
            .Include(c => c.Source)
            .Include(c => c.Owner)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.TopicsManyToMany).ThenInclude(cc => cc.Topic)
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
            .Include(c => c.Contributor)
            .Include(c => c.License)
            .Include(c => c.Source)
            .Include(c => c.Owner)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.Action)
            .Include(c => c.TopicsManyToMany).ThenInclude(cc => cc.Topic)
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
        var result = base.AddAndSave(entity);

        // Ensure all content has a UID.
        if (result.GuaranteeUid())
            result = base.UpdateAndSave(result);

        return result;
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
        if (entity.GuaranteeUid() && original.Uid != entity.Uid) original.Uid = entity.Uid;
        return base.UpdateAndSave(original);
    }


    /// <summary>
    /// Get all the notification instances for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public IEnumerable<NotificationInstance> GetNotificationsFor(long contentId)
    {
        return this.Context.NotificationInstances
            .Where(n => n.ContentId == contentId);
    }
    #endregion
}
