using Microsoft.ML.Tokenizers;

namespace TNO.AI.Tokens;

/// <summary>
/// TokenEstimator class, creates the token estimator for a configured strategy.
/// </summary>
public static class TokenEstimator
{
    private static readonly Lazy<Tokenizer> _o200k = new(() => TiktokenTokenizer.CreateForEncoding("o200k_base"));
    private static readonly Lazy<Tokenizer> _cl100k = new(() => TiktokenTokenizer.CreateForEncoding("cl100k_base"));

    /// <summary>
    /// Create the estimator for the specified strategy. An unknown or empty strategy uses the heuristic.
    /// </summary>
    /// <param name="strategy"></param>
    /// <returns></returns>
    public static ITokenEstimator Create(string? strategy)
    {
        if (String.Equals(strategy, TokenEstimationStrategy.O200kBase, StringComparison.OrdinalIgnoreCase))
            return new TiktokenEstimator(TokenEstimationStrategy.O200kBase, _o200k);
        if (String.Equals(strategy, TokenEstimationStrategy.Cl100kBase, StringComparison.OrdinalIgnoreCase))
            return new TiktokenEstimator(TokenEstimationStrategy.Cl100kBase, _cl100k);
        return new HeuristicTokenEstimator();
    }

    /// <summary>
    /// Whether the strategy name is supported.
    /// </summary>
    /// <param name="strategy"></param>
    /// <returns></returns>
    public static bool IsSupported(string? strategy)
        => TokenEstimationStrategy.All.Any(s => String.Equals(s, strategy, StringComparison.OrdinalIgnoreCase));

    private sealed class TiktokenEstimator : ITokenEstimator
    {
        private readonly Lazy<Tokenizer> _tokenizer;

        public TiktokenEstimator(string strategy, Lazy<Tokenizer> tokenizer)
        {
            this.Strategy = strategy;
            _tokenizer = tokenizer;
        }

        public string Strategy { get; }

        public int Count(string? text) => String.IsNullOrEmpty(text) ? 0 : _tokenizer.Value.CountTokens(text);
    }
}

/// <summary>
/// HeuristicTokenEstimator class, estimates tokens as characters divided by an average number of
/// characters per token, rounded up.
/// </summary>
public sealed class HeuristicTokenEstimator : ITokenEstimator
{
    /// <summary>
    /// The average number of characters per token for English prose.
    /// </summary>
    public const double DefaultCharactersPerToken = 4;

    private readonly double _charactersPerToken;

    /// <summary>
    /// Creates a new instance of a HeuristicTokenEstimator.
    /// </summary>
    /// <param name="charactersPerToken"></param>
    public HeuristicTokenEstimator(double charactersPerToken = DefaultCharactersPerToken)
    {
        _charactersPerToken = charactersPerToken > 0 ? charactersPerToken : DefaultCharactersPerToken;
    }

    /// <summary>
    /// get - The strategy name.
    /// </summary>
    public string Strategy => TokenEstimationStrategy.Heuristic;

    /// <summary>
    /// Estimate the tokens in the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public int Count(string? text) => String.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / _charactersPerToken);
}
