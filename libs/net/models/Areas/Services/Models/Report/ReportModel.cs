using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Services.Models.Report;

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
        this.Settings = JsonSerializer.Deserialize<ReportSettingsModel>(entity.Settings, options) ?? new();
        this.Sections = entity.Sections.OrderBy(s => s.SortOrder).Select(s => new ReportSectionModel(s, options));
        this.Subscribers = entity.SubscribersManyToMany.Select(s => new UserReportModel(s));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    /// <returns></returns>
    public Entities.Report ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.Report)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        entity.Sections.ForEach(s =>
        {
            var section = this.Sections.FirstOrDefault(us => us.Name == s.Name) ?? throw new InvalidOperationException("Unable to find matching section");
            s.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Settings, options));
            if (section.Folder != null && s.Folder != null) s.Folder.Settings = section.Folder.Settings;
            if (section.Filter != null && s.Filter != null)
            {
                s.Filter.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Filter.Settings, options));
                s.Filter.Query = section.Filter.Query;
            }
            s.ChartTemplatesManyToMany.ForEach(ct =>
            {
                var chart = section.ChartTemplates.FirstOrDefault(uct => uct.Id == ct.ChartTemplateId) ?? throw new InvalidOperationException("Unable to find matching chart template");
                ct.Settings = JsonDocument.Parse(JsonSerializer.Serialize(chart.Settings, options));
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

        if (model.Template != null)
        {
            entity.TemplateId = model.TemplateId;
            entity.Template = (Entities.ReportTemplate)model.Template;
        }

        entity.Sections.AddRange(model.Sections.OrderBy(s => s.SortOrder).Select(modelSection =>
        {
            var section = new Entities.ReportSection(modelSection.Id, modelSection.Name, modelSection.ReportId)
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
                    Query = modelSection.Filter.Query
                } : null,
                FolderId = modelSection.FolderId,
                Folder = modelSection.Folder != null ? new Entities.Folder(modelSection.Folder.Id, modelSection.Folder.Name, modelSection.Folder.OwnerId)
                {
                    Description = modelSection.Folder.Description,
                    IsEnabled = modelSection.Folder.IsEnabled,
                    SortOrder = modelSection.Folder.SortOrder,
                    Settings = modelSection.Folder.Settings
                } : null,
                Settings = JsonDocument.Parse(JsonSerializer.Serialize(modelSection.Settings)),
                Version = modelSection.Version ?? 0
            };
            section.ChartTemplatesManyToMany.AddRange(modelSection.ChartTemplates.OrderBy(ct => ct.SortOrder).Select(ct => new Entities.ReportSectionChartTemplate(modelSection.Id, ct.Id, ct.SortOrder)
            {
                ChartTemplate = new Entities.ChartTemplate(ct.Id, ct.Name, ct.Template)
                {
                    Description = ct.Description,
                    IsEnabled = ct.IsEnabled,
                    SortOrder = ct.SortOrder,
                    Settings = JsonDocument.Parse(JsonSerializer.Serialize(ct.Settings)),
                },
                Settings = ct.SectionSettings != null ? JsonDocument.Parse(JsonSerializer.Serialize(ct.SectionSettings)) : JsonDocument.Parse(JsonSerializer.Serialize(new ChartSectionSettingsModel())),
            }));
            return section;
        }));

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new Entities.UserReport(us.UserId, entity.Id, us.IsSubscribed)));

        return entity;
    }
    #endregion
}
