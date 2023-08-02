
using System.Text.Json;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface IContentService : IBaseService<Content, long>
{
    IPaged<Content> FindWithDatabase(ContentFilter filter, bool asNoTracking = true);
    Task<IPaged<API.Areas.Services.Models.Content.ContentModel>> FindFrontPages(string index);
    Task<IPaged<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, ContentFilter filter);
    Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindWithElasticsearchAsync(string index, JsonDocument filter);
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

}
