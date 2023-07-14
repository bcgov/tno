using System.Text.Json;
using TNO.API.Models;
using TNO.API.Models.Settings;

namespace TNO.API.Areas.Admin.Models.Report;

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
    /// get/set - The owner of this report.
    /// </summary>
    public UserModel? Owner { get; set; }

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
    /// get/set - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public IEnumerable<UserModel> Subscribers { get; set; } = Array.Empty<UserModel>();

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
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.IsPublic = entity.IsPublic;
        this.Settings = new ReportSettingsModel(JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>(), options);
        this.Sections = entity.Sections.Select(s => new ReportSectionModel(s, options));
        this.Subscribers = entity.SubscribersManyToMany.Where(s => s.User != null).Select(s => new UserModel(s.User!));
        this.Instances = entity.Instances.OrderByDescending(i => i.Id).Select(i => new ReportInstanceModel(i, options));
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
            var section = this.Sections.FirstOrDefault(us => us.Id == s.Id || us.Name == s.Name) ?? throw new InvalidOperationException("Unable to find matching section");
            s.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Settings, options));
            if (section.Folder != null && s.Folder != null) s.Folder.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Folder.Settings, options));
            if (section.Filter != null && s.Filter != null)
            {
                s.Filter.Settings = JsonDocument.Parse(JsonSerializer.Serialize(section.Filter.Settings, options));
                s.Filter.Query = JsonDocument.Parse(JsonSerializer.Serialize(section.Filter.Query, options));
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

        entity.Sections.AddRange(model.Sections.Select(s =>
        {
            var section = new Entities.ReportSection(s.Id, s.Name, s.ReportId)
            {
                Description = s.Description,
                IsEnabled = s.IsEnabled,
                SortOrder = s.SortOrder,
                FilterId = s.FilterId,
                Filter = s.Filter != null ? new Entities.Filter(s.Filter.Id, s.Filter.Name, s.Filter.OwnerId)
                {
                    Description = s.Filter.Description,
                    IsEnabled = s.Filter.IsEnabled,
                    SortOrder = s.Filter.SortOrder,
                    Settings = JsonDocument.Parse(JsonSerializer.Serialize(s.Filter.Settings)),
                    Query = JsonDocument.Parse(JsonSerializer.Serialize(s.Filter.Query))
                } : null,
                FolderId = s.FolderId,
                Folder = s.Folder != null ? new Entities.Folder(s.Folder.Id, s.Folder.Name, s.Folder.OwnerId)
                {
                    Description = s.Folder.Description,
                    IsEnabled = s.Folder.IsEnabled,
                    SortOrder = s.Folder.SortOrder,
                    Settings = JsonDocument.Parse(JsonSerializer.Serialize(s.Folder.Settings))
                } : null,
                Settings = JsonDocument.Parse(JsonSerializer.Serialize(s.Settings)),
                Version = s.Version ?? 0
            };
            section.ChartTemplatesManyToMany.AddRange(s.ChartTemplates.Select(ct => new Entities.ReportSectionChartTemplate(s.Id, ct.Id, ct.SortOrder)
            {
                Settings = JsonDocument.Parse(JsonSerializer.Serialize(ct.Settings)),
            }));
            return section;
        }));

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new Entities.UserReport(us.Id, entity.Id)));

        return entity;
    }
    #endregion
}
