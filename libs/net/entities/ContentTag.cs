using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_tag")]
public class ContentTag : AuditColumns, IEquatable<ContentTag>
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("tag_id")]
    public string TagId { get; set; } = "";

    public Tag? Tag { get; set; }
    #endregion

    #region Constructors
    protected ContentTag() { }

    public ContentTag(Content content, Tag tag)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.TagId = tag?.Id ?? throw new ArgumentNullException(nameof(tag));
        this.Tag = tag;
    }

    public ContentTag(long contentId, string tagId)
    {
        this.ContentId = contentId;
        this.TagId = tagId;
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
