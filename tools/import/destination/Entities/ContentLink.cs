using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("content_link")]
public class ContentLink : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

    [Column("link_id")]
    public int LinkId { get; set; }

    public Content? Link { get; set; }

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

    public ContentLink(int contentId, int linkId)
    {
        this.ContentId = contentId;
        this.LinkId = linkId;
    }
    #endregion
}