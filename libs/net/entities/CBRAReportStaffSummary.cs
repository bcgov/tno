using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report staff summary
/// </summary>
[Keyless]
public class CBRAReportStaffSummary
{
    [Column("staff")]
    public string Staff { get; set; } = "";

    [Column("cbra_hours")]
    public decimal CbraHours { get; set; }
}
