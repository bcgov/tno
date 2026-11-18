using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>Durable orphan reconciliation progress.</summary>
[Table("orphan_scan", Schema = "public")]
public class OrphanScan
{
    [Column("worker_slot")]
    public int WorkerSlot { get; set; } = 1;
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    [Column("side")]
    public string Side { get; set; } = "elasticsearch";
    [Column("index")]
    public string Index { get; set; } = "content";
    [Column("status")]
    public string Status { get; set; } = "Scanning";
    [Column("created_on")]
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;
    [Column("next_batch_on")]
    public DateTime NextBatchOn { get; set; } = DateTime.UtcNow;
    [Column("completed_on")]
    public DateTime? CompletedOn { get; set; }
    [Column("start_posted_on")]
    public DateOnly? StartPostedOn { get; set; }
    [Column("end_posted_on")]
    public DateOnly? EndPostedOn { get; set; }
    [Column("min_id")]
    public long MinId { get; set; } = 1;
    [Column("max_id")]
    public long? MaxId { get; set; }
    [Column("after_posted_on")]
    public DateTime? AfterPostedOn { get; set; }
    [Column("after")]
    public long After { get; set; } = 0;
    [Column("scanned")]
    public long Scanned { get; set; } = 0;
    [Column("pending")]
    public long Pending { get; set; } = 0;
    [Column("found")]
    public long Found { get; set; } = 0;
    [Column("cancel_requested")]
    public bool CancelRequested { get; set; } = false;
    [Column("pit_id")]
    public string? PitId { get; set; }
    [Column("search_after")]
    public string? SearchAfter { get; set; }
    [Column("error")]
    public string? Error { get; set; }
}
