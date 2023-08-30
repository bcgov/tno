using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace TNO.Entities;

/// <summary>
/// ContentReference class, provides an entity model to maintain ingested content.
/// As content is contented a record is stored in this table to reduce the chance of duplicates.
/// </summary>
[Table("content_reference")]
public class ContentReference : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to uniquely identify the source of the content.
    /// This should be a loose match to the Source.Code.
    /// </summary>
    [Key]
    [Column("source")]
    public string Source { get; set; } = "";

    /// <summary>
    /// get/set - A unique identifier provided by the content source.
    /// </summary>
    [Key]
    [Column("uid")]
    public string Uid { get; set; } = "";

    /// <summary>
    /// get/set - The status of the contention.
    /// </summary>
    [Column("status")]
    public WorkflowStatus Status { get; set; } = WorkflowStatus.InProgress;

    /// <summary>
    /// get/set - The Kafka topic it was sent to.
    /// </summary>
    [Column("topic")]
    public string Topic { get; set; } = "";

    /// <summary>
    /// get/set - The Kafka offset it was sent to.
    /// </summary>
    [Column("offset")]
    public long Offset { get; set; }

    /// <summary>
    /// get/set - The Kafka partition it was sent to.
    /// </summary>
    [Column("partition")]
    public int Partition { get; set; }

    /// <summary>
    /// get/set - Content metadata.
    /// </summary>
    [Column("metadata")]
    public JsonDocument? Metadata { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - When the content was published.
    /// </summary>
    [Column("published_on")]
    public DateTime? PublishedOn { get; set; }

    /// <summary>
    /// get/set - When the content was updated by the source.
    /// </summary>
    [Column("source_updated_on")]
    public DateTime? SourceUpdateOn { get; set; }
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
        this.Status = status;
    }
    #endregion
}
