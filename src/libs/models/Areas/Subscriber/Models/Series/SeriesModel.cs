using TNO.API.Models;
using TNO.API.Areas.Subscriber.Models.MediaType;

namespace TNO.API.Areas.Subscriber.Models.Series;

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

    /// <summary>
    /// get/set -
    /// </summary>
    public int? MediaTypeId { get; set; }

    /// <summary>
    /// get/set - Collection of media types used in search mapping, the many-to-many relationship.
    /// </summary>
    public IEnumerable<MediaTypeModel> MediaTypeSearchMappings { get; set; } = Array.Empty<MediaTypeModel>();
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
        this.MediaTypeSearchMappings = entity.MediaTypeSearchMappingsManyToMany.Select(m => new MediaTypeModel(m.MediaType!));
        this.IsCBRASource = entity.IsCBRASource;
    }
    #endregion
}
