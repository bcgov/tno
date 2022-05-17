using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Content;

/// <summary>
/// ContentLinkModel class, provides a model that represents an time tracking.
/// </summary>
public class ContentLinkModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Primary key to time tracking and foreign key to the content.
    /// </summary>
    public long LinkId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentLinkModel.
    /// </summary>
    public ContentLinkModel() { }

    /// <summary>
    /// Creates a new instance of an ContentLinkModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentLinkModel(Entities.ContentLink entity) : base(entity)
    {
        this.ContentId = entity.ContentId;
        this.LinkId = entity.LinkId;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentLink object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentLink ToEntity(long contentId)
    {
        var entity = (Entities.ContentLink)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentLink(ContentLinkModel model)
    {
        return new Entities.ContentLink(model.ContentId, model.LinkId)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
