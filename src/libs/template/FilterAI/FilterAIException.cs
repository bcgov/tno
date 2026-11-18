namespace TNO.TemplateEngine.FilterAI;

/// <summary>
/// FilterAIException class, the model could not produce a usable answer. The message is written
/// for the person using the prompt.
/// </summary>
public class FilterAIException : Exception
{
    /// <summary>
    /// Creates a new instance of a FilterAIException, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public FilterAIException(string message, Exception? innerException = null) : base(message, innerException) { }
}
