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
    /// Find all the products.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Product> FindAll()
    {
        return this.Context.Products
            .AsNoTracking()
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find all the public products.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Product> FindPublic()
    {
        return this.Context.Products
            .AsNoTracking()
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Where(r => r.IsPublic)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Name).ToArray();
    }

    /// <summary>
    /// Find all products that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IEnumerable<Product> Find(ProductFilter? filter = null)
    {
        var query = this.Context.Products
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .AsNoTracking();

        if (!String.IsNullOrWhiteSpace(filter?.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filter.Name}%"));

        if (filter?.IsPublic.HasValue == true)
            query = query.Where(r => r.IsPublic == filter.IsPublic.Value);

        // brings back products which the user is subscriber to but are not public
        if (filter?.SubscriberUserId.HasValue == true)
            query = query.Where(r => r.SubscribersManyToMany.Any(s => s.IsSubscribed && s.UserId == filter.SubscriberUserId.Value));

        if (filter?.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(q => q.SortOrder).OrderBy(u => u.Name);

        if (filter != null && filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

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
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(r => r.Id == id);
    }

    /// <summary>
    /// Find the products for the specified user.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IEnumerable<Product> FindProductsByUser(int userId)
    {
        return this.Context.Products
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Where(f => f.SubscribersManyToMany.Exists(s => s.UserId == userId && s.IsSubscribed))
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
            {
                original.SubscribersManyToMany.Add(s);

                // Update linked subscription products
                if (entity.ProductType == ProductType.Report)
                {
                    var subscription = this.Context.UserReports.FirstOrDefault(r => r.ReportId == entity.TargetProductId && r.UserId == s.UserId);
                    if (subscription == null)
                        this.Context.UserReports.Add(new UserReport(s.UserId, entity.TargetProductId, s.IsSubscribed)); // TODO: Add formatting.
                    else if (subscription.IsSubscribed != s.IsSubscribed)
                        subscription.IsSubscribed = s.IsSubscribed;
                }
                else if (entity.ProductType == ProductType.Notification)
                {
                    var subscription = this.Context.UserNotifications.FirstOrDefault(n => n.NotificationId == entity.TargetProductId && n.UserId == s.UserId);
                    if (subscription == null)
                        this.Context.UserNotifications.Add(new UserNotification(s.UserId, entity.TargetProductId, s.IsSubscribed));
                    else if (subscription.IsSubscribed != s.IsSubscribed)
                        subscription.IsSubscribed = s.IsSubscribed;
                }
                else if (entity.ProductType == ProductType.EveningOverview)
                {
                    var subscriptions = this.Context.UserAVOverviews.Where(av => av.UserId == s.UserId).ToArray();
                    if (!subscriptions.Any())
                    {
                        this.Context.UserAVOverviews.Add(new UserAVOverview(s.UserId, AVOverviewTemplateType.Weekday, s.IsSubscribed));
                        this.Context.UserAVOverviews.Add(new UserAVOverview(s.UserId, AVOverviewTemplateType.Weekend, s.IsSubscribed));
                    }
                    else if (subscriptions.Any(s => s.IsSubscribed != s.IsSubscribed))
                    {
                        subscriptions.ForEach(s => s.IsSubscribed = s.IsSubscribed);
                    }
                }
            }
            else
            {
                if (originalSubscriber.IsSubscribed != s.IsSubscribed)
                    originalSubscriber.IsSubscribed = s.IsSubscribed;
                if (originalSubscriber.RequestedIsSubscribedStatus != s.RequestedIsSubscribedStatus)
                    originalSubscriber.RequestedIsSubscribedStatus = s.RequestedIsSubscribedStatus;
                if (originalSubscriber.SubscriptionChangeActioned != s.SubscriptionChangeActioned)
                    originalSubscriber.SubscriptionChangeActioned = s.SubscriptionChangeActioned;

                // Update linked subscription products
                if (entity.ProductType == ProductType.Report)
                {
                    var subscription = this.Context.UserReports.FirstOrDefault(r => r.ReportId == entity.TargetProductId && r.UserId == originalSubscriber.UserId);
                    if (subscription != null && subscription.IsSubscribed != s.IsSubscribed)
                    {
                        subscription.IsSubscribed = s.IsSubscribed;
                        // subscription.Format = s.Format; TODO: Need to apply the format to the report subscription.
                    }
                }
                else if (entity.ProductType == ProductType.Notification)
                {
                    var subscription = this.Context.UserNotifications.FirstOrDefault(n => n.NotificationId == entity.TargetProductId && n.UserId == originalSubscriber.UserId);
                    if (subscription != null && subscription.IsSubscribed != s.IsSubscribed)
                        subscription.IsSubscribed = s.IsSubscribed;
                }
                else if (entity.ProductType == ProductType.EveningOverview)
                {
                    var subscriptions = this.Context.UserAVOverviews.Where(av => av.UserId == originalSubscriber.UserId).ToArray();
                    if (subscriptions.Any(s => s.IsSubscribed != s.IsSubscribed))
                        subscriptions.ForEach((s) => s.IsSubscribed = s.IsSubscribed);
                }
            }
        });

        original.Name = entity.Name;
        original.Description = entity.Description;
        original.ProductType = entity.ProductType;
        original.TargetProductId = entity.TargetProductId;
        original.IsEnabled = entity.IsEnabled;
        original.IsPublic = entity.IsPublic;
        original.SortOrder = entity.SortOrder;
        original.Version = entity.Version;
        this.Context.ResetVersion(original);

        return base.Update(original);
    }

    /// <summary>
    /// Unsubscribe the specified 'userId' from the target product.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> Unsubscribe(int userId, int productId)
    {
        var saveChanges = false;
        UserProduct? userProduct = await this.Context.UserProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && x.IsSubscribed);

        if (userProduct != null)
        {
            userProduct.IsSubscribed = false;
            saveChanges = true;
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    /// <summary>
    /// Unsubscribe the specified 'userId' from the target product.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<int> RequestUnsubscribe(int userId, int productId)
    {
        var saveChanges = false;
        UserProduct? userProduct = await this.Context.UserProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && x.IsSubscribed);

        if (userProduct != null)
        {
            userProduct.RequestedIsSubscribedStatus = false;
            userProduct.SubscriptionChangeActioned = false;
            saveChanges = true;
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    /// <summary>
    /// Subscribe the specified 'userId' to the specified product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    public async Task<int> Subscribe(int userId, int productId)
    {
        var saveChanges = false;
        var targetProduct = FindById(productId) ?? throw new NoContentException("Report does not exist");
        var subscriberRecord = targetProduct.Subscribers.FirstOrDefault(s => s.Id == userId);
        if (subscriberRecord != null)
        {
            UserProduct? userProduct = await this.Context.UserProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && !x.IsSubscribed);
            if (userProduct != null)
            {
                userProduct.IsSubscribed = true;
                saveChanges = true;
            }
        }
        else
        {
            this.Context.UserProducts.Add(new UserProduct(userId, productId, true));
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    /// <summary>
    /// Subscribe the specified 'userId' to the specified product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    public async Task<int> RequestSubscribe(int userId, int productId)
    {
        var saveChanges = false;
        var targetProduct = FindById(productId) ?? throw new NoContentException("Report does not exist");
        var subscriberRecord = targetProduct.Subscribers.FirstOrDefault(s => s.Id == userId);
        if (subscriberRecord != null)
        {
            UserProduct? userProduct = await this.Context.UserProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && !x.IsSubscribed);
            if (userProduct != null)
            {
                userProduct.RequestedIsSubscribedStatus = true;
                userProduct.SubscriptionChangeActioned = false;
                saveChanges = true;
            }
        }
        else
        {
            this.Context.UserProducts.Add(new UserProduct(userId, productId) { IsSubscribed = false, RequestedIsSubscribedStatus = true, SubscriptionChangeActioned = false });
            saveChanges = true;
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }

    public async Task<int> CancelSubscriptionStatusChangeRequest(int userId, int productId)
    {
        var saveChanges = false;
        var targetProduct = FindById(productId) ?? throw new NoContentException("Report does not exist");
        var subscriberRecord = targetProduct.Subscribers.FirstOrDefault(s => s.Id == userId);
        if (subscriberRecord == null) throw new NoContentException("User has no subscribe/unsubscribe requests for the report");

        UserProduct? userProduct = await this.Context.UserProducts.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == productId && x.RequestedIsSubscribedStatus.HasValue);
        if (userProduct != null)
        {
            userProduct.RequestedIsSubscribedStatus = null;
            userProduct.SubscriptionChangeActioned = null;
            saveChanges = true;
        }
        return saveChanges ? await Context.SaveChangesAsync() : await Task.FromResult(0);
    }
    #endregion
}
