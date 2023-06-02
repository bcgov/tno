using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
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

    /// <summary>
    /// Add the new report to the database.
    /// Add subscribers to the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Notification Add(Notification entity)
    {
        this.Context.AddRange(entity.SubscribersManyToMany);
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report in the database.
    /// Update subscribers of the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public override Notification Update(Notification entity)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        var subscribers = this.Context.UserNotifications.Where(ur => ur.NotificationId == entity.Id).ToArray();

        subscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(s =>
        {
            var current = subscribers.FirstOrDefault(rs => rs.UserId == s.UserId);
            if (current == null)
                original.SubscribersManyToMany.Add(s);
        });

        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);

        return base.Update(original);
    }
    #endregion
}
