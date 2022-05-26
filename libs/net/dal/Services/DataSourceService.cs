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
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .OrderBy(ds => ds.Code).ThenBy(ds => ds.Name).ToArray();
    }

    public IPaged<DataSource> Find(DataSourceFilter filter)
    {
        var query = this.Context.DataSources
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(ds => EF.Functions.Like(ds.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(ds => ds.Code == filter.Code);
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(ds => EF.Functions.Like(ds.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));

        if (filter.DataLocationId.HasValue)
            query = query.Where(ds => ds.DataLocationId == filter.DataLocationId);
        if (filter.MediaTypeId.HasValue)
            query = query.Where(ds => ds.MediaTypeId == filter.MediaTypeId);
        if (filter.LicenseId.HasValue)
            query = query.Where(ds => ds.LicenseId == filter.LicenseId);

        if (filter.Actions.Any() == true)
            query = query.Where(ds => ds.Actions.Any(a => filter.Actions.Contains(a.Name)));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderBy(ds => ds.Code).ThenBy(ds => ds.Name).ThenBy(ds => ds.IsEnabled);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray() ?? Array.Empty<DataSource>();
        return new Paged<DataSource>(items, filter.Page, filter.Quantity, total);
    }

    public override DataSource? FindById(int id)
    {
        return this.Context.DataSources
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .Include(ds => ds.DataService)
            .Include(ds => ds.Parent)
            .Include(ds => ds.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(ds => ds.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .FirstOrDefault(c => c.Id == id);
    }

    public DataSource? FindByCode(string code)
    {
        return this.Context.DataSources
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .Include(ds => ds.DataService)
            .Include(ds => ds.Parent)
            .Include(ds => ds.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(ds => ds.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .FirstOrDefault(c => c.Code.ToLower() == code.ToLower());
    }

    public IEnumerable<DataSource> FindByMediaType(string mediaTypeName)
    {
        return this.Context.DataSources
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .Include(ds => ds.DataService)
            .Include(ds => ds.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(ds => ds.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Where(ds => ds.MediaType!.Name.ToLower() == mediaTypeName.ToLower()).ToArray();
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
