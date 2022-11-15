using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Product;

/// <summary>
/// ProductModel class, provides a model that represents an product.
/// </summary>
public class ProductModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
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
    public ProductModel(Entities.Product entity) : base(entity)
    {

    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Product object.
    /// </summary>
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
        var entity = new Entities.Product(model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
