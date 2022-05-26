namespace TNO.Core.Exceptions;

/// <summary>
/// MissingFileException class provides an exception when a file is missing.
/// </summary>
public class MissingFileException : Exception
{
    #region Properties
    /// <summary>
    /// get - The name of file that is missing.
    /// </summary>
    public string FileName { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a MissingFileException object.
    /// </summary>
    public MissingFileException()
    {
        this.FileName = "";
    }

    /// <summary>
    /// Creates a new instance of a MissingFileException object, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    public MissingFileException(string message) : base(message)
    {
        this.FileName = "";
    }

    /// <summary>
    /// Creates a new instance of a MissingFileException object, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fileName"></param>
    public MissingFileException(string message, string fileName) : base(message)
    {
        this.FileName = fileName;
    }

    /// <summary>
    /// Creates a new instance of a MissingFileException object, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public MissingFileException(string message, Exception innerException) : base(message, innerException)
    {
        this.FileName = "";
    }

    /// <summary>
    /// Creates a new instance of a MissingFileException object, initializes with specified parameters.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fileName"></param>
    /// <param name="innerException"></param>
    public MissingFileException(string message, string fileName, Exception innerException) : base(message, innerException)
    {
        this.FileName = fileName;
    }
    #endregion
}
