using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Entities;

namespace TNO.DAL.Services;

public class TimeTrackingService : BaseService<TimeTracking, object[]>, ITimeTrackingService
{
    #region Properties
    #endregion

    #region Constructors
    public TimeTrackingService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<TimeTrackingService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<TimeTracking> Find(DateTime from, DateTime to)
    {
        return this.Context.TimeTrackings
            .AsNoTracking()
            .Include(tt => tt.User)
            .Where(tt => tt.CreatedOn >= from && tt.CreatedOn <= to);
    }
    
    public IEnumerable<CBRAReportTotalExcerpts> GetTotalExcerpts(DateTime from, DateTime to)
    {
        return this.Context.fn_cbra_report_total_excerpts(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)).ToArray();
    }
    
    public IEnumerable<CBRAReportStaffSummary> GetStaffSummary(DateTime from, DateTime to)
    {
        return this.Context.fn_cbra_report_staff_summary(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)).ToArray();
    }    
    
    public IEnumerable<CBRAReportTotalsByProgram> GetTotalsByProgram(DateTime from, DateTime to)
    {
        return this.Context.fn_cbra_report_totals_by_program(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)).ToArray();
    } 
    
    public IEnumerable<CBRAReportTotalsByBroadcaster> GetTotalsByBroadcaster(DateTime from, DateTime to)
    {
        return this.Context.fn_cbra_report_totals_by_broadcaster(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)).ToArray();
    }
    
    public IEnumerable<CBRAReportTotalEntries> GetTotalEntries(DateTime from, DateTime to)
    {
        return this.Context.fn_cbra_report_total_entries(DateOnly.FromDateTime(from), DateOnly.FromDateTime(to)).ToArray();
    }
    #endregion
}
