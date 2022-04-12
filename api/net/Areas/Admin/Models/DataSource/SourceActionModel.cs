using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.DataSource;

/// <summary>
/// SourceActionModel class, provides a model that represents an source action.
/// </summary>
public class SourceActionModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The foreign key to the parent data source.
    /// </summary>
    public int DataSourceId { get; set; }

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
    public SourceActionModel(Entities.DataSourceAction entity) : base(entity.SourceAction)
    {
        this.Id = entity.SourceActionId;
        this.DataSourceId = entity.DataSourceId;
        this.Value = entity.Value;
        this.CreatedBy = entity.CreatedBy;
        this.CreatedById = entity.CreatedById;
        this.CreatedOn = entity.CreatedOn;
        this.UpdatedBy = entity.UpdatedBy;
        this.UpdatedById = entity.UpdatedById;
        this.UpdatedOn = entity.UpdatedOn;
        this.Version = entity.Version;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a DataSourceAction object.
    /// </summary>
    /// <param name="dataSourceId"></param>
    /// <returns></returns>
    public Entities.DataSourceAction ToEntity(int dataSourceId)
    {
        var entity = (Entities.DataSourceAction)this;
        entity.DataSourceId = dataSourceId;
        return entity;
    }

    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.DataSourceAction(SourceActionModel model)
    {
        return new Entities.DataSourceAction(model.DataSourceId, model.Id, model.Value ?? "")
        {
            Version = model.Version ?? 0
        };
    }
    #endregion
}
