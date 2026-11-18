namespace TNO.AI.Tokens;

/// <summary>
/// LlmLimits record, a model deployment's token and rate limits, configured on the 'llm' table.
/// </summary>
/// <param name="ContextWindow">The tokens the model reads and writes in one request.</param>
/// <param name="MaxOutputTokens">The most tokens reserved for the response.</param>
/// <param name="TokenEstimation">The token estimation strategy (see TokenEstimationStrategy).</param>
/// <param name="RequestsPerMinute">Requests allowed per minute; null is unlimited.</param>
/// <param name="TokensPerMinute">Tokens allowed per minute; null is unlimited.</param>
public record LlmLimits(int ContextWindow, int MaxOutputTokens, string? TokenEstimation, int? RequestsPerMinute, int? TokensPerMinute)
{
    /// <summary>
    /// Tokens added by the chat protocol for the request and each message (role markers,
    /// separators). A conservative allowance rather than a per-model constant.
    /// </summary>
    public const int RequestOverheadTokens = 16;

    /// <summary>
    /// Tokens added per message by the chat protocol.
    /// </summary>
    public const int MessageOverheadTokens = 8;

    /// <summary>
    /// Whether the limits are complete enough to budget requests.
    /// </summary>
    public bool IsValid => ContextWindow > 0 && MaxOutputTokens > 0 && MaxOutputTokens < ContextWindow;

    /// <summary>
    /// The tokens available for input in a request:
    /// context window − reserved output − instructions − protocol overhead − safety margin.
    /// </summary>
    /// <param name="instructionTokens">Tokens used by the instructions (system prompt, task).</param>
    /// <param name="messageCount">The number of messages in the request.</param>
    /// <param name="safetyMarginPercent">Percentage of the context window held back for estimation error.</param>
    /// <returns></returns>
    public int GetInputAllowance(int instructionTokens, int messageCount, int safetyMarginPercent)
    {
        var margin = (int)Math.Ceiling(ContextWindow * Math.Clamp(safetyMarginPercent, 0, 90) / 100.0);
        var overhead = RequestOverheadTokens + MessageOverheadTokens * Math.Max(1, messageCount);
        return ContextWindow - MaxOutputTokens - instructionTokens - overhead - margin;
    }
}
