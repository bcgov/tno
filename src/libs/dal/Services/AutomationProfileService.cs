using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AutomationProfileService class, provides persistence for automation profiles (v2 definition
/// documents) and their scheduler events.
/// </summary>
public class AutomationProfileService : BaseService<AutomationProfile, int>, IAutomationProfileService
{
    #region Constructors
    public AutomationProfileService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IServiceProvider serviceProvider,
        ILogger<AutomationProfileService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all automation profiles.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AutomationProfile> FindAll()
    {
        return this.Context.AutomationProfiles
            .AsNoTracking()
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .ToArray();
    }

    /// <summary>
    /// Find the automation profile for the specified 'id' with its schedules.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override AutomationProfile? FindById(int id)
    {
        return this.Context.AutomationProfiles
            .AsNoTracking()
            .Include(p => p.Events).ThenInclude(e => e.Schedule)
            .FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Add the automation profile and its scheduler events to the database.
    /// Children must be added explicitly because the base Add only attaches the root entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override AutomationProfile Add(AutomationProfile entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.Events.ForEach(scheduleEvent =>
        {
            scheduleEvent.AutomationProfile = entity;
            if (scheduleEvent.Schedule != null) this.Context.Add(scheduleEvent.Schedule);
            this.Context.Add(scheduleEvent);
        });
        return base.Add(entity);
    }

    /// <summary>
    /// Update the automation profile, reconciling its scheduler events.
    /// The reconciliation is applied to the tracked original entity so audit columns are preserved.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override AutomationProfile Update(AutomationProfile entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var original = this.Context.AutomationProfiles
            .Include(p => p.Events).ThenInclude(e => e.Schedule)
            .FirstOrDefault(p => p.Id == entity.Id)
            ?? throw new InvalidOperationException($"Automation profile '{entity.Id}' does not exist.");

        // Profile scalars.
        original.Name = entity.Name;
        original.Description = entity.Description;
        original.IsEnabled = entity.IsEnabled;
        original.SchemaVersion = entity.SchemaVersion;
        original.Definition = entity.Definition;
        original.LLMId = entity.LLMId;
        original.SortOrder = entity.SortOrder;

        // Reconcile the profile's scheduler events (Automation event schedules).
        var incomingEvents = entity.Events.Where(e => e.EventType == EventScheduleType.Automation).ToArray();
        var originalEvents = original.Events.Where(e => e.EventType == EventScheduleType.Automation).ToArray();

        // Delete schedules that are no longer present (and their schedule rows).
        foreach (var removedEvent in originalEvents.Where(oe => incomingEvents.All(e => e.Id != oe.Id)))
        {
            this.Context.Remove(removedEvent);
            if (removedEvent.Schedule != null) this.Context.Remove(removedEvent.Schedule);
        }

        foreach (var incomingEvent in incomingEvents)
        {
            var originalEvent = incomingEvent.Id != 0
                ? originalEvents.FirstOrDefault(oe => oe.Id == incomingEvent.Id)
                : null;

            if (originalEvent == null)
            {
                incomingEvent.AutomationProfile = original;
                incomingEvent.AutomationProfileId = original.Id;
                if (incomingEvent.Schedule != null) this.Context.Add(incomingEvent.Schedule);
                this.Context.Add(incomingEvent);
                continue;
            }

            originalEvent.Name = incomingEvent.Name;
            originalEvent.IsEnabled = incomingEvent.IsEnabled;
            if (originalEvent.Schedule != null && incomingEvent.Schedule != null)
            {
                originalEvent.Schedule.Name = incomingEvent.Name;
                originalEvent.Schedule.IsEnabled = incomingEvent.Schedule.IsEnabled;
                originalEvent.Schedule.StartAt = incomingEvent.Schedule.StartAt;
                originalEvent.Schedule.RunOn = incomingEvent.Schedule.RunOn;
                originalEvent.Schedule.RunOnWeekDays = incomingEvent.Schedule.RunOnWeekDays;
            }
        }

        this.Context.ResetVersion(original);
        this.Context.UpdateCache<AutomationProfile>();
        return original;
    }
    #endregion
}
