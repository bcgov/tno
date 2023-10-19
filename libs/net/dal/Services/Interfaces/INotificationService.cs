using TNO.Entities;

namespace TNO.DAL.Services;

public interface INotificationService : IBaseService<Notification, int>
{
    IEnumerable<Notification> FindAll();
    Task<int> Unsubscribe(int userId);
}
