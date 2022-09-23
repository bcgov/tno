using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.DAL.Extensions;
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

/// <summary>
/// IngestService class, provides a service layer to work with data sources within the database.
/// </summary>
public class IngestService : BaseService<Ingest, int>, IIngestService
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestService, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public IngestService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<IngestService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find and return all the data sources.
    /// Removes connection information with possible secrets by default.
    /// </summary>
    /// <param name="includeConnection">Whether or not to include connection information</param>
    /// <returns></returns>
    public IEnumerable<Ingest> FindAll(bool includeConnection = false)
    {
        var result = this.Context.Ingests
            .AsNoTracking()
            .Include(ds => ds.Product)
            .Include(ds => ds.IngestType)
            .Include(ds => ds.Source)
            .Include(ds => ds.State)
            .Include(ds => ds.SourceConnection)
            .Include(ds => ds.DestinationConnection)
            .OrderBy(ds => ds.Name)
            .ThenBy(ds => ds.IngestTypeId)
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(ds =>
            {
                ds.Configuration = "{}";
                ds.SourceConnection!.Configuration = "{}";
                ds.DestinationConnection!.Configuration = "{}";
                return ds;
            });
    }

    /// <summary>
    /// Find and return all data sources that match the specified 'filter'.
    /// Removes connection information with possible secrets.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<Ingest> Find(IngestFilter filter)
    {
        var query = this.Context.Ingests
            .AsNoTracking()
            .Include(ds => ds.Product)
            .Include(ds => ds.IngestType)
            .Include(ds => ds.Source)
            .Include(ds => ds.State)
            .Include(ds => ds.SourceConnection)
            .Include(ds => ds.DestinationConnection)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(ds => EF.Functions.Like(ds.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(ds => EF.Functions.Like(ds.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));

        if (filter.IngestTypeId.HasValue)
            query = query.Where(ds => ds.IngestTypeId == filter.IngestTypeId);
        if (filter.SourceId.HasValue)
            query = query.Where(ds => ds.SourceId == filter.SourceId);
        if (filter.ProductId.HasValue)
            query = query.Where(ds => ds.ProductId == filter.ProductId);
        if (filter.SourceConnectionId.HasValue)
            query = query.Where(ds => ds.SourceConnectionId == filter.SourceConnectionId);
        if (filter.DestinationConnectionId.HasValue)
            query = query.Where(ds => ds.DestinationConnectionId == filter.DestinationConnectionId);

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderBy(ds => ds.Name).ThenBy(ds => ds.IngestTypeId).ThenBy(ds => ds.IsEnabled);

        var skip = (filter.Page - 1) * filter.Quantity;
        query = query.Skip(skip).Take(filter.Quantity);

        var items = query?.ToArray()
            .Select(ds =>
            {
                ds.Configuration = "{}";
                ds.SourceConnection!.Configuration = "{}";
                ds.DestinationConnection!.Configuration = "{}";
                return ds;
            }) ?? Array.Empty<Ingest>();
        return new Paged<Ingest>(items, filter.Page, filter.Quantity, total);
    }

    /// <summary>
    /// Find and return all data sources that match the specified 'ingestTypeName'.
    /// Removes connection information with possible secrets by default.
    /// </summary>
    /// <param name="ingestTypeName">Name of ingest type</param>
    /// <param name="includeConnection">Whether or not to include connection information</param>
    /// <returns></returns>
    public IEnumerable<Ingest> FindByIngestType(string ingestTypeName, bool includeConnection)
    {
        var result = this.Context.Ingests
            .AsNoTracking()
            .Include(ds => ds.Product)
            .Include(ds => ds.IngestType)
            .Include(ds => ds.Source)
            .Include(ds => ds.State)
            .Include(ds => ds.SourceConnection)
            .Include(ds => ds.DestinationConnection)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Where(ds => ds.IngestType!.Name.ToLower() == ingestTypeName.ToLower())
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(ds =>
            {
                ds.Configuration = "{}";
                return ds;
            });
    }

    /// <summary>
    /// Find and return all data sources that match the specified 'topic'.
    /// Removes connection information with possible secrets by default.
    /// </summary>
    /// <param name="topic">Name of ingest type</param>
    /// <param name="includeConnection">Whether or not to include connection information</param>
    /// <returns></returns>
    public IEnumerable<Ingest> FindByTopic(string topic, bool includeConnection)
    {
        var result = this.Context.Ingests
            .AsNoTracking()
            .Include(ds => ds.Product)
            .Include(ds => ds.IngestType)
            .Include(ds => ds.Source)
            .Include(ds => ds.State)
            .Include(ds => ds.SourceConnection)
            .Include(ds => ds.DestinationConnection)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Where(ds => ds.Topic.ToLower() == topic.ToLower())
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(ds =>
            {
                ds.Configuration = "{}";
                return ds;
            });
    }

    /// <summary>
    /// Find the data source for the specified 'id'.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Ingest? FindById(int id)
    {
        return this.Context.Ingests
            .Include(ds => ds.Product)
            .Include(ds => ds.IngestType)
            .Include(ds => ds.Source)
            .Include(ds => ds.State)
            .Include(ds => ds.SourceConnection)
            .Include(ds => ds.DestinationConnection)
            .Include(ds => ds.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Add the specified data source to the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Ingest Add(Ingest entity)
    {
        entity.AddToContext();
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
    public override Ingest Update(Ingest entity)
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
    public Ingest Update(Ingest entity, bool updateChildren = false)
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
    private static void ValidateScheduleNames(Ingest entity)
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
