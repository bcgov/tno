using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class NotificationService : BaseService<Notification, int>, INotificationService
{
    #region Properties
    #endregion

    #region Constructors
    public NotificationService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<NotificationService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<Notification> FindAll()
    {
        return this.Context.Notifications
            .AsNoTracking()
            .Include(n => n.SubscribersManyToMany).ThenInclude(s => s.User)
            .OrderBy(a => a.SortOrder).ThenBy(a => a.Name).ToArray();
    }

    public override Notification? FindById(int id)
    {
        return this.Context.Notifications
            .Include(n => n.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(n => n.Id == id);
    }
    #endregion
}
