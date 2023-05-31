using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Content;

/// <summary>
/// LicenseModel class, provides a model that represents an license.
/// </summary>
public class LicenseModel : BaseTypeModel<int>
{
    #region Properties
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

    }
    #endregion
}
