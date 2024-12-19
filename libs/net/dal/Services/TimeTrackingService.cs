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
        var sqlParams = new object[] {
            new Npgsql.NpgsqlParameter("@f_from_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = from },
            new Npgsql.NpgsqlParameter("@f_to_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = to },
        };
        var results= this.Context.CBRAReportTotalExcerptsResults.FromSqlRaw(@$"select * from public.fn_cbra_report_total_excerpts(@f_from_date,@f_to_date);", sqlParams).ToList();
        return results;
    }
    
    public IEnumerable<CBRAReportStaffSummary> GetStaffSummary(DateTime from, DateTime to)
    {
        var sqlParams = new object[] {
            new Npgsql.NpgsqlParameter("@f_from_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = from },
            new Npgsql.NpgsqlParameter("@f_to_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = to },
        };
        var results= this.Context.CBRAReportStaffSummaryResults.FromSqlRaw(@$"select * from public.fn_cbra_report_staff_summary(@f_from_date,@f_to_date);", sqlParams).ToList();
        return results;
    }    
    
    public IEnumerable<CBRAReportTotalsByProgram> GetTotalsByProgram(DateTime from, DateTime to)
    {
        var sqlParams = new object[] {
            new Npgsql.NpgsqlParameter("@f_from_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = from },
            new Npgsql.NpgsqlParameter("@f_to_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = to },
        };
        var results= this.Context.CBRAReportTotalsByProgramResults.FromSqlRaw(@$"select * from public.fn_cbra_report_totals_by_program(@f_from_date,@f_to_date);", sqlParams).ToList();
        return results;
    } 
    
    public IEnumerable<CBRAReportTotalsByBroadcaster> GetTotalsByBroadcaster(DateTime from, DateTime to)
    {
        var sqlParams = new object[] {
            new Npgsql.NpgsqlParameter("@f_from_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = from },
            new Npgsql.NpgsqlParameter("@f_to_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = to },
        };
        var results= this.Context.CBRAReportTotalsByBroadcasterResults.FromSqlRaw(@$"select * from public.fn_cbra_report_totals_by_broadcaster(@f_from_date,@f_to_date);", sqlParams).ToList();
        return results;
    }
    
    public IEnumerable<CBRAReportTotalEntries> GetTotalEntries(DateTime from, DateTime to)
    {
        var sqlParams = new object[] {
            new Npgsql.NpgsqlParameter("@f_from_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = from },
            new Npgsql.NpgsqlParameter("@f_to_date", NpgsqlTypes.NpgsqlDbType.Date){ Value = to },
        };
        var results= this.Context.CBRAReportTotalEntriesResults.FromSqlRaw(@$"select * from public.fn_cbra_report_total_entries(@f_from_date,@f_to_date);", sqlParams).ToList();
        return results;
    }
    #endregion
}
