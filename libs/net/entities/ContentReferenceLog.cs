using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_reference_log")]
public class ContentReferenceLog : AuditColumns
{
    #region Properties
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("source")]
    public string Source { get; set; } = "";

    [Column("uid")]
    public string Uid { get; set; } = "";

    public virtual ContentReference? ContentReference { get; set; }

    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.InProgress;

    [Column("message")]
    public string Message { get; set; } = "";
    #endregion

    #region Constructors
    protected ContentReferenceLog() { }

    public ContentReferenceLog(ContentReference contentReference, string message)
    {
        if (String.IsNullOrWhiteSpace(message)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(message));

        this.Source = contentReference?.Source ?? throw new ArgumentNullException(nameof(contentReference));
        this.Uid = contentReference?.Uid ?? throw new ArgumentNullException(nameof(contentReference));
        this.WorkflowStatus = contentReference.WorkflowStatus;
        this.Message = message;
    }
    #endregion
}
