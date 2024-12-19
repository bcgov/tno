using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report totals by broadcaster
/// </summary>
[Keyless]
public class CBRAReportTotalsByBroadcaster
{
    public string sourcetype { get; set; }
    public string totalrunningtime { get; set; }
    public decimal percentageoftotalrunningtime { get; set; }
}