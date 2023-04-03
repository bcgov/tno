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
    /// get/set - The Razor template to generate the report.
    /// </summary>
    public string Template { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to user who owns this report.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether this report is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public virtual IEnumerable<UserReportModel> Subscribers { get; } = Array.Empty<UserReportModel>();
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
        this.Template = entity.Template;
        this.OwnerId = entity.OwnerId;
        this.IsPublic = entity.IsPublic;
        this.Filter = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Filter, options) ?? new Dictionary<string, object>();
        this.Settings = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings, options) ?? new Dictionary<string, object>();

        this.Subscribers = entity.SubscribersManyToMany.Select(m => new UserReportModel(m));
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
        var entity = new Entities.Report(model.Id, model.Name, model.ReportType, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            SortOrder = model.SortOrder,
            Template = model.Template,
            IsPublic = model.IsPublic,
            Filter = JsonDocument.Parse(JsonSerializer.Serialize(model.Filter)),
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => (UserReport)us));

        return entity;
    }
    #endregion
}
