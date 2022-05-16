using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.SourceAction;

/// <summary>
/// SourceActionModel class, provides a model that represents an source action.
/// </summary>
public class SourceActionModel : BaseTypeModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceActionModel.
    /// </summary>
    public SourceActionModel() { }

    /// <summary>
    /// Creates a new instance of an SourceActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceActionModel(Entities.SourceAction entity) : base(entity)
    {

    }
    #endregion
}
