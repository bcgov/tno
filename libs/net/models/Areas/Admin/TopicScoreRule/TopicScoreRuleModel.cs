using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.TopicScoreRule;

/// <summary>
/// TopicScoreRuleModel class, provides a model that represents an topic.
/// </summary>
public class TopicScoreRuleModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int SourceId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? SeriesId { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? Section { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? PageMin { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? PageMax { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public bool? HasImage { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public TimeSpan? TimeMin { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public TimeSpan? TimeMax { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? CharacterMin { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int? CharacterMax { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// get/set - Whether to delete this topic score rule.
    /// </summary>
    public bool? Remove { get; set; }
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
    public TopicScoreRuleModel(Entities.TopicScoreRule entity) : base(entity)
    {
        this.Id = entity.Id;
        this.SourceId = entity.SourceId;
        this.SeriesId = entity.SeriesId;
        this.Section = entity.Section;
        this.PageMin = entity.PageMin;
        this.PageMax = entity.PageMax;
        this.HasImage = entity.HasImage;
        this.TimeMin = entity.TimeMin;
        this.TimeMax = entity.TimeMax;
        this.CharacterMin = entity.CharacterMin;
        this.CharacterMax = entity.CharacterMax;
        this.Score = entity.Score;
        this.SortOrder = entity.SortOrder;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.TopicScoreRule(TopicScoreRuleModel model)
    {
        var entity = new Entities.TopicScoreRule(model.Id, model.SourceId, model.Section, model.PageMin, model.PageMax, model.HasImage, model.CharacterMin, model.CharacterMax, model.Score, model.SortOrder)
        {
            SeriesId = model.SeriesId,
            TimeMin = model.TimeMin,
            TimeMax = model.TimeMax,
            Version = model.Version ?? 0,
        };
        return entity;
    }
    #endregion
}
