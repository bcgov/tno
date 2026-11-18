using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

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
            .Include(i => i.MediaType)
            .Include(i => i.IngestType)
            .Include(i => i.Source)
            .Include(i => i.State)
            .Include(i => i.SourceConnection)
            .Include(i => i.DestinationConnection)
            .OrderBy(i => i.Name)
            .ThenBy(i => i.IngestTypeId)
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(i =>
            {
                i.Configuration = JsonDocument.Parse("{}");
                i.SourceConnection!.Configuration = JsonDocument.Parse("{}");
                i.DestinationConnection!.Configuration = JsonDocument.Parse("{}");
                return i;
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
            .Include(i => i.MediaType)
            .Include(i => i.IngestType)
            .Include(i => i.Source)
            .Include(i => i.State)
            .Include(i => i.SourceConnection)
            .Include(i => i.DestinationConnection)
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(i => EF.Functions.Like(i.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Topic))
            query = query.Where(i => EF.Functions.Like(i.Topic.ToLower(), $"%{filter.Topic.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.ServiceType))
            query = query.Where(c => EF.Functions.JsonContains(c.Configuration, $"{{\"serviceType\":\"{filter.ServiceType}\"}}"));

        if (filter.IngestTypeId?.Any() == true)
            query = query.Where(i => filter.IngestTypeId.Contains(i.IngestTypeId));
        if (filter.SourceId.HasValue)
            query = query.Where(i => i.SourceId == filter.SourceId);
        if (filter.MediaTypeId.HasValue)
            query = query.Where(i => i.MediaTypeId == filter.MediaTypeId);
        if (filter.SourceConnectionId.HasValue)
            query = query.Where(i => i.SourceConnectionId == filter.SourceConnectionId);
        if (filter.DestinationConnectionId.HasValue)
            query = query.Where(i => i.DestinationConnectionId == filter.DestinationConnectionId);
        if (filter.IsEnabled.HasValue)
            query = query.Where(i => i.IsEnabled == filter.IsEnabled);

        var total = query.Count();

        if (filter.Sort?.Any() == true)
            query = query.OrderByProperty(filter.Sort);
        else
            query = query.OrderBy(i => i.Name).ThenBy(i => i.IngestTypeId).ThenBy(i => i.IsEnabled);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray()
            .Select(i =>
            {
                i.Configuration = JsonDocument.Parse("{}");
                i.SourceConnection!.Configuration = JsonDocument.Parse("{}");
                i.DestinationConnection!.Configuration = JsonDocument.Parse("{}");
                return i;
            }) ?? Array.Empty<Ingest>();
        return new Paged<Ingest>(items, page, quantity, total);
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
            .Include(i => i.MediaType)
            .Include(i => i.IngestType)
            .Include(i => i.Source)
            .Include(i => i.State)
            .Include(i => i.SourceConnection)
            .Include(i => i.DestinationConnection)
            .Include(i => i.SchedulesManyToMany).ThenInclude(i => i.Schedule)
            .Include(i => i.DataLocationsManyToMany).ThenInclude(i => i.DataLocation)
            .Where(i => i.IngestType!.Name.ToLower() == ingestTypeName.ToLower())
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(i =>
            {
                i.Configuration = JsonDocument.Parse("{}");
                return i;
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
            .Include(i => i.MediaType)
            .Include(i => i.IngestType)
            .Include(i => i.Source)
            .Include(i => i.State)
            .Include(i => i.SourceConnection)
            .Include(i => i.DestinationConnection)
            .Include(i => i.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Include(i => i.DataLocationsManyToMany).ThenInclude(i => i.DataLocation)
            .Where(i => i.Topic.ToLower() == topic.ToLower())
            .ToArray();

        /// TODO: Only allow admin to include connection information.
        return includeConnection ? result : result
            .Select(i =>
            {
                i.Configuration = JsonDocument.Parse("{}");
                return i;
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
            .Include(i => i.MediaType)
            .Include(i => i.IngestType)
            .Include(i => i.Source)
            .Include(i => i.State)
            .Include(i => i.SourceConnection)
            .Include(i => i.DestinationConnection)
            .Include(i => i.SchedulesManyToMany).ThenInclude(ct => ct.Schedule)
            .Include(i => i.DataLocationsManyToMany).ThenInclude(i => i.DataLocation)
            .FirstOrDefault(c => c.Id == id);
    }

    /// <summary>
    /// Add the specified data source to the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Ingest AddAndSave(Ingest entity)
    {
        this.Context.AddToContext(entity);
        ValidateScheduleNames(entity);
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Ingest UpdateAndSave(Ingest entity)
    {
        return this.UpdateAndSave(entity);
    }

    /// <summary>
    /// Update the specified data source in the database.
    /// TODO: Only allow admin
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="updateChildren"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public Ingest UpdateAndSave(Ingest entity, bool updateChildren = false)
    {
        ValidateScheduleNames(entity);
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
        this.Context.UpdateContext(original, entity, updateChildren);
        return base.UpdateAndSave(original);
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
