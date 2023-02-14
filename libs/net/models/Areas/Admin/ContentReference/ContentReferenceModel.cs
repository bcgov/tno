namespace TNO.API.Areas.Admin.Models.ContentReference;

/// <summary>
/// ContentReferenceModel class, provides a model for serializing content reference entities.
/// </summary>
public class ContentReferenceModel
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
    /// get/set - The Kafka offset position.
    /// </summary>
    public long Offset { get; set; }

    /// <summary>
    /// get/set - The Kafka partition.
    /// </summary>
    public int Partition { get; set; }

    /// <summary>
    /// get/set - When the content was published.
    /// </summary>
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - When the content source was updated.
    /// </summary>
    public DateTime? SourceUpdateOn { get; set; }
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
    public ContentReferenceModel(Entities.ContentReference reference)
    {
        this.Source = reference.Source;
        this.Uid = reference.Uid;
        this.Status = reference.Status;
        this.Topic = reference.Topic;
        this.Offset = reference.Offset;
        this.Partition = reference.Partition;
        this.PublishedOn = reference.PublishedOn;
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
        var entity = new Entities.ContentReference(model.Source, model.Uid, model.Topic, model.Offset, model.Partition, model.Status)
        {
            PublishedOn = model.PublishedOn,
            SourceUpdateOn = model.SourceUpdateOn,
        };

        return entity;
    }
    #endregion
}
