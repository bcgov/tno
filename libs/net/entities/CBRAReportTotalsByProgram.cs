using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TNO.Entities;

/// <summary>
/// CBRA report totals by program
/// </summary>
[Keyless]
[NotMapped]
public class CBRAReportTotalsByProgram
{
    public string mediatype { get; set; }
    public string sourcetype { get; set; }
    public string series { get; set; }
    public decimal totalcount { get; set; }
    public string totalrunningtime { get; set; }
    public decimal percentageoftotalrunningtime { get; set; }
}