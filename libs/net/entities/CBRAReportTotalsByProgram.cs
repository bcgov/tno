using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report totals by program
/// </summary>
[Keyless]
public class CBRAReportTotalsByProgram
{
    [Column("media_type")]
    public string MediaType { get; set; } = "";

    [Column("source_type")]
    public string SourceType { get; set; } = "";

    [Column("series")]
    public string Series { get; set; } = "";

    [Column("total_count")]
    public decimal TotalCount { get; set; }

    [Column("total_running_time")]
    public string TotalRunningTime { get; set; } = "";

    [Column("percentage_of_total_running_time")]
    public decimal PercentageOfTotalRunningTime { get; set; }
}
