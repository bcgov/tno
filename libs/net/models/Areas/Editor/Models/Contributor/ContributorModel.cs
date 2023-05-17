using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Contributor;

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

    /// <summary>
    /// get/set - Whether to show the topics on the content form.
    /// </summary>
    public bool UseInTopics { get; set; }
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
    #endregion
}
