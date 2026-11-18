using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Minister;

/// <summary>
/// MinisterModel class, provides a model that represents an minister.
/// </summary>
public class MinisterModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Comma separated list of alias values when searching for the minister.
    /// </summary>
    public string Aliases { get; set; } = "";

    /// <summary>
    /// get/set - The minister's position title.
    /// </summary>
    public string Position { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to organization this minster belongs to.
    /// </summary>
    public int? OrganizationId { get; set; }
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
        this.Aliases = entity.Aliases;
        this.Position = entity.Position;
        this.OrganizationId = entity.OrganizationId;
    }
    #endregion
}
