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
    /// get/set - The status of a user's request.
    /// </summary>
    [Column("status")]
    public ProductRequestStatus Status { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserProduct object, initializes with specified parameters.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="product"></param>
    /// <param name="status"></param>
    public UserProduct(User user, Product product, ProductRequestStatus status = ProductRequestStatus.NA)
    {
        this.User = user ?? throw new ArgumentNullException(nameof(user));
        this.UserId = user.Id;
        this.Product = product ?? throw new ArgumentNullException(nameof(product));
        this.ProductId = product.Id;
        this.Status = status;
    }

    /// <summary>
    /// Creates a new instance of a UserProduct object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <param name="status"></param>
    public UserProduct(int userId, int productId, ProductRequestStatus status = ProductRequestStatus.NA)
    {
        this.UserId = userId;
        this.ProductId = productId;
        this.Status = status;
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
