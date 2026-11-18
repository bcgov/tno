using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Ingest;

/// <summary>
/// IngestTypeModel class, provides a model that represents an ingest type.
/// </summary>
public class IngestTypeModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The content type of this ingest and the form to use.
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.AudioVideo;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IngestTypeModel.
    /// </summary>
    public IngestTypeModel() { }

    /// <summary>
    /// Creates a new instance of an IngestTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public IngestTypeModel(Entities.IngestType entity) : base(entity)
    {
        this.ContentType = entity.ContentType;
    }
    #endregion
}
