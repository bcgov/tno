using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Subscriber.Models.Product;

/// <summary>
/// ProductModel class, provides a model that represents an report.
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
    /// get - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public virtual IEnumerable<UserProductModel> Subscribers { get; set; } = Array.Empty<UserProductModel>();

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
        this.Subscribers = entity.SubscribersManyToMany.Select(s => new UserProductModel(s)).ToArray();
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
    /// <param name="options"></param>
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
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };


        entity.SubscribersManyToMany.AddRange(model.Subscribers.Select(us => new Entities.UserProduct(us.UserId, entity.Id)
        {
            IsSubscribed = us.IsSubscribed
        }));


        return entity;
    }
    #endregion
}
