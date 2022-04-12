using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_link")]
public class ContentLink : AuditColumns, IEquatable<ContentLink>
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("link_id")]
    public long LinkId { get; set; }

    public virtual Content? Link { get; set; }

    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentLink() { }

    public ContentLink(Content content, Content link)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.LinkId = link?.Id ?? throw new ArgumentNullException(nameof(link));
        this.Link = link;
    }

    public ContentLink(long contentId, long linkId)
    {
        this.ContentId = contentId;
        this.LinkId = linkId;
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
