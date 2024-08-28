using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.User;

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
    /// get/set - Is this product, visible to all subscribers.
    /// </summary>
    public bool IsPublic { get; set; }
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
    /// <param name="report"></param>
    public ProductModel(Entities.Product entity) : base(entity)
    {
        this.TargetProductId = entity.TargetProductId;
        this.ProductType = entity.ProductType;
        this.IsPublic = entity.IsPublic;
    }
    #endregion
}
