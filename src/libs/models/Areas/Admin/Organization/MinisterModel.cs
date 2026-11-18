namespace TNO.API.Areas.Admin.Models.Organization;

/// <summary>
/// MinisterModel class, provides a model that represents an minister.
/// </summary>
public class MinisterModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to minister.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Name of minister.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Description of minister.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Minister's aliases.
    /// </summary>
    public string Aliases { get; set; } = "";

    /// <summary>
    /// get/set - Minister's position.
    /// </summary>
    public string Position { get; set; } = "";

    /// <summary>
    /// get/set - Whether the minister is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - Sort order.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - Foreign key to organization this minister belongs to.
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
    public MinisterModel(Entities.Minister entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Aliases = entity.Aliases;
        this.Position = entity.Position;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
        this.OrganizationId = entity.OrganizationId;
    }
    #endregion
}
