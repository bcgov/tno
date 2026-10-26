using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>Durable orphan reconciliation candidate or confirmed result; no content foreign key by design.</summary>
[Table("orphan_scan_result", Schema = "public")]
public class OrphanScanResult
{
    [Column("scan_id")]
    public Guid ScanId { get; set; }
    [Column("content_id")]
    public long ContentId { get; set; }
    [Column("detected_on")]
    public DateTime DetectedOn { get; set; }
    [Column("confirmed")]
    public bool Confirmed { get; set; } = false;
    [Column("headline")]
    public string Headline { get; set; } = "";
    [Column("resolution")]
    public string Resolution { get; set; } = "";
    [Column("status")]
    public string Status { get; set; } = "";
}
