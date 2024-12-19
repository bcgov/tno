using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report total excerpts
/// </summary>
[Keyless]
public class CBRAReportTotalExcerpts
{
    public string category { get; set; }
    public int totals { get; set; }
}