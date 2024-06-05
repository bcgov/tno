using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.ContentReference;

/// <summary>
/// ContentReferenceModel class, provides a model that represents an content reference.
/// </summary>
public class ContentReferenceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get/set -
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? SourceUpdateOn { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentReferenceModel.
    /// </summary>
    public ContentReferenceModel() { }

    /// <summary>
    /// Creates a new instance of an ContentReferenceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ContentReferenceModel(Entities.ContentReference entity, JsonSerializerOptions options) : base(entity)
    {
        this.Source = entity.Source;
        this.Uid = entity.Uid;
        this.Status = (int)entity.Status;
        this.Topic = entity.Topic;
        this.PublishedOn = entity.PublishedOn;
        this.SourceUpdateOn = entity.SourceUpdateOn;
        this.Metadata = entity?.Metadata != null
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata, options) ?? new Dictionary<string, object>()
            : new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentReference object.
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    public Entities.ContentReference ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.ContentReference)this;
        entity.Metadata = JsonDocument.Parse(JsonSerializer.Serialize(this.Metadata, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentReference(ContentReferenceModel model)
    {
        var entity = new Entities.ContentReference(model.Source, model.Uid, model.Topic, (WorkflowStatus)Enum.ToObject(typeof(WorkflowStatus), model.Status))
        {
            Metadata = JsonDocument.Parse(JsonSerializer.Serialize(model.Metadata)),
            PublishedOn = model.PublishedOn,
            SourceUpdateOn = model.SourceUpdateOn,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
