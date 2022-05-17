using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.DataSource;

/// <summary>
/// ContentTypeModel class, provides a model that represents an license.
/// </summary>
public class ContentTypeModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContentTypeModel.
    /// </summary>
    public ContentTypeModel() { }

    /// <summary>
    /// Creates a new instance of an ContentTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContentTypeModel(Entities.ContentType entity) : base(entity)
    {

    }
    #endregion
}
