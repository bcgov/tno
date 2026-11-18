namespace TNO.AI.Tokens;

/// <summary>
/// TokenEstimationStrategy class, the names of the supported ways to count tokens for a model.
/// </summary>
public static class TokenEstimationStrategy
{
    /// <summary>
    /// Characters divided by an average number of characters per token (4). Works for any model;
    /// the request budget's safety margin absorbs its error.
    /// </summary>
    public const string Heuristic = "Heuristic";

    /// <summary>
    /// The o200k_base encoding (GPT-4o, GPT-4.1, GPT-5, o-series).
    /// </summary>
    public const string O200kBase = "o200k_base";

    /// <summary>
    /// The cl100k_base encoding (GPT-4, GPT-3.5).
    /// </summary>
    public const string Cl100kBase = "cl100k_base";

    /// <summary>
    /// Every supported strategy.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[] { Heuristic, O200kBase, Cl100kBase };
}
