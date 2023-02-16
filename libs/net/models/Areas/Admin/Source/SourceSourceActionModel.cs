using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.Source;

/// <summary>
/// SourceSourceActionModel class, provides a model that represents an sources source action.
/// </summary>
public class SourceSourceActionModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int SourceId { get; set; }

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
    public SourceSourceActionModel(Entities.SourceSourceAction entity) : base(entity.SourceAction)
    {
        this.Id = entity.SourceActionId;
        this.SourceId = entity.SourceId;
        this.Value = entity.Value;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a SourceAction object.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <returns></returns>
    public Entities.SourceSourceAction ToEntity(int sourceId)
    {
        var entity = (Entities.SourceSourceAction)this;
        entity.SourceId = sourceId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.SourceSourceAction(SourceSourceActionModel model)
    {
        return new Entities.SourceSourceAction(model.SourceId, model.Id, model.Value ?? "")
        {
            Version = model.Version ?? 0
        };
    }
    #endregion
}
