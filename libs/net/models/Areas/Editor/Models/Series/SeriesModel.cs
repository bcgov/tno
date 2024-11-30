using TNO.API.Models;

namespace TNO.API.Areas.Editor.Models.Series;

/// <summary>
/// SeriesModel class, provides a model that represents an series.
/// </summary>
public class SeriesModel : BaseTypeModel<int>
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

    /// <summary>
    /// get/set - is CBRA source or not.
    /// </summary>
    public bool IsCBRASource { get; set; }

    /// <summary>
    /// get/set - Is a secondary source - generally added via use of "Other" field.
    /// Will not be displayed in the primary Series/Source dropdown or in search filters
    /// </summary>
    public bool IsOther { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SeriesModel.
    /// </summary>
    public SeriesModel() { }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SeriesModel(Entities.Series entity) : base(entity)
    {
        this.SourceId = entity.SourceId;
        this.UseInTopics = entity.UseInTopics;
        this.IsOther = entity.IsOther;
        this.IsCBRASource = entity.IsCBRASource;
    }
    #endregion
}
