using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// SeriesMediaType class, provides an entity model that links (many-to-many) Series with data locations.
/// </summary>
[Table("series_media_type_search_mapping")]
public class SeriesMediaTypeSearchMapping : AuditColumns, IEquatable<SeriesMediaTypeSearchMapping>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the series.
    /// </summary>
    [Column("series_id")]
    public int SeriesId { get; set; }

    /// <summary>
    /// get/set - The Series.
    /// </summary>
    public virtual Series? Series { get; set; }

    /// <summary>
    /// get/set - Primary key and foreign key to the data location.
    /// </summary>
    [Column("media_type_id")]
    public int MediaTypeId { get; set; }

    /// <summary>
    /// get/set - The mediaType.
    /// </summary>
    public virtual MediaType? MediaType { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SeriesMediaType.
    /// </summary>
    protected SeriesMediaTypeSearchMapping() { }

    /// <summary>
    /// Creates a new instance of an SeriesMediaType, initializes with specified parameters.
    /// </summary>
    /// <param name="seriesId"></param>
    /// <param name="mediaTypeId"></param>
    public SeriesMediaTypeSearchMapping(int seriesId, int mediaTypeId)
    {
        this.SeriesId = seriesId;
        this.MediaTypeId = mediaTypeId;
    }

    /// <summary>
    /// Creates a new instance of an SeriesMediaTypeSearchMapping, initializes with specified parameters.
    /// </summary>
    /// <param name="series"></param>
    /// <param name="mediaType"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SeriesMediaTypeSearchMapping(Series series, MediaType mediaType)
    {
        this.SeriesId = series?.Id ?? throw new ArgumentNullException(nameof(series));
        this.Series = series;
        this.MediaTypeId = mediaType?.Id ?? throw new ArgumentNullException(nameof(mediaType));
        this.MediaType = mediaType;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determine equality by the primary/foreign key values.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(SeriesMediaTypeSearchMapping? other)
    {
        if (other == null) return false;
        return this.SeriesId == other.SeriesId && this.MediaTypeId == other.MediaTypeId;
    }

    public override bool Equals(object? obj) => Equals(obj as SeriesMediaTypeSearchMapping);
    public override int GetHashCode() => (this.SeriesId, this.MediaTypeId).GetHashCode();
    #endregion
}
