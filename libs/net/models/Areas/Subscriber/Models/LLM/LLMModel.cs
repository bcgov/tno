using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.LLM;

/// <summary>
/// LLMModel class, provides a model that represents an action.
/// </summary>
public class LLMModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Minimum temperature allowed.
    /// </summary>
    public float? MinTemperature { get; set; }

    /// <summary>
    /// get/set - Maximum temperature allowed.
    /// </summary>
    public float? MaxTemperature { get; set; }

    /// <summary>
    /// get/set - The default system prompt.
    /// </summary>
    public string SystemPrompt { get; set; } = "";

    /// <summary>
    /// get/set - The default user prompt.
    /// </summary>
    public string UserPrompt { get; set; } = "";

    /// <summary>
    /// get/set - This model can be used by everyone.
    /// </summary>
    public bool IsPublic { get; set; }
    /// <summary>
    /// get/set - The tokens the model reads and writes in one request.
    /// </summary>
    public int? ContextWindow { get; set; }

    /// <summary>
    /// get/set - The most tokens reserved for a response.
    /// </summary>
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// get/set - How tokens are counted: 'Heuristic', 'o200k_base', or 'cl100k_base'.
    /// </summary>
    public string? TokenEstimation { get; set; }

    /// <summary>
    /// get/set - Requests allowed per minute; null is unlimited.
    /// </summary>
    public int? RequestsPerMinute { get; set; }

    /// <summary>
    /// get/set - Tokens allowed per minute; null is unlimited.
    /// </summary>
    public int? TokensPerMinute { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an LLMModel.
    /// </summary>
    public LLMModel() { }

    /// <summary>
    /// Creates a new instance of an LLMModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public LLMModel(Entities.LLM entity) : base(entity)
    {
        this.MinTemperature = entity.MinTemperature;
        this.MaxTemperature = entity.MaxTemperature;
        this.SystemPrompt = entity.SystemPrompt;
        this.UserPrompt = entity.UserPrompt;
        this.IsPublic = entity.IsPublic;
        this.ContextWindow = entity.ContextWindow;
        this.MaxOutputTokens = entity.MaxOutputTokens;
        this.TokenEstimation = entity.TokenEstimation;
        this.RequestsPerMinute = entity.RequestsPerMinute;
        this.TokensPerMinute = entity.TokensPerMinute;
    }
    #endregion
}
