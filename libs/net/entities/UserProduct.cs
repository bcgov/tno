using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// UserProduct class, provides a model to link users with their products.
/// </summary>
[Table("user_product")]
public class UserProduct : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the product.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the product.
    /// </summary>
    [Column("product_id")]
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - the product linked to the user.
    /// </summary>
    public Product? Product { get; set; }

    /// <summary>
    /// get/set - The actual subscription status for the product.
    /// </summary>
    [Column("is_subscribed")]
    public bool IsSubscribed { get; set; }

    /// <summary>
    /// get/set - The target subscription status for the product.
    /// </summary>
    [Column("requested_is_subscribed_status")]
    public bool? RequestedIsSubscribedStatus { get; set; }

    /// <summary>
    /// get/set - Has an admin approved the status change?
    /// </summary>
    [Column("subscription_change_actioned")]
    public bool? SubscriptionChangeActioned { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserProduct object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="product"></param>
    /// <param name="isSubscribed"></param>
    public UserProduct(User user, Product product, bool isSubscribed = true)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Product = product ?? throw new ArgumentNullException(nameof(product));
        this.ProductId = product.Id;
        this.IsSubscribed = isSubscribed;
    }

    /// <summary>
    /// Creates a new instance of a UserProduct object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <param name="isSubscribed"></param>
    public UserProduct(int userId, int productId, bool isSubscribed = true)
    {
        this.UserId = userId;
        this.ProductId = productId;
        this.IsSubscribed = isSubscribed;
    }
    #endregion

    #region Methods
    public bool Equals(UserProduct? other)
    {
        if (other == null) return false;
        return this.UserId == other.UserId && this.ProductId == other.ProductId;
    }

    public override bool Equals(object? obj) => Equals(obj as UserProduct);
    public override int GetHashCode() => (this.UserId, this.ProductId).GetHashCode();
    #endregion
}
