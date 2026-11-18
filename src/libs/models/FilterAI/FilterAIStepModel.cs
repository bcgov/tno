namespace TNO.API.Models.FilterAI;

/// <summary>
/// FilterAIStepModel class, one tool the model used while it worked on a filter.
/// </summary>
public class FilterAIStepModel
{
    #region Properties
    /// <summary>
    /// get/set - The tool name [lookup, search].
    /// </summary>
    public string Tool { get; set; } = "";

    /// <summary>
    /// get/set - The arguments the model sent, as JSON.
    /// </summary>
    public string Arguments { get; set; } = "";

    /// <summary>
    /// get/set - What the tool returned to the model, as JSON (capped).
    /// </summary>
    public string Result { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterAIStepModel.
    /// </summary>
    public FilterAIStepModel() { }

    /// <summary>
    /// Creates a new instance of a FilterAIStepModel, initializes with specified parameters.
    /// </summary>
    /// <param name="tool"></param>
    /// <param name="arguments"></param>
    /// <param name="result"></param>
    public FilterAIStepModel(string tool, string arguments, string result)
    {
        this.Tool = tool;
        this.Arguments = arguments;
        this.Result = result;
    }
    #endregion
}
