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

    public string Aliases { get; set; } = "";
    public string Position { get; set; } = "";


    /// <summary>
    /// Creates a new instance of an MinisterModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public MinisterModel(Entities.Minister entity) : base(entity)
    {
        this.Aliases = entity.Aliases;
        this.Position = entity.Position;
    }
    #endregion
}

