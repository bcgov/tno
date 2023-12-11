using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// MediaType class, provides an entity model to manage different media types, group related content, and display content to subscribers.
/// </summary>
[Cache("media_types", "lookups")]
[Table("media_type")]
public class MediaType : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - Whether content with this series should automatically be transcribed.
    /// </summary>
    [Column("auto_transcribe")]
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Configuration settings for media type.
    /// </summary>
    public JsonDocument Settings { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get - List of ingest linked to this media type.
    /// </summary>
    public virtual List<Ingest> Ingests { get; } = new List<Ingest>();

    /// <summary>
    /// get - List of content linked to this media type.
    /// </summary>
    public virtual List<Content> Contents { get; } = new List<Content>();

    /// <summary>
    /// get - List of sources linked to this media type.
    /// </summary>
    public virtual List<Source> Sources { get; } = new List<Source>();

    /// <summary>
    /// get - List of sources linked to this media type - for search mapping.
    /// </summary>
    public virtual List<Source> SourceSearchMappings { get; } = new List<Source>();

    /// <summary>
    /// get - List of sources linked to this media type - for search mapping, the many-to-many relationship.
    /// </summary>
    public virtual List<SourceMediaTypeSearchMapping> SourceSearchMappingsManyToMany { get; } = new List<SourceMediaTypeSearchMapping>();

    /// <summary>
    /// get - List of users linked to this media type.
    /// </summary>
    public virtual List<User> Users { get; } = new List<User>();

    /// <summary>
    /// get - List of users (many-to-many) linked to this media type.
    /// </summary>
    public virtual List<UserMediaType> UsersManyToMany { get; } = new List<UserMediaType>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MediaType object.
    /// </summary>
    protected MediaType() { }

    /// <summary>
    /// Creates a new instance of a MediaType object, initializes with specified parameter.
    /// </summary>
    /// <param name="mediaTypeName"></param>
    public MediaType(string mediaTypeName) : base(mediaTypeName)
    {
    }
    #endregion
}
