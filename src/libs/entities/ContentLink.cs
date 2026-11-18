using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentLink class, provides an entity model to link (many-to-many) content with related content.
/// </summary>
[Table("content_link")]
public class ContentLink : AuditColumns, IEquatable<ContentLink>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to related content.
    /// </summary>
    [Column("link_id")]
    public long LinkId { get; set; }

    /// <summary>
    /// get/set - The related content.
    /// </summary>
    public virtual Content? Link { get; set; }

    /// <summary>
    /// get/set - The value to associate with the related content.
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentLink() { }

    public ContentLink(long contentId, long linkId)
    {
        this.ContentId = contentId;
        this.LinkId = linkId;
    }

    public ContentLink(Content content, Content link)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.LinkId = link?.Id ?? throw new ArgumentNullException(nameof(link));
        this.Link = link;
    }
    #endregion

    #region Methods
    public bool Equals(ContentLink? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.LinkId == other.LinkId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentLink);
    public override int GetHashCode() => (this.ContentId, this.LinkId).GetHashCode();
    #endregion
}
