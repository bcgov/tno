using TNO.Entities;

namespace TNO.DAL.Services;

public interface INotificationTemplateService : IBaseService<NotificationTemplate, int>
{
    /// <summary>
    /// Return all the notification templates.
    /// </summary>
    /// <returns></returns>
    IEnumerable<NotificationTemplate> FindAll();

    /// <summary>
    /// Find all the public notification templates.
    /// </summary>
    /// <returns></returns>
    IEnumerable<NotificationTemplate> FindPublic();

    /// <summary>
    /// Determine if this notification template is being used by any notifications.
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    bool IsInUse(int templateId);
}
