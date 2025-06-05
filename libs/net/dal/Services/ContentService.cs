using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
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
    private static readonly string[] ValidateQueryFieldNames = new string[] {"simple_query_string", "query_string"};
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
    /// Validate JSON query and return results.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<Elastic.Models.ValidateResultModel> ValidateElasticsearchSimpleQueryAsync(string index, JsonDocument filter, string? arrayFieldNames)
    {
        var emptyResult = new Elastic.Models.ValidateResultModel();
        if (filter != null)
        {
            JsonObject? filterJsonObject = JsonSerializer.Deserialize<JsonObject>(filter);
            if (filterJsonObject == null) return emptyResult;

            // remove "size" and "sort" fields which are not accepted by validate request
            RemoveJsonObjectKey("size", filterJsonObject);
            RemoveJsonObjectKey("sort", filterJsonObject);
            if (filterJsonObject["query"]?["bool"]?["must"] == null) return emptyResult;

            JsonArray? jsonArray = filterJsonObject["query"]?["bool"]?["must"]?.AsArray();
            var toRemoveIndexList = new List<int>();
            for (int i = 0; i < jsonArray?.Count(); i++)
            {
                var node = jsonArray[i];
                if (node != null)
                {
                    if (ValidateQueryFieldNames.Any(x => node.AsObject().ContainsKey(x)))
                    {
                        var fieldName = ValidateQueryFieldNames.Where(x => node.AsObject().ContainsKey(x)).FirstOrDefault();
                        if (!string.IsNullOrEmpty(fieldName))
                        {
                            var nodeValue = node[fieldName];
                            if (nodeValue != null)
                            {
                                if (nodeValue["fields"] != null)
                                {
                                    var fieldsValues = nodeValue?["fields"]?.AsArray();
                                    UpdateFieldsArray(fieldsValues, arrayFieldNames);
                                }
                            }
                        }
                    }
                    else
                    {
                        toRemoveIndexList.Add(i);
                    }
                }
            }
            for (int i = toRemoveIndexList.Count() - 1; i >= 0; i--)
            {
                jsonArray?.RemoveAt(toRemoveIndexList[i]);
            }
            var query = JsonSerializer.Deserialize<JsonDocument>(filterJsonObject);
            if (query != null)
            {
                return await _client.ValidateAsync(index, query);
            }
        }
        return emptyResult;
    }

    /// <summary>
    /// Customize field names array.
    /// </summary>
    /// <param name="fieldsValues"></param>
    /// <param name="arrayFieldName"></param>
    private void UpdateFieldsArray(JsonArray? fieldsValues, string? arrayFieldNames)
    {
        if (fieldsValues == null || string.IsNullOrEmpty(arrayFieldNames)) return;
        var toRemoveIndexList = new List<int>();
        for (int j = 0; j < fieldsValues.Count(); j++)
        {
            toRemoveIndexList.Add(j);
        }
        for (int j = toRemoveIndexList.Count() - 1; j >= 0; j--)
        {
            fieldsValues.RemoveAt(toRemoveIndexList[j]);
        }
        var fieldNameList = arrayFieldNames.Split(',');
        foreach (var fieldName in fieldNameList)
        {
            fieldsValues.Add(JsonValue.Create(fieldName));            
        }
    }

    /// <summary>
    /// Remove json object key
    /// </summary>
    /// <param name="key"></param>
    /// <param name="jobject"></param>
    /// <returns></returns>
    private void RemoveJsonObjectKey(string key, JsonObject? jobject)
    {
        if (jobject == null) return;
        if (jobject.ContainsKey(key))
        {
            jobject.Remove(key);
        }
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
    /// Get the changed properties between current values and database values, display to users .
    /// Ignores system fields type fields to focus on business-relevant changes.
    /// </summary>
    /// <param name="currentValues">Current property values from the entity being saved</param>
    /// <param name="databaseValues">Current property values from the database</param>
    /// <returns>Dictionary of changed properties with their current and database values</returns>
    private Dictionary<string, object> GetChangedProperties(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues currentValues,
    Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues? databaseValues)
    {
        // Fields to ignore: system fields
        var ignoreFields = new HashSet<string> {
            nameof(Content.Versions),
            nameof(Content.UpdatedBy),
            nameof(Content.UpdatedOn),
            nameof(Content.Version),
            nameof(Content.CreatedBy),
            nameof(Content.CreatedOn),
            nameof(Content.PostedOn),
        };

        var propertyNames = currentValues.Properties
            .Select(p => p.Name)
            .Where(name => !ignoreFields.Contains(name));  // Filter out ignored fields

        var valueComparison = propertyNames.ToDictionary(
            name => name,
            name => new
            {
                Current = currentValues[name],
                Database = databaseValues?[name]
            }
        );

        return valueComparison
            .Where(kvp => !Equals(kvp.Value.Current, kvp.Value.Database))
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new
                {
                    CurrentValue = kvp.Value.Current?.ToString() ?? "null",
                    DatabaseValue = kvp.Value.Database?.ToString() ?? "null"
                } as object
            );
    }

    /// <summary>
    /// Log the detailed changes between current and database values for each changed field.
    /// </summary>
    /// <param name="changedProperties">Dictionary containing the changed properties and their values</param>
    private void LogFieldChanges(Dictionary<string, object> changedProperties)
    {
        var changes = changedProperties.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var values = (dynamic)kvp.Value;
                return new
                {
                    Previous = values.DatabaseValue,
                    Current = values.CurrentValue
                };
            }
        );

        this.Logger.LogInformation("Field changes detected:\n" +
            string.Join("\n", changes.Select(c =>
                $"Field: {c.Key}\n" +
                $"  - Previous: {c.Value.Previous}\n" +
                $"  - Current:  {c.Value.Current}"
            ))
        );
    }

    /// <summary>
    /// Update and save the content entity. Handles concurrency conflicts by providing detailed information about changes.
    /// </summary>
    /// <param name="entity">The content entity to update</param>
    /// <returns>The updated content entity</returns>
    /// <exception cref="NoContentException">Thrown when the entity does not exist</exception>
    /// <exception cref="DbUpdateConcurrencyException">Thrown when a concurrency conflict is detected</exception>
    public override Content UpdateAndSave(Content entity)
    {
        try
        {
            var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
            this.Context.UpdateContext(original, entity);
            if (entity.GuaranteeUid() && original.Uid != entity.Uid) original.Uid = entity.Uid;
            return base.UpdateAndSave(original);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            this.Logger.LogError("Concurrency conflict detected - ContentId: {ContentId}", entity.Id);

            var entry = ex.Entries.Single();
            var currentValues = entry.CurrentValues;
            var databaseValues = entry.GetDatabaseValues();

            if (databaseValues == null)
            {
                var errorMessage = "Unable to retrieve latest content.";
                this.Logger.LogError(ex, errorMessage);
                throw new NoContentException(errorMessage, ex);
            }

            // Get changed properties and log details
            var changedProperties = GetChangedProperties(currentValues, databaseValues);
            LogFieldChanges(changedProperties);

            if (changedProperties.Keys.Any())
            {
                var changedFields = string.Join(",", changedProperties.Keys);
                var contentConflictEx = new ContentConflictException($"FIELDS:{changedFields}");
                throw new DbUpdateConcurrencyException(contentConflictEx.Message, contentConflictEx);
            }

            throw;
        }
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
