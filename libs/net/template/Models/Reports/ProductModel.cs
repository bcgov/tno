using TNO.API.Models;

namespace TNO.TemplateEngine.Models.Reports;

/// <summary>
/// ProductModel class, provides a model that represents an content type.
/// </summary>
public class ProductModel : BaseTypeModel<int>
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

    /// <summary>
    /// Creates a new instance of an ProductModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ProductModel(TNO.API.Areas.Services.Models.Content.ProductModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }

    /// <summary>
    /// Creates a new instance of an ProductModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ProductModel(TNO.API.Areas.Services.Models.ReportInstance.ProductModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
    }
    #endregion
}
