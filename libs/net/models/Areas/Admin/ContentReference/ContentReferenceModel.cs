using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.ContentReference;

/// <summary>
/// ContentReferenceModel class, provides a model for serializing content reference entities.
/// </summary>
public class ContentReferenceModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify the source and the content reference.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set - Primary key to identify the content reference, within a specified source.
    /// </summary>
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - The current workflow status.
    /// </summary>
    public Entities.WorkflowStatus Status { get; set; } = Entities.WorkflowStatus.InProgress;

    /// <summary>
    /// get/set - The Kafka topic the content is stored in.
    /// </summary>
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set - When the content was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - When the content source was updated.
    /// </summary>
    public DateTime? SourceUpdateOn { get; set; }

    /// <summary>
    /// get/set - The content imported for this reference, when it has been imported.
    /// </summary>
    public long? ContentId { get; set; }

    /// <summary>
    /// get/set - The headline of the content imported for this reference.
    /// </summary>
    public string? Headline { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentReferenceModel object.
    /// </summary>
    public ContentReferenceModel() { }

    /// <summary>
    /// Creates a new instance of a ContentReferenceModel object, initializes with specified parameter.
    /// </summary>
    /// <param name="reference"></param>
    public ContentReferenceModel(Entities.ContentReference reference) : base(reference)
    {
        this.Source = reference.Source;
        this.Uid = reference.Uid;
        this.Status = reference.Status;
        this.Topic = reference.Topic;
        this.PublishedOn = reference.PublishedOn;
        this.SourceUpdateOn = reference.SourceUpdateOn;
    }

    /// <summary>
    /// Creates a new instance of a ContentReferenceModel object, initializes with specified parameter.
    /// </summary>
    /// <param name="reference"></param>
    /// <param name="contentId">The content imported for this reference.</param>
    /// <param name="headline">The headline of the content imported for this reference.</param>
    public ContentReferenceModel(Entities.ContentReference reference, long? contentId, string? headline) : this(reference)
    {
        this.ContentId = contentId;
        this.Headline = headline;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentReference object.
    /// </summary>
    /// <returns></returns>
    public Entities.ContentReference ToEntity()
    {
        var entity = (Entities.ContentReference)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentReference(ContentReferenceModel model)
    {
        var entity = new Entities.ContentReference(model.Source, model.Uid, model.Topic, model.Status)
        {
            PublishedOn = model.PublishedOn,
            SourceUpdateOn = model.SourceUpdateOn,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
