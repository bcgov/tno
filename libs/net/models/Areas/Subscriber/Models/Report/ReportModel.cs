using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Subscriber.Models.Report;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class ReportModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the report template.
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// get/set - The report template.
    /// </summary>
    public ReportTemplateModel? Template { get; set; }

    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether this report is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public ReportSettingsModel Settings { get; set; } = new();

    /// <summary>
    /// get/set - An array of report sections.
    /// </summary>
    public IEnumerable<ReportSectionModel> Sections { get; set; } = Array.Empty<ReportSectionModel>();

    /// <summary>
    /// get - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public virtual IEnumerable<UserReportModel> Subscribers { get; set; } = Array.Empty<UserReportModel>();

    /// <summary>
    /// get/set - An array of event schedules to auto run this report.
    /// </summary>
    public IEnumerable<ReportScheduleModel> Events { get; set; } = Array.Empty<ReportScheduleModel>();

    /// <summary>
    /// get/set - An array of report instances.
    /// </summary>
    public IEnumerable<ReportInstanceModel> Instances { get; set; } = Array.Empty<ReportInstanceModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportModel.
    /// </summary>
    public ReportModel() { }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportModel(Entities.Report entity, JsonSerializerOptions options) : base(entity)
    {
        this.TemplateId = entity.TemplateId;
        this.Template = entity.Template != null ? new ReportTemplateModel(entity.Template, options) : null;
        this.OwnerId = entity.OwnerId;
        this.IsPublic = entity.IsPublic;
        this.Settings = new ReportSettingsModel(JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>(), options);
        this.Sections = entity.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s, options)).ToArray();
        this.Subscribers = entity.SubscribersManyToMany.Select(s => new UserReportModel(s)).ToArray();
        this.Events = entity.Events.Select(s => new ReportScheduleModel(s)).ToArray();
        this.Instances = entity.Instances.OrderByDescending(i => i.Id).Select(s => new ReportInstanceModel(s)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.Report ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Report)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        entity.Sections.ForEach(s =>
        {
            var section = this.Sections.FirstOrDefault(us => us.Name == s.Name) ?? throw new InvalidOperationException("Unable to find matching section");
            s.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Settings, options));
            s.Folder = null;
            s.Filter = null;
            s.LinkedReport = null;
            s.ChartTemplatesManyToMany.ForEach(ct =>
            {
                var chart = section.ChartTemplates.FirstOrDefault(uct => uct.Id == ct.ChartTemplateId) ?? throw new InvalidOperationException("Unable to find matching chart template");
                ct.Settings = JsonDocument.Parse(JsonSerializer.Serialize(chart.SectionSettings, options));
                ct.ChartTemplate = null;
            });
        });
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Report(ReportModel model)
    {
        var entity = new Entities.Report(model.Id, model.Name, model.TemplateId, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            IsPublic = model.IsPublic,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        entity.Sections.AddRange(model.Sections.OrderBy(s => s.SortOrder).Select(modelSection =>
        {
            var section = new Entities.ReportSection(modelSection.Id, modelSection.Name, modelSection.SectionType, modelSection.ReportId)
            {
                Description = modelSection.Description,
                IsEnabled = modelSection.IsEnabled,
                SortOrder = modelSection.SortOrder,
                FilterId = modelSection.FilterId,
                Filter = modelSection.Filter != null ? new Entities.Filter(modelSection.Filter.Id, modelSection.Filter.Name, modelSection.Filter.OwnerId)
                {
                    Description = modelSection.Filter.Description,
                    IsEnabled = modelSection.Filter.IsEnabled,
                    SortOrder = modelSection.Filter.SortOrder,
                    Settings = JsonDocument.Parse(JsonSerializer.Serialize(modelSection.Filter.Settings)),
                    Query = JsonDocument.Parse(JsonSerializer.Serialize(modelSection.Filter.Query))
                } : null,
                FolderId = modelSection.FolderId,
                Folder = modelSection.Folder != null ? new Entities.Folder(modelSection.Folder.Id, modelSection.Folder.Name, modelSection.Folder.OwnerId)
                {
                    Description = modelSection.Folder.Description,
                    IsEnabled = modelSection.Folder.IsEnabled,
                    SortOrder = modelSection.Folder.SortOrder,
                    Settings = JsonDocument.Parse(JsonSerializer.Serialize(modelSection.Folder.Settings))
                } : null,
                LinkedReportId = modelSection.LinkedReportId,
                Settings = JsonDocument.Parse(JsonSerializer.Serialize(modelSection.Settings)),
                Version = modelSection.Version ?? 0
            };
            section.ChartTemplatesManyToMany.AddRange(modelSection.ChartTemplates.OrderBy(ct => ct.SortOrder).Select(ct => new Entities.ReportSectionChartTemplate(modelSection.Id, ct.Id, ct.SortOrder)
            {
                Settings = ct.SectionSettings != null ? JsonDocument.Parse(JsonSerializer.Serialize(ct.SectionSettings)) : JsonDocument.Parse(JsonSerializer.Serialize(new ChartSectionSettingsModel())),
            }));
            return section;
        }));

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new Entities.UserReport(us.UserId, entity.Id, us.IsSubscribed, us.Format, us.SendTo)));

        entity.Events.AddRange(model.Events.Select(s =>
        {
            return new Entities.EventSchedule(s.Name, Entities.EventScheduleType.Report, s.ScheduleId, s.Settings)
            {
                Id = s.Id,
                Description = s.Description,
                IsEnabled = s.IsEnabled,
                RequestSentOn = s.RequestSentOn,
                LastRanOn = s.LastRanOn,
                ReportId = model.Id,
                Schedule = new Entities.Schedule(s.Name, s.DelayMS)
                {
                    Id = s.ScheduleId,
                    Description = s.Description,
                    IsEnabled = s.IsEnabled,
                    RunOn = s.RunOn,
                    StartAt = s.StartAt,
                    StopAt = s.StopAt,
                    RunOnlyOnce = s.RunOnlyOnce,
                    Repeat = s.Repeat,
                    RunOnWeekDays = s.RunOnWeekDays,
                    RunOnMonths = s.RunOnMonths,
                    DayOfMonth = s.DayOfMonth,
                    RequestedById = s.RequestedById,
                    Version = s.ScheduleVersion ?? 0
                },
                Version = s.Version ?? 0
            };
        }));

        return entity;
    }
    #endregion
}
