using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

[Table("file_reference")]
public class FileReference : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("content_id")]
    public int ContentId { get; set; }

    public Content? Content { get; set; }

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
    #endregion
}