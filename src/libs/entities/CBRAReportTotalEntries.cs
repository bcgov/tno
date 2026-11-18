using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report total entries
/// </summary>
[Keyless]
public class CBRAReportTotalEntries
{
    [Column("day_of_week")]
    public string DayOfWeek { get; set; } = "";

    [Column("total_count")]
    public decimal TotalCount { get; set; }

    [Column("total_cbra")]
    public decimal TotalCbra { get; set; }
}
