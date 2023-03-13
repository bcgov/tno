using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.IngestType;

/// <summary>
/// IngestTypeModel class, provides a model that represents an ingest type.
/// </summary>
public class IngestTypeModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - The content type of this ingest and the form to use.
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Snippet;

    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Whether content should not be allowed to be requested for transcription.
    /// </summary>
    public bool DisableTranscribe { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestTypeModel.
    /// </summary>
    public IngestTypeModel() { }

    /// <summary>
    /// Creates a new instance of an IngestTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public IngestTypeModel(Entities.IngestType entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
        this.ContentType = entity.ContentType;
        this.AutoTranscribe = entity.AutoTranscribe;
        this.DisableTranscribe = entity.DisableTranscribe;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.IngestType(IngestTypeModel model)
    {
        var entity = new Entities.IngestType(model.Name)
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            ContentType = model.ContentType,
            AutoTranscribe = model.AutoTranscribe,
            DisableTranscribe = model.DisableTranscribe,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
