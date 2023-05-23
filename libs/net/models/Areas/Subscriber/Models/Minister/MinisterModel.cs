using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Minister;

/// <summary>
/// MinisterModel class, provides a model that represents a Minister.
/// </summary>
public class MinisterModel : BaseTypeModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an MinisterModel.
    /// </summary>
    public MinisterModel() { }

    /// <summary>
    /// Creates a new instance of an MinisterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public MinisterModel(Entities.Minister entity) : base(entity)
    {

    }
    #endregion
}

