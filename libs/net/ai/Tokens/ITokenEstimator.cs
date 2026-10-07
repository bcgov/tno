namespace TNO.AI.Tokens;

/// <summary>
/// ITokenEstimator interface, counts the tokens a model reads for a piece of text.
/// </summary>
public interface ITokenEstimator
{
    /// <summary>
    /// get - The strategy name.
    /// </summary>
    string Strategy { get; }

    /// <summary>
    /// Count the tokens in the specified text.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    int Count(string? text);
}
