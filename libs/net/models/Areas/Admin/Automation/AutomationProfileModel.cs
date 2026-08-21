using System.Text.Json.Serialization;

namespace TNO.API.Areas.Admin.Models.Automation;

/// <summary>
/// AutomationProfileModel class, represents an automation configuration profile.
/// </summary>
public class AutomationProfileModel
{
    #region Properties
    /// <summary>
    /// get/set - Identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Profile name.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether profile is active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// get/set - Schema version.
    /// </summary>
    public int SchemaVersion { get; set; } = 2;

    /// <summary>
    /// get/set - The profile definition document as raw JSON (prompts library, steps, analyses,
    /// actions); validated against the action catalog on save.
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>
    /// get/set - LLM identifier.
    /// </summary>
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - Schedules (event schedules fired by the scheduler service).
    /// </summary>
    public IEnumerable<AutomationScheduleModel> Schedules { get; set; } = Array.Empty<AutomationScheduleModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AutomationProfileModel.
    /// </summary>
    public AutomationProfileModel() { }

    /// <summary>
    /// Creates a new instance of an AutomationProfileModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AutomationProfileModel(Entities.AutomationProfile entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.SchemaVersion = entity.SchemaVersion;
        this.Definition = entity.Definition?.RootElement.GetRawText();
        this.LLMId = entity.LLMId;
        this.Schedules = entity.Events
            .Where(e => e.EventType == Entities.EventScheduleType.Automation)
            .OrderBy(e => e.Id)
            .Select(e => new AutomationScheduleModel(e))
            .ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationProfile entity from this model, including its schedules.
    /// </summary>
    /// <returns></returns>
    public Entities.AutomationProfile ToEntity()
    {
        var entity = new Entities.AutomationProfile(this.Id, this.Name)
        {
            Description = this.Description,
            IsEnabled = this.IsEnabled,
            SchemaVersion = this.SchemaVersion,
            Definition = !string.IsNullOrWhiteSpace(this.Definition)
                ? System.Text.Json.JsonDocument.Parse(this.Definition)
                : null,
            LLMId = this.LLMId,
        };

        foreach (var schedule in this.Schedules)
        {
            var name = string.IsNullOrWhiteSpace(schedule.Name) ? this.Name : schedule.Name;
            entity.Events.Add(new Entities.EventSchedule(name, Entities.EventScheduleType.Automation, new Entities.Schedule(name, 0)
            {
                IsEnabled = schedule.IsEnabled,
                StartAt = schedule.StartAt,
                RunOn = schedule.RunOn,
                RunOnWeekDays = schedule.GetRunOnWeekDays(),
            })
            {
                Id = schedule.Id,
                // The scheduler skips event schedules that are not enabled, so gate the event on the
                // profile as well; a disabled profile would otherwise still queue a run every day
                // that the automation service can only fail. The schedule keeps its own IsEnabled
                // (above), so re-enabling the profile restores each schedule's setting.
                IsEnabled = this.IsEnabled && schedule.IsEnabled,
                AutomationProfileId = this.Id,
            });
        }

        return entity;
    }
    #endregion
}
