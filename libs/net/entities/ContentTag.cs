using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentTag class, provides an entity model that links (many-to-many) content with tags.
/// </summary>
[Table("content_tag")]
public class ContentTag : AuditColumns, IEquatable<ContentTag>
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
    /// get/set - Primary key and foreign key to the tag.
    /// </summary>
    [Column("tag_id")]
    public string TagId { get; set; } = "";

    /// <summary>
    /// get/set - The tag.
    /// </summary>
    public Tag? Tag { get; set; }
    #endregion

    #region Constructors
    protected ContentTag() { }

    public ContentTag(long contentId, string tagId)
    {
        this.ContentId = contentId;
        this.TagId = tagId;
    }

    public ContentTag(Content content, Tag tag)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.TagId = tag?.Id ?? throw new ArgumentNullException(nameof(tag));
        this.Tag = tag;
    }
    #endregion

    #region Methods
    public bool Equals(ContentTag? other)
    {
        if (other == null) return false;
        return this.ContentId == other.ContentId && this.TagId == other.TagId;
    }

    public override bool Equals(object? obj) => Equals(obj as ContentTag);
    public override int GetHashCode() => (this.ContentId, this.TagId).GetHashCode();
    #endregion
}
