namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIOptions record, the limits a filter AI conversation runs within. A request is answered
/// synchronously, so the budget stays under the two minute route limit.
/// </summary>
/// <param name="MaxRounds">The most requests sent to the model for one answer (tool calls included).</param>
/// <param name="MaxRepairs">The most times an unusable answer is sent back to be fixed.</param>
/// <param name="Budget">The most time one answer may take.</param>
/// <param name="Attempts">Attempts per request to the model.</param>
/// <param name="SupportsBodyLength">Whether the filter page applies minBodyLength and minBodyWords (the editor does; the subscriber does not).</param>
/// <param name="RequireSearch">Whether a generated filter must be checked with at least one search before it is accepted.</param>
public record FilterAIOptions(int MaxRounds, int MaxRepairs, TimeSpan Budget, int Attempts, bool SupportsBodyLength = false, bool RequireSearch = true)
{
    /// <summary>The limits used when none are given.</summary>
    public static readonly FilterAIOptions Default = new(8, 2, TimeSpan.FromSeconds(100), 2);
}
