using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report staff summary
/// </summary>
[Keyless]
[NotMapped]
public class CBRAReportStaffSummary
{
    public string staff { get; set; }
    public decimal cbra_hours { get; set; }
}