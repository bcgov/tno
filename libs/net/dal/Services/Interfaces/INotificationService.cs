using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface INotificationService : IBaseService<Notification, int>
{
    /// <summary>
    /// Find all notifications for the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<Notification> Find(NotificationFilter filter);

    /// <summary>
    /// Unsubscribe the specified 'userId' from all reports.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> Unsubscribe(int userId);

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'notification'.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindContentWithElasticsearchAsync(Notification notification, int? requestorId);
}
