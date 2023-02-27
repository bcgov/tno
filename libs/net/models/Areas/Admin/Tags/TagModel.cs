using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Tag;

/// <summary>
/// TagModel class, provides a model that represents an media type.
/// </summary>
public class TagModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// get/set - Unique name to identify the entity.
    /// </summary>
    public string Code { get; set; } = "";

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TagModel.
    /// </summary>
    public TagModel() {}

    /// <summary>
    /// Creates a new instance of an TagModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TagModel(Entities.Tag entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Code = entity.Code;
        this.Description = entity.Description;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Tag(TagModel model)
    {
        var entity = new Entities.Tag(model.Id, model.Name, model.Code)
        {
            Id = model.Id,
            Name = model.Name,
            Code = model.Code,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
