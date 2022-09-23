using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("print_content")]
public class PrintContent : AuditColumns
{
    #region Properties
    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("edition")]
    public string Edition { get; set; } = "";

    [Column("section")]
    public string Section { get; set; } = "";

    [Column("byline")]
    public string Byline { get; set; } = "";
    #endregion

    #region Constructors
    protected PrintContent() { }

    public PrintContent(Content content, string edition, string section, string byline)
    {
        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.Edition = edition ?? throw new ArgumentNullException(nameof(edition));
        this.Section = section ?? throw new ArgumentNullException(nameof(section));
        this.Byline = byline ?? throw new ArgumentNullException(nameof(byline));
    }
    #endregion
}
