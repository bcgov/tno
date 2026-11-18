using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Content;

/// <summary>
/// ContentLabelModel class, provides a model that represents a label to identify content information.
/// </summary>
public class ContentLabelModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to label.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The key to group related labels.
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// get/set - The value of the label.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentLabelModel.
    /// </summary>
    public ContentLabelModel() { }

    /// <summary>
    /// Creates a new instance of an ContentLabelModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentLabelModel(Entities.ContentLabel entity) : base(entity)
    {
        this.Id = entity.Id;
        this.ContentId = entity.ContentId;
        this.Key = entity.Key;
        this.Value = entity.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentModel object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentLabel ToEntity(long contentId)
    {
        var entity = (Entities.ContentLabel)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentLabel(ContentLabelModel model)
    {
        return new Entities.ContentLabel(model.ContentId, model.Key, model.Value)
        {
            Id = model.Id,
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
