using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Product;

/// <summary>
/// UserProductModel class, provides a model that represents a subscriber to a report.
/// </summary>
public class UserProductModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user who is linked to the report.
    /// </summary>
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the report.
    /// </summary>
    public int ProductId { get; set; }
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
            Version = model.Version ?? 0
        };
    }
    #endregion
}
