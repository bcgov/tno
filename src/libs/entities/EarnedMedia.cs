using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// Report class, provides a DB model to manage earned media formula settings.
/// </summary>
[Cache("earned_media")]
[Table("earned_media")]
public class EarnedMedia : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary Key and Foreign Key to Source
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source this earned media formula belongs to.
    /// </summary>
    public Source? Source { get; set; }

    /// <summary>
    /// get/set - Whether this chart template has sections enabled.
    /// </summary>
    [Column("content_type")]
    public ContentType ContentType { get; set; } = ContentType.AudioVideo;

    /// <summary>
    /// get/set - Number that represents the length of the content that will be represented in the formula.
    ///     AudioVideo = Number of seconds for the rate amount (i.e. $61 for 30 second spot).
    ///     PrintContent = Number of characters that are used to represent the charge amount (i.e. $10.16 per column inch = 125 characters);
    /// </summary>
    [Column("length_of_content")]
    public int LengthOfContent { get; set; }

    /// <summary>
    /// get/set - Currency value based on the length of content.
    /// </summary>
    [Column("rate")]
    public float Rate { get; set; } = 1;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a EarnedMedia object.
    /// </summary>
    protected EarnedMedia() : base() { }

    /// <summary>
    /// Creates a new instance of a EarnedMedia object, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="contentType"></param>
    /// <param name="lengthOfContent"></param>
    /// <param name="rate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public EarnedMedia(Source source, ContentType contentType, int lengthOfContent, float rate)
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.ContentType = contentType;
        this.LengthOfContent = lengthOfContent;
        this.Rate = rate;
    }

    /// <summary>
    /// Creates a new instance of a EarnedMedia object, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="contentType"></param>
    /// <param name="lengthOfContent"></param>
    /// <param name="rate"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public EarnedMedia(int sourceId, ContentType contentType, int lengthOfContent, float rate)
    {
        this.SourceId = sourceId;
        this.ContentType = contentType;
        this.LengthOfContent = lengthOfContent;
        this.Rate = rate;
    }
    #endregion
}
