namespace TNO.API.Areas.Editor.Models.TopicScoreRule;

/// <summary>
/// TopicScoreRuleModel class, provides a model that represents an topic score rule.
/// </summary>
public class TopicScoreRuleModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// get/set - Foreign key to series.
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set - The story section.
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set - Minimum page number.
    /// </summary>
    public string? PageMin { get; set; }

    /// <summary>
    /// get/set - Maximum page number.
    /// </summary>
    public string? PageMax { get; set; }

    /// <summary>
    /// get/set - Whether story has image.
    /// </summary>
    public bool? HasImage { get; set; }

    /// <summary>
    /// get/set - Minimum time.
    /// </summary>
    public TimeSpan? TimeMin { get; set; }

    /// <summary>
    /// get/set - Maximum time.
    /// </summary>
    public TimeSpan? TimeMax { get; set; }

    /// <summary>
    /// get/set - Minimum characters.
    /// </summary>
    public int? CharacterMin { get; set; }

    /// <summary>
    /// get/set - Maximum characters.
    /// </summary>
    public int? CharacterMax { get; set; }

    /// <summary>
    /// get/set - The score.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// get/set - Sort order.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TopicScoreRuleModel.
    /// </summary>
    public TopicScoreRuleModel() { }

    /// <summary>
    /// Creates a new instance of an TopicScoreRuleModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TopicScoreRuleModel(Entities.TopicScoreRule entity)
    {
        this.Id = entity.Id;
        this.SourceId = entity.SourceId;
        this.SeriesId = entity.SeriesId;
        this.Section = entity.Section;
        this.PageMin = entity.PageMin;
        this.PageMax = entity.PageMax;
        this.HasImage = entity.HasImage;
        this.CharacterMin = entity.CharacterMin;
        this.CharacterMax = entity.CharacterMax;
        this.TimeMin = entity.TimeMin;
        this.TimeMax = entity.TimeMax;
        this.Score = entity.Score;
        this.SortOrder = entity.SortOrder;
    }
    #endregion
}
