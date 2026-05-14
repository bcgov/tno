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
    }
    #endregion
}
