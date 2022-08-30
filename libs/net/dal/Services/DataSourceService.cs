using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// DataSourceService class, provides a service layer to work with data sources within the database.
/// </summary>
public class DataSourceService : BaseService<DataSource, int>, IDataSourceService
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceService, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public DataSourceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<DataSourceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find and return all the data sources.
    /// Removes connection information with possible secrets.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<DataSource> FindAll()
    {
        return this.Context.DataSources
            .AsNoTracking()
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .Where(ds => ds.ContentTypeId != 0)
            .OrderBy(ds => ds.Code)
            .ThenBy(ds => ds.Name)
            .ToArray()
            .Select(ds =>
            {
                ds.Connection = "{}";
                return ds;
            });
    }

    /// <summary>
    /// Find and return all data sources that match the specified 'filter'.
    /// Removes connection information with possible secrets.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<DataSource> Find(DataSourceFilter filter)
    {
        var query = this.Context.DataSources
            .AsNoTracking()
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

        var items = query?.ToArray()
            .Select(ds =>
            {
                ds.Connection = "{}";
                return ds;
            }) ?? Array.Empty<DataSource>();
        return new Paged<DataSource>(items, filter.Page, filter.Quantity, total);
    }


    /// <summary>
    /// Find and return all data sources that match the specified 'mediaTypeName'.
    /// Removes connection information with possible secrets.
    /// </summary>
    /// <param name="mediaTypeName"></param>
    /// <returns></returns>
    public IEnumerable<DataSource> FindByMediaType(string mediaTypeName)
    {
        return this.Context.DataSources
            .AsNoTracking()
            .Include(ds => ds.ContentType)
            .Include(ds => ds.DataLocation)
            .Include(ds => ds.MediaType)
            .Include(ds => ds.License)
            .Include(ds => ds.DataService)
            .Include(ds => ds.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(ds => ds.MetricsManyToMany).ThenInclude(cc => cc.SourceMetric)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Where(ds => ds.MediaType!.Name.ToLower() == mediaTypeName.ToLower())
            .ToArray().Select(ds =>
            {
                ds.Connection = "{}";
                return ds;
            });
    }

    /// <summary>
    /// Find the data source for the specified 'id'.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Find the data source for the specified 'code'.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Add the specified data source to the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override DataSource Add(DataSource entity)
    {
        entity.AddToContext(this.Context);
        ValidateScheduleNames(entity);
        base.Add(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override DataSource Update(DataSource entity)
    {
        return this.Update(entity);
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public DataSource Update(DataSource entity, bool updateChildren = false)
    {
        ValidateScheduleNames(entity);
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity, updateChildren);
        base.Update(original);
        return original;
    }

    /// <summary>
    /// Ensures schedules have unique names.
    /// </summary>
    /// <param name="entity"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static void ValidateScheduleNames(DataSource entity)
    {
        // TODO: This should become a validation attribute for a data source if possible.
        if (entity.SchedulesManyToMany
            .Select(s => s.Schedule?.Name)
            .Where(s => !String.IsNullOrWhiteSpace(s))
            .Distinct()
            .GroupBy(v => v)
            .Any(g => g.Count() > 1))
            throw new InvalidOperationException("Data source schedules must have unique names");
        if (entity.Schedules
            .Select(s => s.Name)
            .Where(s => !String.IsNullOrWhiteSpace(s))
            .Distinct()
            .GroupBy(v => v)
            .Any(g => g.Count() > 1))
            throw new InvalidOperationException("Data source schedules must have unique names");
    }
    #endregion
}
