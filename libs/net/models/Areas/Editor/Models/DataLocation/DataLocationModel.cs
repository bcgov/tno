using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Editor.Models.DataLocation;

/// <summary>
/// DataLocationModel class, provides a model that represents an data location.
/// </summary>
public class DataLocationModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public DataLocationType LocationType { get; set; }
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
        this.LocationType = entity.LocationType;
    }
    #endregion
}
