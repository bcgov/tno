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
    /// get/set - Primary key and foreign key to the product.
    /// </summary>
    public int TargetProductId { get; set; }

    /// <summary>
    /// get/set - Whether this product is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;

    /// <summary>
    /// get - List of users who are subscribed to this product.
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
        this.ProductType = entity.ProductType;
        this.TargetProductId = entity.TargetProductId;
        this.IsPublic = entity.IsPublic;
        this.Subscribers = entity.SubscribersManyToMany.Select(s => new UserProductModel(s)).ToArray();
    }
    #endregion

    #region Methods
    public bool Equals(ProductModel? other)
    {
        if (other == null) return false;
        return this.Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as ProductModel);
    public override int GetHashCode() => (this.Id).GetHashCode();
    #endregion

}
