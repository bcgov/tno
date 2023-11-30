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

    /// <summary>
    /// get/set - Has an admin approved the status change?
    /// </summary>
    public bool? SubscriptionChangeActioned { get; set; }
    /// <summary>
    /// get/set - Has an admin approved the status change?
    /// </summary>
    public bool? RequestedIsSubscribedStatus { get; set; }

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
        var userRecord = entity.SubscribersManyToMany.FirstOrDefault(s => s.UserId == userId);
        if (userRecord != null) {
            this.IsSubscribed = userRecord.IsSubscribed;
            this.RequestedIsSubscribedStatus = userRecord.RequestedIsSubscribedStatus;
            this.SubscriptionChangeActioned = userRecord.SubscriptionChangeActioned;
        } else {
            this.IsSubscribed = false;
        }
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
