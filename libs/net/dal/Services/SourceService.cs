using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// SourceService class, provides a service layer to work with sources within the database.
/// </summary>
public class SourceService : BaseService<Source, int>, ISourceService
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a SourceService, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public SourceService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SourceService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find and return all the data sources.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Source> FindAll()
    {
        return this.Context.Sources
            .AsNoTracking()
            .Include(s => s.License)
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Code)
            .ToArray();
    }

    /// <summary>
    /// Find and return all data sources that match the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<Source> Find(SourceFilter filter)
    {
        var query = this.Context.Sources
            .AsNoTracking()
            .Include(s => s.License)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(s => EF.Functions.Like(s.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(s => EF.Functions.Like(s.Code.ToLower(), $"%{filter.Code.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.ShortName))
            query = query.Where(s => EF.Functions.Like(s.ShortName.ToLower(), $"%{filter.ShortName.ToLower()}%"));

        if (filter.LicenseId.HasValue)
            query = query.Where(s => s.LicenseId == filter.LicenseId);
        if (filter.OwnerId.HasValue)
            query = query.Where(s => s.OwnerId == filter.OwnerId);

        if (filter.Actions.Any() == true)
            query = query.Where(s => s.Actions.Any(a => filter.Actions.Contains(a.Name)));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderBy(s => s.Code).ThenBy(s => s.Name).ThenBy(s => s.IsEnabled);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);
        var items = query?.ToArray() ?? Array.Empty<Source>();
        return new Paged<Source>(items, filter.Page, filter.Quantity, total);
    }

    /// <summary>
    /// Find the data source for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Source? FindById(int id)
    {
        return this.Context.Sources
            .Include(s => s.License)
            .Include(s => s.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(s => s.MetricsManyToMany).ThenInclude(cc => cc.Metric)
            .FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Find the data source for the specified 'code'.
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    public Source? FindByCode(string code)
    {
        return this.Context.Sources
            .Include(s => s.License)
            .Include(s => s.ActionsManyToMany).ThenInclude(ca => ca.SourceAction)
            .Include(s => s.MetricsManyToMany).ThenInclude(cc => cc.Metric)
            .FirstOrDefault(c => c.Code.ToLower() == code.ToLower());
    }

    /// <summary>
    /// Add the specified data source to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Source AddAndSave(Source entity)
    {
        entity.AddToContext(this.Context);
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Source UpdateAndSave(Source entity)
    {
        return this.UpdateAndSave(entity);
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Source UpdateAndSave(Source entity, bool updateChildren = false)
    {
        var original = FindById(entity.Id) ?? throw new InvalidOperationException("Entity does not exist");
        this.Context.UpdateContext(original, entity, updateChildren);
        base.UpdateAndSave(original);
        return original;
    }
    #endregion
}
