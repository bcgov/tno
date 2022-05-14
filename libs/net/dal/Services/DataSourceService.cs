using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public class DataSourceService : BaseService<DataSource, int>, IDataSourceService
{
    #region Properties
    #endregion

    #region Constructors
    public DataSourceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<DataSourceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    public IEnumerable<DataSource> FindAll()
    {
        return this.Context.DataSources
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(c => c.License)
            .OrderBy(a => a.Code).ThenBy(a => a.Name).ToArray();
    }

    public IPaged<DataSource> Find(DataSourceFilter filter)
    {
        var query = this.Context.DataSources
            .Include(c => c.DataLocation)
            .Include(c => c.MediaType)
            .Include(c => c.License)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(c => c.Code == filter.Code);
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(c => EF.Functions.Like(c.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));

        if (filter.DataLocationId.HasValue)
            query = query.Where(c => c.DataLocationId == filter.DataLocationId);
        if (filter.MediaTypeId.HasValue)
            query = query.Where(c => c.MediaTypeId == filter.MediaTypeId);
        if (filter.LicenseId.HasValue)
            query = query.Where(c => c.LicenseId == filter.LicenseId);

        if (filter.Actions.Any() == true)
            query = query.Where(c => c.Actions.Any(a => filter.Actions.Contains(a.Name)));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderBy(c => c.Code).ThenBy(c => c.Name).ThenBy(c => c.IsEnabled);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<DataSource>();
        return new Paged<DataSource>(items, filter.Page, filter.Quantity, total);
    }

    public override DataSource? FindById(int id)
    {
        return this.Context.DataSources
            .Include(c => c.DataLocation)
            .Include(c => c.MediaType)
            .Include(c => c.License)
            .Include(c => c.Parent)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(c => c.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(c => c.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .FirstOrDefault(c => c.Id == id);
    }

    public DataSource? FindByCode(string code)
    {
        return this.Context.DataSources
            .Include(c => c.DataLocation)
            .Include(c => c.MediaType)
            .Include(c => c.License)
            .Include(c => c.Parent)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(c => c.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(c => c.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .FirstOrDefault(c => c.Code.ToLower() == code.ToLower());
    }

    public IEnumerable<DataSource> FindByMediaType(string mediaTypeName)
    {
        return this.Context.DataSources
            .Include(c => c.DataLocation)
            .Include(c => c.MediaType)
            .Include(c => c.License)
            .Include(c => c.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(c => c.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(c => c.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Where(c => c.MediaType!.Name.ToLower() == mediaTypeName.ToLower()).ToArray();
    }

    public override DataSource Add(DataSource entity)
    {
        entity.AddToContext(this.Context);
        base.Add(entity);
        return entity;
    }

    public override DataSource Update(DataSource entity)
    {
        return this.Update(entity);
    }

    public DataSource Update(DataSource entity, bool updateChildren = false)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity, updateChildren);
        base.Update(original);
        return original;
    }
    #endregion
}
