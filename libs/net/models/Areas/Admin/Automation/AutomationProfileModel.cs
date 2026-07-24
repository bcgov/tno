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
    public int SchemaVersion { get; set; } = 1;

    /// <summary>
    /// get/set - Optional Elasticsearch filter used to select content for iteration.
    /// </summary>
    public int? FilterId { get; set; }

    /// <summary>
    /// get - The Elasticsearch query of the profile filter (read-only; provided for the automation service).
    /// </summary>
    public string? FilterQuery { get; set; }

    /// <summary>
    /// get - The settings of the profile filter (read-only; provided for the automation service).
    /// </summary>
    public string? FilterSettings { get; set; }

    /// <summary>
    /// get/set - LLM identifier.
    /// </summary>
    public int? LLMId { get; set; }

    /// <summary>
    /// get/set - Schedules (event schedules fired by the scheduler service).
    /// </summary>
    public IEnumerable<AutomationScheduleModel> Schedules { get; set; } = Array.Empty<AutomationScheduleModel>();

    /// <summary>
    /// get/set - Ordered automation steps.
    /// </summary>
    [JsonPropertyName("steps")]
    public IEnumerable<AutomationStepModel> Steps { get; set; } = Array.Empty<AutomationStepModel>();

    /// <summary>
    /// get/set - Legacy alias for step payload.
    /// </summary>
    [JsonPropertyName("rules")]
    public IEnumerable<AutomationStepModel> Rules
    {
        get => this.Steps;
        set
        {
            if (this.Steps.Any()) return;
            this.Steps = value ?? Array.Empty<AutomationStepModel>();
        }
    }
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
        this.FilterId = entity.FilterId;
        this.FilterQuery = entity.Filter?.Query.RootElement.GetRawText();
        this.FilterSettings = entity.Filter?.Settings.RootElement.GetRawText();
        this.LLMId = entity.LLMId;
        this.Schedules = entity.Events
            .Where(e => e.EventType == Entities.EventScheduleType.Automation)
            .OrderBy(e => e.Id)
            .Select(e => new AutomationScheduleModel(e))
            .ToArray();
        this.Steps = entity.Steps
            .OrderBy(s => s.SortOrder)
            .Select(s => new AutomationStepModel(s))
            .ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new AutomationProfile entity from this model, including its step/action graph.
    /// </summary>
    /// <returns></returns>
    public Entities.AutomationProfile ToEntity()
    {
        var entity = new Entities.AutomationProfile(this.Id, this.Name)
        {
            Description = this.Description,
            IsEnabled = this.IsEnabled,
            SchemaVersion = this.SchemaVersion,
            FilterId = this.FilterId,
            LLMId = this.LLMId,
        };

        foreach (var schedule in this.Schedules)
        {
            var name = string.IsNullOrWhiteSpace(schedule.Name) ? this.Name : schedule.Name;
            entity.Events.Add(new Entities.EventSchedule(name, Entities.EventScheduleType.Automation, new Entities.Schedule(name, 0)
            {
                IsEnabled = schedule.IsEnabled,
                StartAt = schedule.StartAt,
                RunOnWeekDays = schedule.GetRunOnWeekDays(),
            })
            {
                Id = schedule.Id,
                IsEnabled = schedule.IsEnabled,
                AutomationProfileId = this.Id,
            });
        }

        foreach (var step in this.Steps)
        {
            entity.Steps.Add(step.ToEntity(this.Id));
        }

        return entity;
    }
    #endregion
}
