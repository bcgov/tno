using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Product;

/// <summary>
/// UserProductModel class, provides a model that represents a subscriber to a product.
/// </summary>
public class UserProductModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the product.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the product.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// get/set - Whether the user is subscribed to the av evening overview product.
    /// </summary>
    public bool IsSubscribed { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserProductModel.
    /// </summary>
    public UserProductModel() { }

    /// <summary>
    /// Creates a new instance of an UserProductModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserProductModel(Entities.UserProduct entity) : base(entity)
    {
        this.UserId = entity.UserId;
        this.User = entity.User != null ? new UserModel(entity.User) : null;
        this.ProductId = entity.ProductId;
        this.IsSubscribed = entity.IsSubscribed;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.UserProduct(UserProductModel model)
    {
        return new Entities.UserProduct(model.UserId, model.ProductId)
        {
            IsSubscribed = model.IsSubscribed,
            Version = model.Version ?? 0
        };
    }
    #endregion
}
