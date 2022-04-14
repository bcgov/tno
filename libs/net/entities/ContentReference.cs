using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

[Table("content_reference")]
public class ContentReference : AuditColumns
{
    #region Properties
    [Key]
    [Column("source")]
    public string Source { get; set; } = "";

    [Key]
    [Column("uid")]
    public string Uid { get; set; } = "";

    [Column("workflow_status")]
    public WorkflowStatus WorkflowStatus { get; set; } = WorkflowStatus.InProgress;

    [Column("topic")]
    public string Topic { get; set; } = "";

    [Column("offset")]
    public long Offset { get; set; }

    [Column("partition")]
    public int Partition { get; set; }

    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    [Column("source_updated_on")]
    public DateTime? SourceUpdateOn { get; set; }

    public virtual List<ContentReferenceLog> Logs { get; set; } = new List<ContentReferenceLog>();
    #endregion

    #region Constructors
    protected ContentReference() { }

    public ContentReference(string source, string uid, string topic, long offset, int partition, WorkflowStatus status)
    {
        if (String.IsNullOrWhiteSpace(source)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(source));
        if (String.IsNullOrWhiteSpace(uid)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(uid));
        if (String.IsNullOrWhiteSpace(topic)) throw new ArgumentException("Parameter is required and cannot be null, empty, or whitespace", nameof(topic));

        this.Source = source;
        this.Uid = uid;
        this.Topic = topic;
        this.Offset = offset;
        this.Partition = partition;
        this.WorkflowStatus = status;
    }
    #endregion
}
