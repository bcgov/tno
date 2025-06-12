using System.Text.Json;
using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Ingest;

/// <summary>
/// SourceModel class, provides a model that represents an source.
/// </summary>
public class SourceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string ShortName { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set -
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int LicenseId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public LicenseModel? License { get; set; }

    /// <summary>
    /// get/set - The default owner of this content.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The media type override for ingested content.
    /// </summary>
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Whether to auto transcribe the content when ingested.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether to disable the ability for subscribers to request a transcript.
    /// </summary>
    public bool DisableTranscribe { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceModel.
    /// </summary>
    public SourceModel() { }

    /// <summary>
    /// Creates a new instance of an SourceModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceModel(Entities.Source entity, JsonSerializerOptions options) : base(entity)
    {
        this.Id = entity.Id;
        this.Code = entity.Code;
        this.Name = entity.Name;
        this.ShortName = entity.ShortName;
        this.Description = entity.Description;
        this.IsEnabled = entity.IsEnabled;
        this.LicenseId = entity.LicenseId;
        this.License = entity.License != null ? new LicenseModel(entity.License) : null;
        this.OwnerId = entity.OwnerId;
        this.MediaTypeId = entity.MediaTypeId;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
        this.Configuration = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Configuration, options) ?? new Dictionary<string, object>();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Source object.
    /// </summary>
    /// <returns></returns>
    public Entities.Source ToEntity()
    {
        var entity = (Entities.Source)this;
        entity.Configuration = JsonDocument.Parse(JsonSerializer.Serialize(this.Configuration));
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Source(SourceModel model)
    {
        var entity = new Entities.Source(model.Name, model.Code, model.LicenseId)
        {
            Id = model.Id,
            ShortName = model.ShortName,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            OwnerId = model.OwnerId,
            MediaTypeId = model.MediaTypeId,
            AutoTranscribe = model.AutoTranscribe,
            DisableTranscribe = model.DisableTranscribe,
            Configuration = JsonDocument.Parse(JsonSerializer.Serialize(model.Configuration)),
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
