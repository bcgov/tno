using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Minister;

/// <summary>
/// MinisterModel class, provides a model that represents a Minister.
/// </summary>
public class MinisterModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Minister's aliases.
    /// </summary>
    public string Aliases { get; set; } = "";

    /// <summary>
    /// get/set - Minister's position.
    /// </summary>
    public string Position { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to organization this minister belongs to.
    /// </summary>
    public int? OrganizationId { get; set; }

    /// <summary>
    /// get/set - The organization the minister belongs to.
    /// </summary>
    public OrganizationModel? Organization { get; set; }
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
        this.Organization = entity.Organization != null ? new OrganizationModel(entity.Organization) : null;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Minister(MinisterModel model)
    {
        var entity = new Entities.Minister(model.Id, model.Name, model.OrganizationId)
        {
            Id = model.Id,
            Position = model.Position,
            Aliases = model.Aliases,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
