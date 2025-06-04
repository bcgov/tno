
using System.Text.Json;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IContentService : IBaseService<Content, long>
{
    /// <summary>
    /// Get the content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeUserNotifications"></param>
    /// <returns></returns>
    Content? FindById(long id, bool includeUserNotifications);

    IPaged<Content> FindWithDatabase(ContentFilter filter, bool asNoTracking = true);
    Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, JsonDocument filter);
    Task<Elastic.Models.ValidateResultModel> ValidateElasticsearchSimpleQueryAsync(string index, JsonDocument filter, string? arrayFieldNames);
    Content? FindByUid(string uid, string? source);

    /// <summary>
    /// Get all the notification instances for the specified 'contentId'.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    IEnumerable<NotificationInstance> GetNotificationsFor(long contentId);

    /// <summary>
    /// Update the ContentStatus for the specified 'contentId'.
    /// Will not trigger a version number change.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    Content UpdateStatusOnly(Content entity);

    /// <summary>
    /// Add or update the specified content 'action'.
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    ContentAction AddOrUpdateContentAction(ContentAction action);

    /// <summary>
    /// Update the content topics.
    /// </summary>
    /// <param name="topics">update the current topics with these</param>
    /// <returns></returns>
    IEnumerable<ContentTopic> AddOrUpdateContentTopics(long contentId, IEnumerable<ContentTopic> topics);
}
