using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_tag")]
public class ContentTag : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

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

    public ContentTag(int contentId, string tagId)
    {
        this.ContentId = contentId;
        this.TagId = tagId;
    }
    #endregion
}