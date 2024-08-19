using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Product;

/// <summary>
/// ProductModel class, provides a model that represents an product.
/// </summary>
public class ProductModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Each type of product is this.
    /// </summary>
    public ProductType ProductType { get; set; }

    /// <summary>
    /// get/set - Foreign key to the product.
    /// </summary>
    public int TargetProductId { get; set; }

    /// <summary>
    /// get/set - List of users who are subscribed to this product (many-to-many).
    /// </summary>
    public IEnumerable<UserProductModel> Subscribers { get; set; } = Array.Empty<UserProductModel>();

    /// <summary>
    /// get/set - Is this product, visible to all subscribers.
    /// </summary>
    public bool IsPublic { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ProductModel.
    /// </summary>
    public ProductModel() { }

    /// <summary>
    /// Creates a new instance of an ProductModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="report"></param>
    public ProductModel(Entities.Product entity) : base(entity)
    {
        this.TargetProductId = entity.TargetProductId;
        this.ProductType = entity.ProductType;
        this.IsPublic = entity.IsPublic;
        this.Subscribers = entity.SubscribersManyToMany.Select(s => new UserProductModel(s)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="includeDependencies">Whether to include related dependencies like ChartTemplate, Filter, and Folder entities.</param>
    /// <returns></returns>
    public Entities.Product ToEntity()
    {
        var entity = (Entities.Product)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Product(ProductModel model)
    {
        var entity = new Entities.Product(model.Id, model.Name, model.ProductType, model.TargetProductId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            IsPublic = model.IsPublic,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        // Extract the user subscriptions to the actual products.
        if (entity.ProductType == ProductType.Report)
        {
            entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us =>
            {
                var userProduct = new UserProduct(us.UserId, entity.Id, us.Status);
                var subscription = new UserReport(
                    us.UserId,
                    entity.TargetProductId,
                    us.IsSubscribed,
                    us.Format ?? ReportDistributionFormat.FullText,
                    us.SendTo ?? EmailSentTo.To);
                userProduct.User = new Entities.User(subscription);

                return userProduct;
            }));
        }
        else if (entity.ProductType == ProductType.Notification)
        {
            entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us =>
            {
                var userProduct = new UserProduct(us.UserId, entity.Id, us.Status);
                var subscription = new UserNotification(
                    us.UserId,
                    entity.TargetProductId,
                    us.IsSubscribed);
                userProduct.User = new Entities.User(subscription);

                return userProduct;
            }));
        }
        else if (entity.ProductType == ProductType.EveningOverview)
        {
            entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us =>
            {
                var userProduct = new UserProduct(us.UserId, entity.Id, us.Status);
                var subscription = new UserAVOverview(
                    us.UserId,
                    (AVOverviewTemplateType)entity.TargetProductId,
                    us.IsSubscribed,
                    us.SendTo ?? EmailSentTo.To);
                userProduct.User = new Entities.User(subscription);

                return userProduct;
            }));
        }

        return entity;
    }
    #endregion
}
