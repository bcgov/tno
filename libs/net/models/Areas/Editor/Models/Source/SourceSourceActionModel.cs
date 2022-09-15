using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Source;

/// <summary>
/// SourceSourceActionModel class, provides a model that represents an sources source action.
/// </summary>
public class SourceSourceActionModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The value of the action.
    /// </summary>
    public string Value { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SourceSourceActionModel.
    /// </summary>
    public SourceSourceActionModel() { }

    /// <summary>
    /// Creates a new instance of an SourceSourceActionModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SourceSourceActionModel(Entities.SourceSourceAction entity) : base(entity.SourceAction ?? throw new ArgumentNullException(nameof(entity)))
    {
        this.Value = entity.Value;
    }
    #endregion
}
