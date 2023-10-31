using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.Entities;

namespace TNO.DAL.Services;

public class NotificationTemplateService : BaseService<NotificationTemplate, int>, INotificationTemplateService
{
    #region Variables
    #endregion

    #region Constructors
    public NotificationTemplateService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<NotificationTemplateService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the notification templates.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<NotificationTemplate> FindAll()
    {
        return this.Context.NotificationTemplates
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find all the public notification templates.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<NotificationTemplate> FindPublic()
    {
        return this.Context.NotificationTemplates
            .AsNoTracking()
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name)
            .Where(r => r.IsPublic)
            .ToArray();
    }

    /// <summary>
    /// Find the notification template for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override NotificationTemplate? FindById(int id)
    {
        return this.Context.NotificationTemplates
            .FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Add the new notification template to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override NotificationTemplate Add(NotificationTemplate entity)
    {
        return base.Add(entity);
    }

    /// <summary>
    /// Update the notification template in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override NotificationTemplate Update(NotificationTemplate entity)
    {
        return base.Update(entity);
    }

    /// <summary>
    /// Determine if this notification template is being used by any notifications.
    /// </summary>
    /// <param name="templateId"></param>
    /// <returns></returns>
    public bool IsInUse(int templateId)
    {
        return this.Context.Notifications.Any(rt => rt.TemplateId == templateId);
    }
    #endregion
}
