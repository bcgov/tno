using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.DataSource;

/// <summary>
/// SourceActionModel class, provides a model that represents an source action.
/// </summary>
public class SourceActionModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The value of the action.
    /// </summary>
    public string Value { get; set; } = "";
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
    public SourceActionModel(Entities.DataSourceAction entity) : base(entity.SourceAction ?? throw new ArgumentNullException(nameof(entity)))
    {
        this.Value = entity.Value;
    }
    #endregion
}
