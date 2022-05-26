using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Services.Models.ContentReference;

/// <summary>
/// ContentReferenceModel class, provides a model that represents an category.
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
    public int WorkflowStatus { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public long Offset { get; set; } = -1;

    /// <summary>
    /// get/set -
    /// </summary>
    public int Partition { get; set; } = -1;

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
    public ContentReferenceModel(Entities.ContentReference entity) : base(entity)
    {
        this.Source = entity.Source;
        this.Uid = entity.Uid;
        this.WorkflowStatus = (int)entity.WorkflowStatus;
        this.Topic = entity.Topic;
        this.Offset = entity.Offset;
        this.Partition = entity.Partition;
        this.PublishedOn = entity.PublishedOn;
        this.SourceUpdateOn = entity.SourceUpdateOn;
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
        var entity = new Entities.ContentReference(model.Source, model.Uid, model.Topic, model.Offset, model.Partition, (WorkflowStatus)Enum.ToObject(typeof(WorkflowStatus), model.WorkflowStatus))
        {
            PublishedOn = model.PublishedOn,
            SourceUpdateOn = model.SourceUpdateOn,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
