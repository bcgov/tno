using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Extensions;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public class ProductService : BaseService<Product, int>, IProductService
{
    #region Variables
    private readonly IChesService _chesService;
    private readonly ChesOptions _chesOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ProductService object, initializes with specified parameters.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="principal"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public ProductService(
        TNOContext dbContext,
        ClaimsPrincipal principal,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IServiceProvider serviceProvider,
        ILogger<ProductService> logger) : base(dbContext, principal, serviceProvider, logger)
    {
        _chesService = chesService;
        _chesOptions = chesOptions.Value;
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
    public IEnumerable<Product> Find(ProductFilter filter)
    {
        int[] distributionIds = Array.Empty<int>();
        var query = filter.IsAvailableToUserId.HasValue == true ?
            this.Context.Products.AsNoTracking() :
            this.Context.Products.Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User).AsNoTracking();

        if (!String.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(r => EF.Functions.Like(r.Name, $"%{filter.Name}%"));

        if (filter.IsEnabled.HasValue == true)
            query = query.Where(r => r.IsEnabled == filter.IsEnabled.Value);
        if (filter.IsPublic.HasValue == true)
            query = query.Where(r => r.IsPublic == filter.IsPublic.Value);
        // brings back products which the user is subscriber to but are not public
        if (filter.SubscriberUserId.HasValue == true)
            query = query.Where(r => r.SubscribersManyToMany.Any(s => s.UserId == filter.SubscriberUserId.Value));
        if (filter.IsAvailableToUserId.HasValue == true)
        {
            // Get all distribution lists this user is part of.
            distributionIds = this.Context.UserDistributions
                .Where(ud => ud.LinkedUserId == filter.IsAvailableToUserId.Value)
                .Select(ud => ud.UserId).ToArray();

            query = query.Where(r => r.IsPublic ||
                r.SubscribersManyToMany.Any(s => s.UserId == filter.IsAvailableToUserId.Value) ||
                r.SubscribersManyToMany.Any(s => distributionIds.Contains(s.UserId)));
        }

        if (filter.Sort?.Any() == true)
        {
            query = query.OrderByProperty(filter.Sort.First());
            foreach (var sort in filter.Sort.Skip(1))
            {
                query = query.ThenByProperty(sort);
            }
        }
        else
            query = query.OrderBy(q => q.SortOrder).OrderBy(u => u.Name);

        if (filter.Page.HasValue && filter.Quantity.HasValue)
        {
            var skip = (filter.Page.Value - 1) * filter.Quantity.Value;
            query = query
                .Skip(skip)
                .Take(filter.Quantity.Value);
        }

        var products = query.ToArray();

        if (filter.IsAvailableToUserId.HasValue == true)
        {
            // When request products for the specific user we only want to return references to that specific user.
            // Get all the actual subscription records.
            var user = this.Context.Users
                .AsNoTracking()
                .Include(u => u.ReportSubscriptionsManyToMany)
                .Include(u => u.NotificationSubscriptionsManyToMany)
                .Include(u => u.AVOverviewSubscriptionsManyToMany)
                .FirstOrDefault(u => u.Id == filter.IsAvailableToUserId.Value) ?? throw new NoContentException();

            // Fetch product subscribers only for the specified user.
            var subscribers = this.Context.UserProducts
                .AsNoTracking()
                .Include(s => s.Product)
                .Include(s => s.User)
                .Include(s => s.User).ThenInclude(u => u!.ReportSubscriptionsManyToMany)
                .Include(s => s.User).ThenInclude(u => u!.NotificationSubscriptionsManyToMany)
                .Include(s => s.User).ThenInclude(u => u!.AVOverviewSubscriptionsManyToMany)
                .Where(up => up.UserId == filter.IsAvailableToUserId.Value || distributionIds.Contains(up.UserId))
                .ToArray();

            // For each product add the actual subscription records.
            foreach (var product in products)
            {
                product.SubscribersManyToMany.AddRange(subscribers.Where(s => s.ProductId == product.Id));
                var subscriber = product.SubscribersManyToMany.FirstOrDefault(s => s.UserId == filter.IsAvailableToUserId.Value);
                if (!product.SubscribersManyToMany.Any())
                    product.SubscribersManyToMany.Add(new UserProduct(user, product));
            }
        }

        return products;
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
    /// Provides a simple way to allow the caller to decide which includes are required.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="include"></param>
    /// <param name="asNoTracking"></param>
    /// <returns></returns>
    public Product? FindById(int id, Func<IQueryable<Product>, IQueryable<Product>>? include, bool asNoTracking = true)
    {
        IQueryable<Product> query = this.Context.Products;

        if (asNoTracking)
            query = query.AsNoTracking();

        if (include != null)
        {
            query = include(query);
        }

        return query.FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// Find the report for the specified 'id'.
    /// Also include the user subscriptions to the linked product types.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeSubscriptions"></param>
    /// <param name="asNoTracking"></param>
    /// <returns></returns>
    public Product? FindById(int id, bool includeSubscriptions, bool asNoTracking = false)
    {
        IQueryable<Product> query = this.Context.Products;

        if (asNoTracking)
            query = query.AsNoTracking();

        var product = query
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .FirstOrDefault(r => r.Id == id);

        if (product != null && includeSubscriptions)
        {
            // Fetch the user subscriptions for this product.
            if (product.ProductType == ProductType.Report)
            {
                var userSubscriptions = this.Context.UserReports.AsNoTracking()
                    .Where(ur => ur.ReportId == product.TargetProductId).ToArray();
                product.SubscribersManyToMany.ForEach(ps =>
                {
                    var subscriptions = userSubscriptions.Where(s => s.UserId == ps.UserId);
                    ps.User!.ReportSubscriptionsManyToMany.AddRange(subscriptions);
                });
            }
            else if (product.ProductType == ProductType.Notification)
            {
                var userSubscriptions = this.Context.UserNotifications.AsNoTracking()
                    .Where(ur => ur.NotificationId == product.TargetProductId).ToArray();
                product.SubscribersManyToMany.ForEach(ps =>
                {
                    var subscriptions = userSubscriptions.Where(s => s.UserId == ps.UserId);
                    ps.User!.NotificationSubscriptionsManyToMany.AddRange(subscriptions);
                });
            }
            else if (product.ProductType == ProductType.EveningOverview)
            {
                var userSubscriptions = this.Context.UserAVOverviews.AsNoTracking().ToArray();
                product.SubscribersManyToMany.ForEach(ps =>
                {
                    var subscriptions = userSubscriptions.Where(s => s.UserId == ps.UserId);
                    ps.User!.AVOverviewSubscriptionsManyToMany.AddRange(subscriptions);
                });
            }
        }

        return product;
    }

    /// <summary>
    /// Find the products for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public IEnumerable<Product> FindProductsByUser(int userId)
    {
        return this.Context.Products
            .Include(r => r.SubscribersManyToMany).ThenInclude(s => s.User)
            .Where(f => f.SubscribersManyToMany.Exists(s => s.UserId == userId))
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

        // Add subscriptions to the product if required.
        if (entity.ProductType == ProductType.Report)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserReports.Where(ur => ur.ReportId == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.ReportSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                            this.Context.Entry(subscription).State = EntityState.Added;
                        else
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            currentSubscription.Format = subscription.Format;
                            currentSubscription.SendTo = subscription.SendTo;
                            this.Context.Entry(subscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }
        if (entity.ProductType == ProductType.Notification)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserNotifications.Where(ur => ur.NotificationId == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.NotificationSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                            this.Context.Entry(subscription).State = EntityState.Added;
                        else
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            this.Context.Entry(subscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }
        if (entity.ProductType == ProductType.EveningOverview)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserAVOverviews.Where(ur => (int)ur.TemplateType == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.AVOverviewSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                            this.Context.Entry(subscription).State = EntityState.Added;
                        else
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            currentSubscription.SendTo = subscription.SendTo;
                            this.Context.Entry(subscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }
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
        // Add/Update/Delete subscribers.
        var originalSubscribers = this.Context.UserProducts.Where(up => up.ProductId == entity.Id).ToArray();
        originalSubscribers.Except(entity.SubscribersManyToMany).ForEach(s =>
        {
            this.Context.Entry(s).State = EntityState.Deleted;
        });
        entity.SubscribersManyToMany.ForEach(userProduct =>
        {
            var originalSubscriber = originalSubscribers.FirstOrDefault(rs => rs.UserId == userProduct.UserId);
            if (originalSubscriber == null)
            {
                this.Context.Entry(userProduct).State = EntityState.Added;
            }
            else
            {
                if (originalSubscriber.Status != userProduct.Status)
                {
                    originalSubscriber.Status = userProduct.Status;
                }
            }
        });

        // Add subscriptions to the product if required.
        if (entity.ProductType == ProductType.Report)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserReports.Where(ur => ur.ReportId == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.ReportSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                        {
                            this.Context.Entry(subscription).State = EntityState.Added;
                        }
                        else if (currentSubscription.IsSubscribed != subscription.IsSubscribed ||
                            currentSubscription.Format != subscription.Format ||
                            currentSubscription.SendTo != subscription.SendTo)
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            currentSubscription.Format = subscription.Format;
                            currentSubscription.SendTo = subscription.SendTo;
                            this.Context.Entry(currentSubscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }
        else if (entity.ProductType == ProductType.Notification)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserNotifications.Where(ur => ur.NotificationId == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.NotificationSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                        {
                            this.Context.Entry(subscription).State = EntityState.Added;
                        }
                        else if (currentSubscription.IsSubscribed != subscription.IsSubscribed)
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            this.Context.Entry(currentSubscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }
        else if (entity.ProductType == ProductType.EveningOverview)
        {
            // Fetch current subscriptions.
            var currentSubscriptions = this.Context.UserAVOverviews.Where(ur => (int)ur.TemplateType == entity.TargetProductId).ToArray();
            foreach (var subscriber in entity.SubscribersManyToMany)
            {
                if (subscriber.User != null)
                {
                    foreach (var subscription in subscriber.User.AVOverviewSubscriptionsManyToMany)
                    {
                        var currentSubscription = currentSubscriptions.FirstOrDefault(cs => cs.UserId == subscriber.UserId);
                        if (currentSubscription == null)
                        {
                            this.Context.Entry(subscription).State = EntityState.Added;
                        }
                        else if (currentSubscription.IsSubscribed != subscription.IsSubscribed ||
                            currentSubscription.SendTo != subscription.SendTo)
                        {
                            currentSubscription.IsSubscribed = subscription.IsSubscribed;
                            currentSubscription.SendTo = subscription.SendTo;
                            this.Context.Entry(currentSubscription).State = EntityState.Modified;
                        }
                    }
                }
            }
        }

        return base.Update(entity);
    }

    /// <summary>
    /// Add the user product.
    /// </summary>
    /// <param name="subscription"></param>
    public void AddAndSave(UserProduct subscription)
    {
        this.Context.Add(subscription);
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ProductService - AddAndSave ProductId: {subscription.ProductId}, UserId: {subscription.UserId} throws exception.");
            throw;
        }
    }

    /// <summary>
    /// Update the user product.
    /// </summary>
    /// <param name="subscription"></param>
    public void UpdateAndSave(UserProduct subscription)
    {
        this.Context.Update(subscription);
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ProductService - UpdateAndSave ProductId: {subscription.ProductId}, UserId: {subscription.UserId} throws exception.");
            throw;
        }
    }

    /// <summary>
    /// Unsubscribe the specified 'userId' from the target product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    public UserProduct Unsubscribe(int userId, int productId)
    {
        var userProduct = this.Context.UserProducts.Include(up => up.Product).FirstOrDefault(x => x.UserId == userId && x.ProductId == productId) ?? throw new NoContentException();
        userProduct.Status = ProductRequestStatus.NA;
        if (userProduct.Product?.ProductType == ProductType.Report)
        {
            var subscription = this.Context.UserReports.FirstOrDefault(ur => ur.UserId == userId && ur.ReportId == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = false;
        }
        else if (userProduct.Product?.ProductType == ProductType.Notification)
        {
            var subscription = this.Context.UserNotifications.FirstOrDefault(ur => ur.UserId == userId && ur.NotificationId == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = false;
        }
        else if (userProduct.Product?.ProductType == ProductType.EveningOverview)
        {
            var subscription = this.Context.UserAVOverviews.FirstOrDefault(ur => ur.UserId == userId && (int)ur.TemplateType == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = false;
        }
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ProductService - Unsubscribe userId: {userId}, productId: {productId} throws exception.");
            throw;
        }
        return userProduct;
    }

    /// <summary>
    /// Subscribe the specified 'userId' to the specified product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    public UserProduct Subscribe(int userId, int productId)
    {
        var userProduct = this.Context.UserProducts.Include(up => up.Product).FirstOrDefault(x => x.UserId == userId && x.ProductId == productId) ?? throw new NoContentException();
        userProduct.Status = ProductRequestStatus.NA;
        if (userProduct.Product?.ProductType == ProductType.Report)
        {
            var subscription = this.Context.UserReports.FirstOrDefault(ur => ur.UserId == userId && ur.ReportId == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = true;
        }
        else if (userProduct.Product?.ProductType == ProductType.Notification)
        {
            var subscription = this.Context.UserNotifications.FirstOrDefault(ur => ur.UserId == userId && ur.NotificationId == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = true;
        }
        else if (userProduct.Product?.ProductType == ProductType.EveningOverview)
        {
            var subscription = this.Context.UserAVOverviews.FirstOrDefault(ur => ur.UserId == userId && (int)ur.TemplateType == userProduct.Product!.TargetProductId) ?? throw new NoContentException();
            subscription.IsSubscribed = true;
        }
        try
        {
            this.Context.SaveChanges();
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, $"ProductService - Subscribe userId: {userId}, productId: {productId} throws exception.");
            throw;
        }
        return userProduct;
    }

    /// <summary>
    /// Send email to administrator to identify a subscription request or cancellation request.
    /// </summary>
    /// <param name="subscription"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    public async Task SendSubscriptionRequestEmailAsync(UserProduct subscription)
    {
        var user = this.Context.Users.FirstOrDefault(u => u.Id == subscription.UserId) ?? throw new NoContentException();
        var product = this.Context.Products.FirstOrDefault(p => p.Id == subscription.ProductId) ?? throw new NoContentException();

        string subject = string.Empty;
        var message = new StringBuilder();
        message.AppendLine("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dtd\">");
        message.AppendLine("<HTML>");
        message.AppendLine("<BODY>");
        message.AppendLine($"<p><strong>User Name</strong>: {user.DisplayName}</p>");
        message.AppendLine($"<p><strong>User Email</strong>: {user.Email}</p>");
        message.AppendLine($"<p><strong>Product</strong>: {product.Name}</p>");

        if (subscription.Status == ProductRequestStatus.RequestSubscription)
        {
            subject = $"MMI: Product Subscription request - [{product.Name}]";
            message.AppendLine($"<p><strong>Action</strong>: SUBSCRIBE</p>");
        }
        else if (subscription.Status == ProductRequestStatus.RequestUnsubscribe)
        {
            subject = $"MMI: Product Subscription cancellation request - [{product.Name}]";
            message.AppendLine($"<p><strong>Action</strong>: UNSUBSCRIBE</p>");
        }

        message.AppendLine($"<p><a href=\"https://editor.mmi.gov.bc.ca/admin/products/{product.Id}\" target=\"_blank\">Link to request</a></p>");
        message.AppendLine("</BODY>");
        message.AppendLine("</HTML>");

        try
        {
            var productSubscriptionManagerEmail = this.Context.Settings.FirstOrDefault(s => s.Name.ToLower() == AdminConfigurableSettingNames.ProductSubscriptionManagerEmail.ToString().ToLower());
            if (productSubscriptionManagerEmail != null)
            {
                var emailAddresses = productSubscriptionManagerEmail.Value.Split(new char[] { ';', ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                var email = new TNO.Ches.Models.EmailModel(_chesOptions.From, emailAddresses, subject, message.ToString());
                var response = await _chesService.SendEmailAsync(email);
                this.Logger.LogInformation("Product subscription request email to [${email}] queued: ${transactionId}", productSubscriptionManagerEmail.Value, response.TransactionId);
            }
            else
            {
                this.Logger.LogError("Couldn't send product subscription request email: [ProductSubscriptionManagerEmail] not set.");
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Email failed to send");
        }
    }
    #endregion
}
