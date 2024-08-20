using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class NotificationService : BaseService<Notification, int>, INotificationService
{
    #region Variables
    private readonly ITNOElasticClient _elasticClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a NotificationService object, initializes with specified parameters.
    /// </summary>
    /// <param name="elasticClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public NotificationService(
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<NotificationService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Search for all notifications that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Notification> Find(NotificationFilter filter)
    {
        var query = this.Context.Notifications
            .AsNoTracking()
            .Include(n => n.Template)
            .Include(n => n.SubscribersManyToMany).ThenInclude(s => s.User)
            .AsQueryable();

        if (filter.AlertOnIndex.HasValue)
            query = query.Where(n => n.AlertOnIndex == filter.AlertOnIndex);
        if (filter.NotificationType.HasValue)
            query = query.Where(n => n.NotificationType == filter.NotificationType);
        if (filter.OwnerId.HasValue)
            query = query.Where(n => n.OwnerId == filter.OwnerId.Value);
        if (filter.SubscriberUserId.HasValue)
            query = query.Where(n => n.SubscribersManyToMany.Any(ns => ns.IsSubscribed && ns.UserId == filter.SubscriberUserId.Value));
        if (filter.IsEnabled.HasValue)
            query = query.Where(n => n.IsEnabled == filter.IsEnabled);
        if (filter.Ids?.Any() == true)
            query = query.Where(n => filter.Ids.Contains(n.Id));

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(u => u.Name);

        if (filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

        return query.ToArray();
    }

    /// <summary>
    /// Find the notification for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Notification? FindById(int id)
    {
        return this.Context.Notifications
            .Include(n => n.Template)
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

        // Add/Update the report template.
        if (entity.Template != null)
        {
            if (entity.Template.Id == 0)
            {
                // A new template has been provided.
                this.Context.Add(entity.Template);
            }
        }

        return base.Add(entity);
    }

    /// <summary>
    /// Update the report in the database.
    /// Update subscribers of the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Notification Update(Notification entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
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
            else
            {
                if (current.IsSubscribed != s.IsSubscribed)
                    current.IsSubscribed = s.IsSubscribed;
            }
        });


        // Add/Update the report template.
        if (entity.Template != null)
        {
            if (entity.Template.Id == 0)
            {
                // A new template has been provided.
                this.Context.Add(entity.Template);
                original.Template = entity.Template;
            }
            else
            {
                if (original.Template == null)
                {
                    // A different template has been assigned.
                    if (original.TemplateId == entity.TemplateId)
                        this.Context.Entry(entity.Template).State = EntityState.Modified;
                    else
                        this.Context.Entry(entity.Template).State = EntityState.Added;

                    original.Template = entity.Template;
                }
                else if (original.TemplateId == entity.TemplateId)
                {
                    // Update the existing template.
                    original.Template.Name = entity.Template.Name;
                    original.Template.Description = entity.Template.Description;
                    original.Template.SortOrder = entity.Template.SortOrder;
                    original.Template.IsEnabled = entity.Template.IsEnabled;
                    original.Template.Subject = entity.Template.Subject;
                    original.Template.Body = entity.Template.Body;
                    original.Template.Settings = entity.Template.Settings;
                    original.Template.Version = entity.Template.Version;
                }
                else
                {
                    // A different template is now associated to this report.
                    this.Context.Entry(entity.Template).State = EntityState.Modified;
                    original.Template = entity.Template;
                }
            }
        }

        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);

        return base.Update(original);
    }

    /// <summary>
    /// Delete the notification and the template if no other notification uses it.
    /// </summary>
    /// <param name="entity"></param>
    public override void Delete(Notification entity)
    {
        // If the template isn't being used by anything else, delete it.
        var isTemplateUsed = this.Context.Notifications.Any(nt => nt.Id != entity.Id && nt.TemplateId == entity.TemplateId);
        if (!isTemplateUsed)
        {
            var template = this.Context.NotificationTemplates.Find(entity.TemplateId);
            if (template != null)
                this.Context.Remove(template);
        }

        base.Delete(entity);
    }

    /// <summary>
    /// Unsubscribe this user from all notifications.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> Unsubscribe(int userId)
    {
        var saveChanges = false;
        var notifications = this.Context.UserNotifications.Where(x => x.UserId == userId);
        notifications.ForEach(s =>
        {
            if (s.IsSubscribed)
            {
                s.IsSubscribed = !s.IsSubscribed;
                if (!saveChanges) saveChanges = true;
            }
        });
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    /// <summary>
    /// Find the last notification instance created for the specified 'notificationId' and 'ownerId'.
    /// </summary>
    /// <param name="notificationId"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    private NotificationInstance? GetPreviousNotificationInstance(int notificationId, int? ownerId)
    {
        return this.Context.NotificationInstances
            .OrderByDescending(i => i.Id)
            .Where(i => i.NotificationId == notificationId &&
                i.OwnerId == ownerId)
            .FirstOrDefault();
    }

    /// <summary>
    /// Make a request to Elasticsearch to find content for the specified 'notification'.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    public async Task<Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>> FindContentWithElasticsearchAsync(Notification notification, int? requestorId)
    {
        var filter = JsonSerializer.Deserialize<FilterSettingsModel>(notification.Settings.ToJson(), _serializerOptions) ?? new();

        var ownerId = requestorId ?? notification.OwnerId; // TODO: Handle users generating instances for a notifications they do not own.

        var defaultIndex = filter.SearchUnpublished ? _elasticOptions.UnpublishedIndex : _elasticOptions.PublishedIndex;
        var query = notification.Query;

        // Fetch all notifications within the offset of the requested notification filter.
        // Need to identify all content already notified by this notification so that we don't resend.
        var settings = JsonSerializer.Deserialize<FilterSettingsModel>(notification.Settings, _serializerOptions);
        if (settings != null && (settings.DateOffset.HasValue || settings.StartDate.HasValue))
        {
            var statuses = new[] { NotificationStatus.Pending, NotificationStatus.Accepted, NotificationStatus.Completed };
            var dateOffset = settings.DateOffset.HasValue ? DateTime.UtcNow.AddDays(-1 * settings.DateOffset.Value) : (DateTime?)null;
            var sentNotificationContentIds = this.Context.NotificationInstances.Where(ni => ni.NotificationId == notification.Id
                && (ownerId == null || ni.OwnerId == ownerId)
                && statuses.Contains(ni.Status)
                && ni.SentOn != null
                && (dateOffset == null || ni.SentOn >= dateOffset)
                && (settings.StartDate == null | ni.SentOn >= settings.StartDate))
                .Select(ni => ni.ContentId).Distinct().ToArray();

            if (sentNotificationContentIds.Any())
                query = query.AddExcludeContent(sentNotificationContentIds);
        }

        return await _elasticClient.SearchAsync<API.Areas.Services.Models.Content.ContentModel>(defaultIndex, query);
    }

    /// <summary>
    /// Subscribe the user to the content so that they receive the specified notification.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="contentId"></param>
    public void SubscriberUserToContent(int userId, long contentId)
    {
        var userContentNotifications = this.Context.UserContentNotifications.Where(ucn => ucn.UserId == userId && ucn.ContentId == contentId).Any();
        if (!userContentNotifications)
        {
            this.Context.Add(new UserContentNotification(userId, contentId, true));
            this.Context.CommitTransaction();
        }
    }
    #endregion
}
