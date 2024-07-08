using System.Linq.Expressions;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nest;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

/// <summary>
/// ContentService class, provides the Data Access Layer (DAL) service for interacting with content with the database.
/// </summary>
public class ContentService : BaseService<Content, long>, IContentService
{
    #region Variables
    private readonly ITNOElasticClient _client;
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
    /// <param name="logger"></param>
    #region Constructors
    public ContentService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ITNOElasticClient client,
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
            .Include(c => c.MediaType)
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
        if (!String.IsNullOrWhiteSpace(filter.MediaType))
            query = query.Where(c => EF.Functions.Like(c.MediaType!.Name.ToLower(), $"%{filter.MediaType.ToLower()}%"));

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
        if (filter.IsHidden.HasValue)
            query = query.Where(c => c.IsHidden == filter.IsHidden);
        if (filter.OnlyPublished.HasValue && filter.OnlyPublished.Value)
            query = query.Where(c => _onlyPublished.Contains(c.Status));

        if (filter.MediaTypeId.HasValue)
            query = query.Where(c => c.MediaTypeId == filter.MediaTypeId);
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

        if (filter.MediaTypeIds?.Any() == true)
            query = query.Where(c => filter.MediaTypeIds.Contains(c.MediaTypeId));

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

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<Content>();
        return new Paged<Content>(items, page, quantity, total);
    }

    // TODO: Delete this function
    /// <summary>
    /// Find content that matches the specified 'filter'.
    /// </summary>
    /// <param name="filter">Filter to apply to the query.</param>
    /// <returns>A page of content items that match the filter.</returns>
    public async Task<IPaged<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, ContentFilter filter)
    {
        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;

        var contentQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        if (filter.ContentIds.Any())
            contentQueries.Add(s => s.Terms(t => t.Field(f => f.Id).Terms(filter.ContentIds)));

        var filterQueries = new List<Func<QueryContainerDescriptor<API.Areas.Services.Models.Content.ContentModel>, QueryContainer>>();
        if (filter.SourceIds.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.SourceId).Terms(filter.SourceIds)));
        if (filter.ExcludeSourceIds.Any())
            filterQueries.Add(s => !s.Terms(t => t.Field(f => f.SourceId).Terms(filter.ExcludeSourceIds)));

        if (filter.Sentiment.Any())
            filterQueries.Add(s => s.Nested(n => n.Path(p => p.TonePools).Query(y => y.Range(m => m.Field("tonePools.value").GreaterThanOrEquals(filter.Sentiment.First()).LessThanOrEquals(filter.Sentiment.Last())))));

        if (filter.MediaTypeIds.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.MediaTypeId).Terms(filter.MediaTypeIds)));

        if (filter.ContentTypes.Any())
            filterQueries.Add(s => s.Terms(t => t.Field(f => f.ContentType).Terms(filter.ContentTypes.Select(ct => ct.GetName()))));

        if (contentQueries.Count > 0)
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
        {
            filterQueries.Add(s =>
               filter.Headline.Contains(' ') ?
               s.MatchPhrase(m => m.Field(p => p.Headline).Query($"*{filter.Headline.ToLower()}*")) :
               s.Wildcard(m => m.Field(p => p.Headline).Value($"*{filter.Headline.ToLower()}*"))
            );
        }

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

        if (!string.IsNullOrWhiteSpace(filter.Names))
        {
            var names = filter.Names.Split(',').Select(a => a.Trim()).ToList();
            List<string> nameQueries = new();
            foreach (var name in names)
            {
                var nameSegments = name.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (nameSegments.Length == 1)
                {
                    //(Beyonce)
                    nameQueries.Add($"({name})");
                }
                else if (nameSegments.Length > 1)
                {
                    var firstName = nameSegments.First();
                    var firstInitial = firstName.Substring(0, 1);
                    var lastName = nameSegments.Last();
                    // search for firstName+lastName OR firstInitial.lastName
                    // (David+Eby | D.Eby)
                    nameQueries.Add($"({lastName}+{firstName} | {firstInitial}.{lastName})");
                }
            }
            const int searchBoost = 10;
            filterQueries.Add(s =>
                s.SimpleQueryString(qs => qs
                    .Fields(f => f
                        .Field(p => p.Headline, searchBoost)
                        .Field(p => p.Byline, searchBoost)
                        .Field(p => p.Body)
                        .Field(p => p.Summary)
                    )
                .Query(string.Join(" | ", nameQueries))));
        }

        if (!string.IsNullOrWhiteSpace(filter.PageName))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Page).Value($"*{filter.PageName.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Section))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Section).Value($"*{filter.Section.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Edition))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Edition).Value($"*{filter.Edition.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.Byline))
            filterQueries.Add(s => s.Wildcard(m => m.Field(p => p.Byline).Value($"*{filter.Byline.ToLower()}*")));

        if (!string.IsNullOrWhiteSpace(filter.StoryText))
            filterQueries.Add(s => s.MultiMatch(m => m
                .Fields(f => f
                    .Field(p => p.Body)
                    .Field(p => p.Summary)
                )
                .Query(filter.StoryText.ToLower())
            ));

        if (filter.OwnerId.HasValue)
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.OwnerId.Value));

        if (!filter.IsHidden.HasValue)
            filterQueries.Add(s => s.Term(t => t.IsHidden, filter.IsHidden));

        if (filter.HasTopic == true)
            filterQueries.Add(s => s.Nested(n => n.IgnoreUnmapped().Path(f => f.Topics).Query(q => q.MatchAll())));

        if (filter.HasFile == true)
            filterQueries.Add(s => s.Nested(n => n.IgnoreUnmapped().Path(f => f.FileReferences).Query(q => q.MatchAll())));

        if (filter.OnlyPublished == true)
            filterQueries.Add(s => s.Terms(m => m.Field("status").Terms(_onlyPublished.Select(s => s.GetName()))));

        if (filter.UserId.HasValue)
            filterQueries.Add(s => s.Term(t => t.OwnerId, filter.UserId.Value));

        if (filter.SeriesId.HasValue)
            filterQueries.Add(s => s.Term(t => t.SeriesId, filter.SeriesId.Value));

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
                .From((page - 1) * quantity)
                .Size(quantity);

            if (contentQueries.Any())
                result = result.Query(q => q.Bool(b => b.Must(contentQueries)));
            else if (filterQueries.Any())
                result = result.Query(q => q.Bool(b => b.Must(filterQueries)));

            if (filter.Sort.Any())
            {
                var selector = new SortDescriptor<API.Areas.Services.Models.Content.ContentModel>();
                foreach (var value in filter.Sort)
                {
                    Expression<Func<API.Areas.Services.Models.Content.ContentModel, dynamic>>? predicate = null;
                    var sort = value.Split(' ');
                    var field = sort.FirstOrDefault();
                    var order = value.EndsWith(" desc") == true ? SortOrder.Descending : SortOrder.Ascending;

                    if (field == "id") predicate = p => p.Id;
                    if (field == "mediaTypeId") predicate = p => p.MediaTypeId;
                    if (field == "ownerId") predicate = p => p.OwnerId!;
                    if (field == "publishedOn") predicate = p => p.PublishedOn!;
                    if (field == "otherSource") predicate = p => p.OtherSource;
                    if (field == "page") predicate = p => p.Page;
                    if (field == "section") predicate = p => p.Section;
                    if (field == "status") predicate = p => p.Status;
                    if (field == "source.sortOrder") predicate = p => p.Source!.SortOrder;

                    if (field != null) selector = selector.Field(f => f.Field(predicate).Order(order));
                }

                result.Sort(s => selector);
            }
            else
            {
                result = result.Sort(s => s.Descending(p => p.PublishedOn).Descending(p => p.Id));
            }

            // uncomment this to see the raw query that will be sent to ElasticSearch
            // var json = _client.RequestResponseSerializer.SerializeToString(result);

            return result;
        });

        var items = response.IsValid ?
            response.Documents :
            throw new Exception($"Invalid Elasticsearch response: {response.ServerError?.Error?.Reason}");
        return new Paged<API.Areas.Services.Models.Content.ContentModel>(items, page, quantity, response.Total);
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

    /// <summary>
    /// Get the content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Content? FindById(long id)
    {
        return this.Context.Contents
            .Include(c => c.MediaType)
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
            .Include(c => c.Quotes)
            .FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Get the content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeUserNotifications"></param>
    /// <returns></returns>
    public Content? FindById(long id, bool includeUserNotifications)
    {
        var query = this.Context.Contents
            .Include(c => c.MediaType)
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
            .Include(c => c.Quotes)
            .AsQueryable();

        if (includeUserNotifications)
            query = query.Include(c => c.UserNotifications).ThenInclude(un => un.User);

        return query.FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Get the content for the specified 'uid' and 'source'.
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public Content? FindByUid(string uid, string? source)
    {
        var query = this.Context.Contents
            .Include(c => c.MediaType)
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
            .Include(c => c.Quotes)
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
    /// <exception cref="NoContentException"></exception>
    public override Content UpdateAndSave(Content entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
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
            .OrderByDescending(n => n.Id)
            .Where(n => n.ContentId == contentId);
    }

    /// <summary>
    /// Update the specified 'entity' in the database.
    /// </summary>
    /// <param name="updated"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public Content UpdateStatusOnly(Content updated)
    {
        var original = FindById(updated.Id) ?? throw new NoContentException("Entity does not exist");

        original.Status = updated.Status;
        original.Version = updated.Version;
        this.Context.ResetVersion(original);

        return base.UpdateAndSave(original);
    }

    /// <summary>
    /// Update the content action.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public ContentAction AddOrUpdateContentAction(ContentAction action)
    {
        var result = this.Context.ContentActions.Where(ca => ca.ContentId == action.ContentId && ca.ActionId == action.ActionId).FirstOrDefault();

        if (result != null)
        {
            result.Value = action.Value;
            result.Version = action.Version;
        }
        else
            this.Context.Add(action);

        this.CommitTransaction();
        return action;
    }

    /// <summary>
    /// Update the content topics.
    /// </summary>
    /// <param name="topics">update the current topics with these</param>
    /// <returns></returns>
    public IEnumerable<ContentTopic> AddOrUpdateContentTopics(long contentId, IEnumerable<ContentTopic> topics)
    {
        var currentTopics = this.Context.ContentTopics.Where(ca => ca.ContentId == contentId);

        foreach (var topic in currentTopics)
        {
            var matchingTopic = topics.FirstOrDefault((t) => t.TopicId == topic.TopicId);
            if (matchingTopic == null)
            {
                // remove any topics no longer associated with the content
                this.Context.Remove(topic);
            }
            else if (topic.Score != matchingTopic.Score)
            {
                // update topic score
                topic.Score = matchingTopic.Score;
            }
        }

        foreach (var topic in topics)
        {
            if (!currentTopics.Any((t) => t.TopicId == topic.TopicId))
            {
                // add new topics
                this.Context.Add(topic);
            }
        }

        this.CommitTransaction();

        return this.Context.ContentTopics.Where(ca => ca.ContentId == contentId);
    }
    #endregion
}
