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
    /// Provides a simple way to allow the caller to decide which includes are required.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include"></param>
    /// <returns></returns>
    Notification? FindById(int id, Func<IQueryable<Notification>, IQueryable<Notification>>? include, bool asNoTracking = true);

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

    /// <summary>
    /// Subscribe the user to the content so that they receive the specified notification.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="contentId"></param>
    void SubscriberUserToContent(int userId, long contentId);

    /// <summary>
    /// Get notifications based on the filter for the dashboard.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<NotificationInstance> GetDashboard(DashboardFilter filter);

    /// <summary>
    /// Get all CHES message Ids for notifications at the specified 'status' and that were sent on or after 'cutOff' date and time.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="cutOff"></param>
    /// <returns></returns>
    IEnumerable<API.Areas.Services.Models.Notification.ChesNotificationMessagesModel> GetChesMessageIds(NotificationStatus status, DateTime cutOff);
}
