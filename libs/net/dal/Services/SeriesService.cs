using System.Security.Claims;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TNO.Core.Exceptions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

/// <summary>
/// SeriesService class, provides a way to perform CRUD methods on series in the database.
/// </summary>
public class SeriesService : BaseService<Series, int>, ISeriesService
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a SeriesService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public SeriesService(TNOContext dbContext, ClaimsPrincipal principal, IServiceProvider serviceProvider, ILogger<SeriesService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all series.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Series> FindAll()
    {
        return this.Context.Series
            .AsNoTracking()
            .Include(s => s.Source)
            .Include(s => s.MediaTypeSearchMappingsManyToMany).ThenInclude(cc => cc.MediaType)
            .OrderBy(s => s.SortOrder).ThenBy(s => s.Name).ToArray();
    }

    /// <summary>
    /// Find a page of series for the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IPaged<Series> Find(SeriesFilter filter)
    {
        var query = this.Context.Series
            .AsQueryable();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{filter.Name.ToLower()}%"));
        if (!String.IsNullOrWhiteSpace(filter.Description))
            query = query.Where(c => EF.Functions.Like(c.Description.ToLower(), $"%{filter.Description.ToLower()}%"));

        var total = query.Count();

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        var page = filter.Page ?? 1;
        var quantity = filter.Quantity ?? 500;
        var skip = (page - 1) * quantity;
        query = query.Skip(skip).Take(quantity);

        var items = query?.ToArray() ?? Array.Empty<Series>();
        return new Paged<Series>(items, page, quantity, total);
    }

    /// <summary>
    /// Find the series for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Series? FindById(int id)
    {
        return this.Context.Series
            .Include(s => s.Source)
            .Include(s => s.MediaTypeSearchMappingsManyToMany).ThenInclude(cc => cc.MediaType)
            .FirstOrDefault(s => s.Id == id);
    }

    /// <summary>
    /// Add the specified data source to the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Series AddAndSave(Series entity)
    {
        entity.MediaTypeSearchMappingsManyToMany?.ForEach(m =>
            {
                m.MediaType = this.Context.MediaTypes.FirstOrDefault(x => x.Id == m.MediaTypeId);
                if (m.MediaType != null)
                {
                    this.Context.Add(m);
                }
            });
        base.AddAndSave(entity);
        return entity;
    }

    /// <summary>
    /// Update the specified data Series in the database.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Series UpdateAndSave(Series entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");
        var originalMedias = original.MediaTypeSearchMappingsManyToMany.ToArray();
        originalMedias.Except(entity.MediaTypeSearchMappingsManyToMany).ForEach(s =>
            {
                this.Context.Remove(s);
            });
        entity.MediaTypeSearchMappingsManyToMany.ForEach(a =>
            {
                var originalMedia = originalMedias.FirstOrDefault(rs => rs.MediaTypeId == a.MediaTypeId);
                if (originalMedia == null)
                {
                    a.MediaType = this.Context.MediaTypes.FirstOrDefault(x => x.Id == a.MediaTypeId);
                    original.MediaTypeSearchMappingsManyToMany.Add(a);
                }
            });
        this.Context.Entry(original).CurrentValues.SetValues(entity);
        this.Context.ResetVersion(original);
        return base.UpdateAndSave(original);
    }

    public Series? Merge(int intoId, int fromId)
    {
        // get a list of content ids by finding all content records which refer to the fromId
        var contentIdsToUpdate = this.Context.Contents.Where((c) => c.SeriesId == fromId).Select((c) => c.Id).ToList();
        // if there are in fact records to update, update them
        if (contentIdsToUpdate.Any())
        {
            this.Context.Contents
                .Where(f => contentIdsToUpdate.Contains(f.Id))
                .ExecuteUpdate(f => f.SetProperty(x => x.SeriesId, x => intoId));
            this.Context.CommitTransaction();
        }
        // find the from Series and delete it
        var fromSeries = this.FindById(fromId);
        if (fromSeries != null)
            this.DeleteAndSave(fromSeries);

        return this.FindById(intoId);
    }
    #endregion
}
