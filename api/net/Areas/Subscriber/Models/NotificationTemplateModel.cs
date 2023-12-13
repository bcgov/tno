using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models;

/// <summary>
/// NotificationTemplateModel class, provides a model that represents an report template.
/// </summary>
public class NotificationTemplateModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The Razor subject template to generate the report.
    /// </summary>
    public string Subject { get; set; } = "";

    /// <summary>
    /// get/set - The Razor body template to generate the report.
    /// </summary>
    public string Body { get; set; } = "";

    /// <summary>
    /// get/set - Whether this report template is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get/set - The settings for this report.
    /// </summary>
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an NotificationTemplateModel.
    /// </summary>
    public NotificationTemplateModel() { }

    /// <summary>
    /// Creates a new instance of an NotificationTemplateModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public NotificationTemplateModel(Entities.NotificationTemplate entity, JsonSerializerOptions options) : base(entity)
    {
        this.Subject = entity.Subject;
        this.Body = entity.Body;
        this.IsPublic = entity.IsPublic;
        this.Settings = entity.Settings;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Notification object.
    /// </summary>
    /// <returns></returns>
    public Entities.NotificationTemplate ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.NotificationTemplate)this;
        entity.Settings = JsonDocument.Parse(JsonSerializer.Serialize(this.Settings, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.NotificationTemplate(NotificationTemplateModel model)
    {
        var entity = new Entities.NotificationTemplate(model.Id, model.Name, model.Subject, model.Body)
        {
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            IsPublic = model.IsPublic,
            SortOrder = model.SortOrder,
            Settings = JsonDocument.Parse(JsonSerializer.Serialize(model.Settings)),
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
