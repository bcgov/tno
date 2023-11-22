using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Models.Settings;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Elastic;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class ProductService : BaseService<Product, int>, IProductService
{
    #region Variables
    private readonly ElasticOptions _elasticOptions;
    private readonly ITNOElasticClient _elasticClient;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    public ProductService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        ITNOElasticClient elasticClient,
        IOptions<ElasticOptions> elasticOptions,
        IServiceProvider serviceProvider,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<ProductService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _elasticClient = elasticClient;
        _elasticOptions = elasticOptions.Value;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Find all the reports.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Product> FindAll()
    {
        return this.Context.Products
            .AsNoTracking()
            .Include(r => r.Owner)
            // .Include(r => r.Template).ThenInclude(t => t!.ChartTemplates)
            // .Include(r => r.Sections)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find all the public reports.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Product> FindPublic()
    {
        return this.Context.Products
            .AsNoTracking()
            .Include(r => r.Owner)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            // .Where(r => r.IsPublic == true)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find all reports that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Product> Find(ProductFilter filter)
    {
        var query = this.Context.Products
            .AsNoTracking();

        if (filter.OwnerId.HasValue)
            query = query.Where(r => r.OwnerId == filter.OwnerId);

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filter.Name}%"));

        return query.ToArray();
    }

    /// <summary>
    /// Find the report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public override Product? FindById(int id)
    {
        return this.Context.Products
            .Include(r => r.Owner)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Find the reports for the specified user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<Product> FindProductsByUser(int userId)
    {
        return this.Context.Products
            .Include(f => f.Owner)
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Where(f => f.OwnerId == userId)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Add the new report to the database.
    /// Add subscribers to the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public override Product Add(Product entity)
    {
        this.Context.AddRange(entity.SubscribersManyToMany);
        return base.Add(entity);
    }

    /// <summary>
    /// Update the report in the database.
    /// Update subscribers of the report.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public override Product Update(Product entity)
    {
        var original = FindById(entity.Id) ?? throw new NoContentException("Entity does not exist");

        // Add/Update/Delete report subscribers.
        var originalSubscribers = original.SubscribersManyToMany.ToArray();
        originalSubscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(s =>
        {
            var originalSubscriber = originalSubscribers.FirstOrDefault(rs => rs.UserId == s.UserId);
            if (originalSubscriber == null)
                original.SubscribersManyToMany.Add(s);
            else
            {
                if (originalSubscriber.IsSubscribed != s.IsSubscribed)
                    originalSubscriber.IsSubscribed = s.IsSubscribed;
            }
        });

        original.Name = entity.Name;
        original.Description = entity.Description;
        original.ProductType = entity.ProductType;
        original.TargetProductId = entity.TargetProductId;
        original.IsEnabled = entity.IsEnabled;
        original.SortOrder = entity.SortOrder;
        original.OwnerId = entity.OwnerId;
        original.Version = entity.Version;
        this.Context.ResetVersion(original);

        return base.Update(original);
    }

    // /// <summary>
    // /// Delete product and related entities.
    // /// </summary>
    // /// <param name="entity"></param>
    // public override void Delete(Product entity)
    // {
    //     var schedules = this.Context.EventSchedules
    //         .Include(es => es.Schedule)
    //         .Where(es => es.ProductId == entity.Id)
    //         .Select(es => es.Schedule!)
    //         .ToArray();

    //     this.Context.RemoveRange(schedules);
    //     base.Delete(entity);
    // }

    /// <summary>
    /// Unsubscribe the specified 'userId' from the target report.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> Unsubscribe(int userId, int productId)
    {
        var saveChanges = false;
        var userProducts = this.Context.UserProducts.Where(x => x.UserId == userId && x.ProductId == productId);

        userProducts.ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }
    #endregion
}
