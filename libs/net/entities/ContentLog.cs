using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// ContentLog class, provides an entity model to capture changes to content.
/// </summary>
[Table("content_log")]
public class ContentLog : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to content.
    /// </summary>
    [Column("content_id")]
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The content.
    /// </summary>
    public virtual Content? Content { get; set; }

    /// <summary>
    /// get/set - The prior content status value.
    /// </summary>
    [Column("status")]
    public ContentStatus Status { get; set; }

    /// <summary>
    /// get/set - A message to explain the change.
    /// </summary>
    [Column("message")]
    public string Message { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentLog() { }

    public ContentLog(Content content, string message)
    {
        if (String.IsNullOrWhiteSpace(message)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(message));

        this.ContentId = content?.Id ?? throw new ArgumentNullException(nameof(content));
        this.Content = content;
        this.Status = content.Status;
        this.Message = message;
    }
    #endregion
}
