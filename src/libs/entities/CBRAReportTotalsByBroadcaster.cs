using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report totals by broadcaster
/// fn_cbra_report_totals_by_broadcaster
/// </summary>
[Keyless]
public class CBRAReportTotalsByBroadcaster
{
    [Column("source_type")]
    public string SourceType { get; set; } = "";

    [Column("total_running_time")]
    public string TotalRunningTime { get; set; } = "";

    [Column("percentage_of_total_running_time")]
    public decimal PercentageOfTotalRunningTime { get; set; }
}
