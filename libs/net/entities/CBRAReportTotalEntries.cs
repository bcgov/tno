using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report total entries
/// </summary>
[Keyless]
[NotMapped]
public class CBRAReportTotalEntries
{
    public string dayofweek { get; set; }
    public decimal totalcount { get; set; }
    public decimal totalcbra { get; set; }
}