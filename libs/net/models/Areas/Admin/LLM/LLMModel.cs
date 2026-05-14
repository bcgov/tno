using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.LLM;

/// <summary>
/// LLMModel class, provides a model that represents an action.
/// </summary>
public class LLMModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The deployment name.  This allows the ability to have a generic name, or to change the deployment name after reports are already linked to the LLM.
    /// </summary>
    public string DeploymentName { get; set; } = "";

    /// <summary>
    /// get/set - The agent name.
    /// </summary>
    public string? AgentName { get; set; }

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
    /// get/set - The API key.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// get/set - The URL to the AI project endpoint.
    /// </summary>
    public Uri? ProjectEndpoint { get; set; }
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
        this.DeploymentName = entity.DeploymentName;
        this.AgentName = entity.AgentName;
        this.MinTemperature = entity.MinTemperature;
        this.MaxTemperature = entity.MaxTemperature;
        this.SystemPrompt = entity.SystemPrompt;
        this.UserPrompt = entity.UserPrompt;
        this.IsPublic = entity.IsPublic;
        this.ApiKey = entity.ApiKey;
        this.ProjectEndpoint = entity.ProjectEndpoint;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a LLM object.
    /// </summary>
    /// <returns></returns>
    public Entities.LLM ToEntity()
    {
        var entity = (Entities.LLM)this;
        return entity;
    }

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.LLM(LLMModel model)
    {
        var entity = new Entities.LLM(model.Name, model.DeploymentName)
        {
            Id = model.Id,
            Description = model.Description,
            AgentName = model.AgentName,
            MinTemperature = model.MinTemperature,
            MaxTemperature = model.MaxTemperature,
            SystemPrompt = model.SystemPrompt,
            UserPrompt = model.UserPrompt,
            IsPublic = model.IsPublic,
            ApiKey = model.ApiKey,
            ProjectEndpoint = model.ProjectEndpoint,
            IsEnabled = model.IsEnabled,
            Version = model.Version ?? 0
        };

        return entity;
    }
    #endregion
}
