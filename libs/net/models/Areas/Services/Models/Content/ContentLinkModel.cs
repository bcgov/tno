namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// ContentLinkModel class, a relationship between two content items with an associated value
/// (e.g. 'duplicate' recorded by the automation dedupe action).
/// </summary>
public class ContentLinkModel
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to the content the link belongs to.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - Foreign key to the related content.
    /// </summary>
    public long LinkId { get; set; }

    /// <summary>
    /// get/set - The value associated with the relationship.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentLinkModel object.
    /// </summary>
    public ContentLinkModel() { }

    /// <summary>
    /// Creates a new instance of a ContentLinkModel object, initialized with the specified entity.
    /// </summary>
    /// <param name="entity"></param>
    public ContentLinkModel(Entities.ContentLink entity)
    {
        this.ContentId = entity.ContentId;
        this.LinkId = entity.LinkId;
        this.Value = entity.Value;
    }
    #endregion
}
