using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using TNO.Core.Data;

namespace TNO.Entities;

/// <summary>
/// LLM class, provides an entity for report large language models.
/// </summary>
[Cache("llms", "lookups")]
[Table("llm")]
public class LLM : BaseType<int>
{
    #region Properties
    /// <summary>
    /// get/set - The deployment name.  This allows the ability to have a generic name, or to change the deployment name after reports are already linked to the LLM.
    /// </summary>
    [Column("deployment_name")]
    public required string DeploymentName { get; set; }

    /// <summary>
    /// get/set - The agent name.
    /// </summary>
    [Column("agent_name")]
    public string? AgentName { get; set; }

    /// <summary>
    /// get/set - Minimum temperature allowed.
    /// </summary>
    [Column("min_temperature")]
    public float? MinTemperature { get; set; }

    /// <summary>
    /// get/set - Maximum temperature allowed.
    /// </summary>
    [Column("max_temperature")]
    public float? MaxTemperature { get; set; }

    /// <summary>
    /// get/set - The default system prompt.
    /// </summary>
    [Column("system_prompt")]
    public string SystemPrompt { get; set; } = "";

    /// <summary>
    /// get/set - The default user prompt.
    /// </summary>
    [Column("user_prompt")]
    public string UserPrompt { get; set; } = "";

    /// <summary>
    /// get/set - This model can be used by everyone.
    /// </summary>
    [Column("is_public")]
    public bool IsPublic { get; set; }

    /// <summary>
    /// get/set - The API key.
    /// </summary>
    [Column("api_key")]
    public string? ApiKey { get; set; }

    /// <summary>
    /// get/set - The URL to the AI project endpoint.
    /// </summary>
    [Column("project_endpoint")]
    public Uri? ProjectEndpoint { get; set; }

    /// <summary>
    /// get/set - The tokens the model reads and writes in one request.
    /// </summary>
    [Column("context_window")]
    public int? ContextWindow { get; set; }

    /// <summary>
    /// get/set - The most tokens reserved for a response.
    /// </summary>
    [Column("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    /// <summary>
    /// get/set - How tokens are counted: 'Heuristic', 'o200k_base', or 'cl100k_base'.
    /// </summary>
    [Column("token_estimation")]
    public string? TokenEstimation { get; set; }

    /// <summary>
    /// get/set - Requests allowed per minute; null is unlimited.
    /// </summary>
    [Column("requests_per_minute")]
    public int? RequestsPerMinute { get; set; }

    /// <summary>
    /// get/set - Tokens allowed per minute; null is unlimited.
    /// </summary>
    [Column("tokens_per_minute")]
    public int? TokensPerMinute { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an LLM object.
    /// </summary>
    protected LLM() { }

    /// <summary>
    /// Creates a new instance of an LLM object, initializes with specified parameters.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="deploymentName"></param>
    [SetsRequiredMembers]
    public LLM(string name, string deploymentName) : base(name)
    {
        this.DeploymentName = deploymentName;
    }
    #endregion
}
