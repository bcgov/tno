using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Product;

/// <summary>
/// ProductModel class, provides a model that represents an report.
/// </summary>
public class ProductModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to the product.
    /// </summary>
    public bool IsSubscribed { get; set; }

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
    public ProductModel(Entities.Product entity, int userId) : base(entity)
    {
        // if they currently have a subscription status of true|false *OR* have NO subscription status
        this.IsSubscribed = entity.SubscribersManyToMany.FirstOrDefault(s => s.UserId == userId)?.IsSubscribed ?? false;
    }
    #endregion
}
