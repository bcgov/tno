using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// ReportModel class, provides a model that represents an report.
/// </summary>
public class ReportModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Whether this report is public to all users.
    /// </summary>
    public bool IsPublic { get; set; } = false;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ReportModel.
    /// </summary>
    public ReportModel() { }

    /// <summary>
    /// Creates a new instance of an ReportModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ReportModel(Entities.Report entity) : base(entity)
    {
        this.IsPublic = entity.IsPublic;
    }
    #endregion
}
