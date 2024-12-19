using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report total excerpts
/// </summary>
[Keyless]
[NotMapped]
public class CBRAReportTotalExcerpts
{
    public string category { get; set; }
    public int totals { get; set; }
}