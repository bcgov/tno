
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
    IEnumerable<ContentLink> FindLinks(long contentId, string? value = null);
    IEnumerable<Content> FindLinkedContent(long contentId, string? value = null);
    ContentLink AddOrUpdateLink(long contentId, long linkId, string value);

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
    /// Update the content topics. A score equal to the calculated score is stored as calculated;
    /// any other changed score is an override. Each existing topic's version is checked.
    /// </summary>
    /// <param name="contentId"></param>
    /// <param name="topics">update the current topics with these</param>
    /// <param name="calculatedScore">The score the topic score rules give the content, or null when it is not scored.</param>
    /// <returns></returns>
    IEnumerable<ContentTopic> AddOrUpdateContentTopics(long contentId, IEnumerable<ContentTopic> topics, int? calculatedScore);
}
