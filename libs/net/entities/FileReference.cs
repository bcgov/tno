using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("file_reference")]
public class FileReference : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("mime_type")]
    public string MimeType { get; set; } = "";

    [Column("path")]
    public string Path { get; set; } = "";

    [Column("size")]
    public int Size { get; set; }

    [Column("running_time")]
    public int RunningTime { get; set; }
    #endregion

    #region Constructors
    protected FileReference() { }

    public FileReference(Content content, string mimeType, string path)
    {
        if (String.IsNullOrWhiteSpace(mimeType)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(mimeType));
        if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(path));

        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.MimeType = mimeType;
        this.Path = path;
    }

    public FileReference(long contentId, string mimeType, string path)
    {
        if (String.IsNullOrWhiteSpace(mimeType)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(mimeType));
        if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException("Parameter is required, cannot be null, empty, or whitespace.", nameof(path));

        this.ContentId = contentId;
        this.MimeType = mimeType;
        this.Path = path;
    }
    #endregion
}
