using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report total excerpts
/// </summary>
[Keyless]
public class CBRAReportTotalExcerpts
{
    [Column("category")]
    public string Category { get; set; } = "";

    [Column("totals")]
    public int Totals { get; set; }
}
