using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.License;

/// <summary>
/// LicenseModel class, provides a model that represents an license.
/// </summary>
public class LicenseModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Time to live (number of day).
    /// </summary>
    public int TTL { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an LicenseModel.
    /// </summary>
    public LicenseModel() { }

    /// <summary>
    /// Creates a new instance of an LicenseModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public LicenseModel(Entities.License entity) : base(entity)
    {
        this.TTL = entity.TTL;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a License object.
    /// </summary>
    /// <returns></returns>
    public Entities.License ToEntity()
    {
        var entity = (Entities.License)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.License(LicenseModel model)
    {
        var entity = new Entities.License(model.Name, model.TTL)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
