namespace TNO.Core.Exceptions;

/// <summary>
/// NoContentException class, provides a way to throw an exception when a record does not exist.
/// </summary>
public class NoContentException : Exception
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a NoContentException class.
    /// </summary>
    /// <returns></returns>
    public NoContentException() : base() { }

    /// <summary>
    /// Creates a new instance of a NoContentException class, and initializes it with the specified arguments.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public NoContentException(string message) : base(message ?? "User is not authorized to perform this action.") { }

    /// <summary>
    /// Creates a new instance of a NoContentException class, and initializes it with the specified arguments.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    /// <returns></returns>
    public NoContentException(string message, Exception innerException) : base(message ?? "User is not authorized to perform this action.", innerException) { }
    #endregion
}
