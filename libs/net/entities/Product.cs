using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// Product class, provides a database entity model to manage a list of products/offerings.
/// </summary>
[Table("product")]
public class Product : BaseType<int> {

    /// <summary>
    /// get/set - Foreign key to user who owns this product.
    /// Also a unique index with the product name.
    /// Cannot have a product with the same name as another product for a single owner.
    /// </summary>
    [Column("owner_id")]
    public int? OwnerId { get; set; }

    /// <summary>
    /// get/set - The user who owns this product.
    /// </summary>
    public virtual User? Owner { get; set; }

    /// <summary>
    /// get/set - Each type of product is this.
    /// </summary>
    [Column("product_type")]
    public ProductType ProductType { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the product.
    /// </summary>
    [Column("target_product_id")]
    public int TargetProductId { get; set; }

    /// <summary>
    /// get - List of users who are subscribed to this report (many-to-many).
    /// </summary>
    public virtual List<UserProduct> SubscribersManyToMany { get; } = new List<UserProduct>();

    /// <summary>
    /// get - List of users who are subscribed to this product.
    /// </summary>
    public virtual List<User> Subscribers { get; } = new List<User>();

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
    protected Product() : base() { }

    /// <summary>
    /// Creates a new instance of a Product object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="productType"></param>
    /// <param name="targetProductId"></param>
    /// <param name="ownerId"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public Product(int id, string name, ProductType productType, int targetProductId, int? ownerId = null) : base(id, name)
    {
        this.ProductType = productType;
        this.TargetProductId = targetProductId;
        this.OwnerId = ownerId;
    }
    #endregion
}
