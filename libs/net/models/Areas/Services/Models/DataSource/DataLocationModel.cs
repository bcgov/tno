using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// DataLocationModel class, provides a model that represents an data location.
/// </summary>
public class DataLocationModel : BaseTypeModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an DataLocationModel.
    /// </summary>
    public DataLocationModel() { }

    /// <summary>
    /// Creates a new instance of an DataLocationModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public DataLocationModel(Entities.DataLocation entity) : base(entity)
    {

    }
    #endregion
}
