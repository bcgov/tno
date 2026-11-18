using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Content;

/// <summary>
/// ContentTonePoolModel class, provides a model that represents an tone pool.
/// </summary>
public class ContentTonePoolModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - Foreign key to parent content.
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// get/set - The user Id who owns this tone pool.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - The value of the tone pool.
    /// </summary>
    public int Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel.
    /// </summary>
    public ContentTonePoolModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTonePoolModel(Entities.ContentTonePool entity) : base(entity)
    {
        this.ContentId = entity.ContentId;
        this.Id = entity.TonePoolId;
        this.Name = entity.TonePool?.Name ?? "";
        this.OwnerId = entity.TonePool?.OwnerId ?? 0;
        this.Value = entity.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a ContentTonePool object.
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    public Entities.ContentTonePool ToEntity(long contentId)
    {
        var entity = (Entities.ContentTonePool)this;
        entity.ContentId = contentId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.ContentTonePool(ContentTonePoolModel model)
    {
        return new Entities.ContentTonePool(model.ContentId, model.Id, model.Value)
        {
            Version = model.Version ?? 0,
        };
    }
    #endregion
}
