using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// AutomationProfileService class, provides persistence for automation profiles, their ordered steps,
/// and each step's ordered actions.
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
    /// Find all automation profiles with their steps and actions.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<AutomationProfile> FindAll()
    {
        return this.Context.AutomationProfiles
            .AsNoTracking()
            .Include(p => p.Filter)
            .Include(p => p.Steps).ThenInclude(s => s.Actions).ThenInclude(a => a.Filter)
            .Include(p => p.Steps).ThenInclude(s => s.Filter)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
            .ToArray();
    }

    /// <summary>
    /// Find the automation profile for the specified 'id' with its steps and actions.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override AutomationProfile? FindById(int id)
    {
        return this.Context.AutomationProfiles
            .AsNoTracking()
            .Include(p => p.Filter)
            .Include(p => p.Steps).ThenInclude(s => s.Actions).ThenInclude(a => a.Filter)
            .Include(p => p.Steps).ThenInclude(s => s.Filter)
            .Include(p => p.Events).ThenInclude(e => e.Schedule)
            .FirstOrDefault(p => p.Id == id);
    }

    /// <summary>
    /// Add the automation profile and its step/action graph to the database.
    /// Children must be added explicitly because the base Add only attaches the root entity.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override AutomationProfile Add(AutomationProfile entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        entity.Steps.ForEach(step =>
        {
            step.AutomationProfile = entity;
            this.Context.Add(step);
            step.Actions.ForEach(action =>
            {
                action.AutomationStep = step;
                this.Context.Add(action);
            });
        });
        entity.Events.ForEach(scheduleEvent =>
        {
            scheduleEvent.AutomationProfile = entity;
            if (scheduleEvent.Schedule != null) this.Context.Add(scheduleEvent.Schedule);
            this.Context.Add(scheduleEvent);
        });
        return base.Add(entity);
    }

    /// <summary>
    /// Update the automation profile, reconciling its step/action graph (add new, update existing, delete removed).
    /// The reconciliation is applied to the tracked original entity so audit columns are preserved.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override AutomationProfile Update(AutomationProfile entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var original = this.Context.AutomationProfiles
            .Include(p => p.Steps).ThenInclude(s => s.Actions)
            .Include(p => p.Events).ThenInclude(e => e.Schedule)
            .FirstOrDefault(p => p.Id == entity.Id)
            ?? throw new InvalidOperationException($"Automation profile '{entity.Id}' does not exist.");

        // Profile scalars.
        original.Name = entity.Name;
        original.Description = entity.Description;
        original.IsEnabled = entity.IsEnabled;
        original.SchemaVersion = entity.SchemaVersion;
        original.Definition = entity.Definition;
        original.FilterId = entity.FilterId;
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

        // Delete steps that are no longer present.
        foreach (var removedStep in original.Steps.Where(os => entity.Steps.All(s => s.Id != os.Id)).ToArray())
        {
            this.Context.Remove(removedStep);
        }

        foreach (var incomingStep in entity.Steps)
        {
            var originalStep = incomingStep.Id != 0
                ? original.Steps.FirstOrDefault(os => os.Id == incomingStep.Id)
                : null;

            if (originalStep == null)
            {
                // New step; explicitly add it and its actions so they are tracked as Added.
                incomingStep.AutomationProfileId = original.Id;
                this.Context.Add(incomingStep);
                incomingStep.Actions.ForEach(action =>
                {
                    action.AutomationStep = incomingStep;
                    this.Context.Add(action);
                });
                continue;
            }

            // Existing step scalars.
            originalStep.Name = incomingStep.Name;
            originalStep.Description = incomingStep.Description;
            originalStep.Prompt = incomingStep.Prompt;
            originalStep.SortOrder = incomingStep.SortOrder;
            originalStep.Target = incomingStep.Target;
            originalStep.FilterId = incomingStep.FilterId;
            originalStep.ApplyToAutomationFilter = incomingStep.ApplyToAutomationFilter;
            originalStep.IterateStepFilter = incomingStep.IterateStepFilter;
            originalStep.LLMId = incomingStep.LLMId;
            originalStep.SendSeparatePrompts = incomingStep.SendSeparatePrompts;
            originalStep.UseChatCompletions = incomingStep.UseChatCompletions;
            originalStep.IsEnabled = incomingStep.IsEnabled;

            // Delete actions that are no longer present.
            foreach (var removedAction in originalStep.Actions.Where(oa => incomingStep.Actions.All(a => a.Id != oa.Id)).ToArray())
            {
                this.Context.Remove(removedAction);
            }

            foreach (var incomingAction in incomingStep.Actions)
            {
                var originalAction = incomingAction.Id != 0
                    ? originalStep.Actions.FirstOrDefault(oa => oa.Id == incomingAction.Id)
                    : null;

                if (originalAction == null)
                {
                    // New action; explicitly add it so it is tracked as Added.
                    incomingAction.AutomationStepId = originalStep.Id;
                    this.Context.Add(incomingAction);
                    continue;
                }

                originalAction.Name = incomingAction.Name;
                originalAction.Prompt = incomingAction.Prompt;
                originalAction.ActionType = incomingAction.ActionType;
                originalAction.MaxCalls = incomingAction.MaxCalls;
                originalAction.ConfirmationStatement = incomingAction.ConfirmationStatement;
                originalAction.ContentField = incomingAction.ContentField;
                originalAction.ContentActionId = incomingAction.ContentActionId;
                originalAction.ReportId = incomingAction.ReportId;
                originalAction.NotificationId = incomingAction.NotificationId;
                originalAction.FilterId = incomingAction.FilterId;
                originalAction.PriorActionId = incomingAction.PriorActionId;
                originalAction.AutoExecute = incomingAction.AutoExecute;
                originalAction.AbortIfNoConfirmation = incomingAction.AbortIfNoConfirmation;
                originalAction.LLMId = incomingAction.LLMId;
                originalAction.Objective = incomingAction.Objective;
                originalAction.WorksOn = incomingAction.WorksOn;
                originalAction.CreateIdentifier = incomingAction.CreateIdentifier;
                originalAction.CreateClone = incomingAction.CreateClone;
                originalAction.Settings = incomingAction.Settings;
                originalAction.IsEnabled = incomingAction.IsEnabled;
                originalAction.SortOrder = incomingAction.SortOrder;
            }
        }

        this.Context.ResetVersion(original);
        this.Context.UpdateCache<AutomationProfile>();
        return original;
    }
    #endregion
}
