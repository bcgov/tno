using TNO.Entities;

namespace TNO.API.Models;

/// <summary>
/// AuditColumnsModel abstract class, provides a common model for objects with audit columns.
/// </summary>
public abstract class AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - When the model was created on.
    /// </summary>
    public DateTime? CreatedOn { get; set; }

    /// <summary>
    /// get/set - The uid of the user who created the model.
    /// </summary>
    public Guid? CreatedById { get; set; }

    /// <summary>
    /// get/set - The name of the user who created the model.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// get/set - When the model was last updated on.
    /// </summary>
    public DateTime? UpdatedOn { get; set; }

    /// <summary>
    /// get/set - The uid of the user who last updated the model.
    /// </summary>
    public Guid? UpdatedById { get; set; }

    /// <summary>
    /// get/set - The naem of the user who last updated the model.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// get/set - The concurrency version of the model.
    /// </summary>
    public long? Version { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AuditColumnsModel.
    /// </summary>
    public AuditColumnsModel() { }

    /// <summary>
    /// Creates a new instance of an AuditColumnsModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public AuditColumnsModel(AuditColumns? entity)
    {
        this.CreatedBy = entity?.CreatedBy;
        this.CreatedById = entity?.CreatedById;
        this.CreatedOn = entity?.CreatedOn;
        this.UpdatedBy = entity?.UpdatedBy;
        this.UpdatedById = entity?.UpdatedById;
        this.UpdatedOn = entity?.UpdatedOn;
        this.Version = entity?.Version;
    }
    #endregion
}
