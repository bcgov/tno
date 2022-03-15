using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Tools.Import.Destination.Entities;

public abstract class AuditColumns
{
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [Column("created_by")]
    public string CreatedBy { get; set; } = "";

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("updated_by_id")]
    public Guid UpdatedById { get; set; }

    [Column("updated_by")]
    public string UpdatedBy { get; set; } = "";

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }
}