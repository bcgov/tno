using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Report;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class ReportModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The type of report.
    /// </summary>
    public ReportType ReportType { get; set; }

    /// <summary>
    /// get/set - The default filter for this report.
    /// </summary>
    public Dictionary<string, object> Filter { get; set; } = new Dictionary<string, object>();

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
    public int OwnerId { get; set; }

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
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

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
        this.ReportType = entity.ReportType;
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.IsPublic = entity.IsPublic;
        this.TemplateId = entity.TemplateId;
        this.Template = entity.Template != null ? new ReportTemplateModel(entity.Template) : null;
        this.Filter = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Filter, options) ?? new Dictionary<string, object>();
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();
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
        entity.Filter = JsonDocument.Parse(JsonSerializer.Serialize(this.Filter, options));
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Report(ReportModel model)
    {
        var entity = new Entities.Report(model.Id, model.Name, model.ReportType, model.OwnerId, model.TemplateId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            IsPublic = model.IsPublic,
            Filter = JsonDocument.Parse(JsonSerializer.Serialize(model.Filter)),
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        if (model.Template != null)
        {
            entity.TemplateId = model.TemplateId;
            entity.Template = (Entities.ReportTemplate)model.Template;
        }

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new UserReport(us.Id, entity.Id)));

        return entity;
    }
    #endregion
}
