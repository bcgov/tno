using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// TopicScoreRule class, provides a DB model to store the logic used to calculate a topic score.
/// When content is assigned a topic a score is automatically generated based on the configuration in this table.
/// Each source can assign scores based on 'section, page, image, time, and characters'.
/// The table will support multiple rules which can contradict one another.
/// To resolve some of these contradictions the SortOrder will control which rule wins (first found).
/// </summary>
[Cache("topic_score_rules", "lookups")]
[Table("topic_score_rule")]
public class TopicScoreRule : AuditColumns, IEquatable<TopicScoreRule>
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    [Key]
    [Column("id")]
    public int Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the source this score will be applied to.
    /// </summary>
    [Column("source_id")]
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public Source? Source { get; set; }

    /// <summary>
    /// get/set - Foreign key to the series this score will be applied to.
    /// </summary>
    [Column("series_id")]
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The series.
    /// </summary>
    public Series? Series { get; set; }

    /// <summary>
    /// get/set - The section this score will be applied to.
    /// </summary>
    [Column("section")]
    public string? Section { get; set; } = "";

    /// <summary>
    /// get/set - The minimum page this score will be applied to.
    /// </summary>
    [Column("page_min")]
    public string? PageMin { get; set; }

    /// <summary>
    /// get/set - The maximum page this score will be applied to.
    /// </summary>
    [Column("page_max")]
    public string? PageMax { get; set; }

    /// <summary>
    /// get/set - Whether the content has an associated image.
    /// </summary>
    [Column("has_image")]
    public bool? HasImage { get; set; }

    /// <summary>
    /// get/set - The minimum time of day this score will be applied to.
    /// </summary>
    [Column("time_min")]
    public TimeSpan? TimeMin { get; set; }

    /// <summary>
    /// get/set - The maximum time of day this score will be applied to.
    /// </summary>
    [Column("time_max")]
    public TimeSpan? TimeMax { get; set; }

    /// <summary>
    /// get/set - The minimum number of characters in the content.
    /// </summary>
    [Column("char_min")]
    public int? CharacterMin { get; set; }

    /// <summary>
    /// get/set - The maximum number of characters in the content.
    /// </summary>
    [Column("char_max")]
    public int? CharacterMax { get; set; }

    /// <summary>
    /// get/set - The score that will be applied.
    /// </summary>
    [Column("score")]
    public int Score { get; set; }

    /// <summary>
    /// get/set - The order the rules are applied.
    /// </summary>
    [Column("sort_order")]
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TopicScoreRule object.
    /// </summary>
    protected TopicScoreRule() { }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="section"></param>
    /// <param name="page"></param>
    /// <param name="hasImage"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(Source source, string? section, string page, bool? hasImage, int score, int sortOrder)
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.Section = section;
        this.PageMin = page;
        this.PageMax = page;
        this.HasImage = hasImage;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="section"></param>
    /// <param name="page"></param>
    /// <param name="hasImage"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int sourceId, string? section, string page, bool? hasImage, int score, int sortOrder)
    {
        this.SourceId = sourceId;
        this.Section = section;
        this.PageMin = page;
        this.PageMax = page;
        this.HasImage = hasImage;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="section"></param>
    /// <param name="page"></param>
    /// <param name="hasImage"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int id, int sourceId, string? section, string page, bool? hasImage, int score, int sortOrder)
        : this(sourceId, section, page, hasImage, score, sortOrder)
    {
        this.Id = id;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="section"></param>
    /// <param name="pageMin"></param>
    /// <param name="pageMax"></param>
    /// <param name="hasImage"></param>
    /// <param name="charMin"></param>
    /// <param name="charMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(Source source, string? section, string? pageMin, string? pageMax, bool? hasImage, int? charMin, int? charMax, int score, int sortOrder)
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.Section = section;
        this.PageMin = pageMin;
        this.PageMax = pageMax;
        this.HasImage = hasImage;
        this.CharacterMin = charMin;
        this.CharacterMax = charMax;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="section"></param>
    /// <param name="pageMin"></param>
    /// <param name="pageMax"></param>
    /// <param name="hasImage"></param>
    /// <param name="charMin"></param>
    /// <param name="charMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int sourceId, string? section, string? pageMin, string? pageMax, bool? hasImage, int? charMin, int? charMax, int score, int sortOrder)
    {
        this.SourceId = sourceId;
        this.Section = section;
        this.PageMin = pageMin;
        this.PageMax = pageMax;
        this.HasImage = hasImage;
        this.CharacterMin = charMin;
        this.CharacterMax = charMax;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="section"></param>
    /// <param name="pageMin"></param>
    /// <param name="pageMax"></param>
    /// <param name="hasImage"></param>
    /// <param name="charMin"></param>
    /// <param name="charMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int id, int sourceId, string? section, string? pageMin, string? pageMax, bool? hasImage, int? charMin, int? charMax, int score, int sortOrder)
        : this(sourceId, section, pageMin, pageMax, hasImage, charMin, charMax, score, sortOrder)
    {
        this.Id = id;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="series"></param>
    /// <param name="timeMin"></param>
    /// <param name="timeMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(Source source, Series? series, TimeSpan? timeMin, TimeSpan? timeMax, int score, int sortOrder)
    {
        this.Source = source ?? throw new ArgumentNullException(nameof(source));
        this.SourceId = source.Id;
        this.Series = series;
        this.SeriesId = series?.Id;
        this.TimeMin = timeMin;
        this.TimeMax = timeMax;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="sourceId"></param>
    /// <param name="seriesId"></param>
    /// <param name="timeMin"></param>
    /// <param name="timeMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int sourceId, int? seriesId, TimeSpan? timeMin, TimeSpan? timeMax, int score, int sortOrder)
    {
        this.SourceId = sourceId;
        this.SeriesId = seriesId;
        this.TimeMin = timeMin;
        this.TimeMax = timeMax;
        this.Score = score;
        this.SortOrder = sortOrder;
    }

    /// <summary>
    /// Creates a new instance of a TopicScoreRule object, initializes with specified parameters.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="sourceId"></param>
    /// <param name="seriesId"></param>
    /// <param name="timeMin"></param>
    /// <param name="timeMax"></param>
    /// <param name="score"></param>
    /// <param name="sortOrder"></param>
    public TopicScoreRule(int id, int sourceId, int? seriesId, TimeSpan? timeMin, TimeSpan? timeMax, int score, int sortOrder)
        : this(sourceId, seriesId, timeMin, timeMax, score, sortOrder)
    {
        this.Id = id;
    }

    public bool Equals(TopicScoreRule? other)
    {
        if (ReferenceEquals(other, null))
            return false;
 
        if (ReferenceEquals(this, other))
            return true;

        return this.SourceId.Equals(other.SourceId)
            && ((this.Section == null && other.Section == null) || ((this.Section != null && other.Section != null) && (this.Section.Equals(other.Section))))
            && ((this.PageMin == null && other.PageMin == null) || ((this.PageMin != null && other.PageMin != null) && (this.PageMin.Equals(other.PageMin))))
            && ((this.PageMax == null && other.PageMax == null) || ((this.PageMax != null && other.PageMax != null) && (this.PageMax.Equals(other.PageMax))))
            && this.HasImage.Equals(other.HasImage)
            && this.CharacterMin.Equals(other.CharacterMin)
            && this.CharacterMax.Equals(other.CharacterMax)
            && this.Score.Equals(other.Score)
            && this.SortOrder.Equals(other.SortOrder);
    }
    #endregion
}
