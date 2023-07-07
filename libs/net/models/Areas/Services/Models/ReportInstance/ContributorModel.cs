using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.ReportInstance;

/// <summary>
/// ContributorModel class, provides a model that represents an contributor.
/// </summary>
public class ContributorModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int? SourceId { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContributorModel.
    /// </summary>
    public ContributorModel() { }

    /// <summary>
    /// Creates a new instance of an ContributorModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContributorModel(Entities.Contributor entity) : base(entity)
    {
        this.SourceId = entity.SourceId;
    }

    /// <summary>
    /// Creates a new instance of an ContributorModel, initializes with specified parameter.
    /// </summary>
    /// <param name="model"></param>
    public ContributorModel(TNO.API.Areas.Services.Models.Content.ContributorModel model)
    {
        this.Id = model.Id;
        this.Name = model.Name;
        this.Description = model.Description;
        this.IsEnabled = model.IsEnabled;
        this.SortOrder = model.SortOrder;
        this.SourceId = model.SourceId;
    }
    #endregion
}
