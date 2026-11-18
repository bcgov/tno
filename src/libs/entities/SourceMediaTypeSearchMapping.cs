using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// SourceMediaType class, provides an entity model that links (many-to-many) source with data locations.
/// </summary>
[Table("source_media_type_search_mapping")]
public class SourceMediaTypeSearchMapping : AuditColumns, IEquatable<SourceMediaTypeSearchMapping>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key and foreign key to the source.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public virtual Source? Source { get; set; }

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
    /// Creates a new instance of an SourceMediaType.
    /// </summary>
    protected SourceMediaTypeSearchMapping() { }

    /// <summary>
    /// Creates a new instance of an SourceMediaType, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="mediaTypeId"></param>
    public SourceMediaTypeSearchMapping(int sourceId, int mediaTypeId)
    {
        this.SourceId = sourceId;
        this.MediaTypeId = mediaTypeId;
    }

    /// <summary>
    /// Creates a new instance of an SourceMediaTypeSearchMapping, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mediaType"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public SourceMediaTypeSearchMapping(Source source, MediaType mediaType)
    {
        this.SourceId = source?.Id ?? throw new ArgumentNullException(nameof(source));
        this.Source = source;
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
    public bool Equals(SourceMediaTypeSearchMapping? other)
    {
        if (other == null) return false;
        return this.SourceId == other.SourceId && this.MediaTypeId == other.MediaTypeId;
    }

    public override bool Equals(object? obj) => Equals(obj as SourceMediaTypeSearchMapping);
    public override int GetHashCode() => (this.SourceId, this.MediaTypeId).GetHashCode();
    #endregion
}
