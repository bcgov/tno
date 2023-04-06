using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// ReportInstanceModel class, provides a model that represents an report instance.
/// </summary>
public class ReportInstanceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the report definition.
    /// </summary>
    public int ReportId { get; set; }

    /// <summary>
    /// get/set - The date and time the report was published on.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - CHES response containing keys to find the status of a report.
    /// </summary>
    public Dictionary<string, object> Response { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// get - Collection of content associated with this report instance.
    /// </summary>
    public IEnumerable<ReportInstanceContentModel> Content { get; } = Array.Empty<ReportInstanceContentModel>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportInstanceModel.
    /// </summary>
    public ReportInstanceModel() { }

    /// <summary>
    /// Creates a new instance of an ReportInstanceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="options"></param>
    public ReportInstanceModel(Entities.ReportInstance entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.ReportId = entity.ReportId;
        this.PublishedOn = entity.PublishedOn;
        this.Response = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Response, options) ?? new Dictionary<string, object>();

        this.Content = entity.ContentManyToMany.Select(m => new ReportInstanceContentModel(m));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ReportInstance object.
    /// </summary>
    /// <returns></returns>
    public Entities.ReportInstance ToEntity(JsonSerializerOptions options)
    {
        var entity = (Entities.ReportInstance)this;
        entity.Response = JsonDocument.Parse(JsonSerializer.Serialize(this.Response, options));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ReportInstance(ReportInstanceModel model)
    {
        var entity = new Entities.ReportInstance(model.ReportId, model.Content.Select(c => c.ContentId))
        {
            Id = model.Id,
            PublishedOn = model.PublishedOn,
            Response = JsonDocument.Parse(JsonSerializer.Serialize(model.Response)),
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
