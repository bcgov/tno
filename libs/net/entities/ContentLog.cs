using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_log")]
public class ContentLog : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("content_id")]
    public long ContentId { get; set; }

    public virtual Content? Content { get; set; }

    [Column("status")]
    public ContentStatus Status { get; set; }

    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; }

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
        this.WorkflowStatus = content.WorkflowStatus;
        this.Message = message;
    }
    #endregion
}
