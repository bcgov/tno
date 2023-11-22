using System.Text.Json;
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
    /// get/set - Foreign key to user who owns this product.
    /// </summary>
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The owner of this product.
    /// </summary>
    public UserModel? Owner { get; set; }

    /// <summary>
    /// get/set - List of users who are subscribed to this product (many-to-many).
    /// </summary>
    public IEnumerable<UserModel> Subscribers { get; set; } = Array.Empty<UserModel>();

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
    /// <param name="options"></param>
    public ProductModel(Entities.Product entity) : base(entity)
    {
        this.TargetProductId = entity.TargetProductId;
        this.ProductType = entity.ProductType;
        this.OwnerId = entity.OwnerId;
        this.Owner = entity.Owner != null ? new UserModel(entity.Owner) : null;
        this.Subscribers = entity.SubscribersManyToMany.Where(s => s.User != null).Select(s => new UserModel(s.User!, s.IsSubscribed)).ToArray();
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
        var entity = new Entities.Product(model.Id, model.Name, model.ProductType, model.TargetProductId, model.OwnerId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new Entities.UserProduct(us.Id, entity.Id)
        {
            IsSubscribed = us.IsSubscribed
        }));

        return entity;
    }
    #endregion
}
