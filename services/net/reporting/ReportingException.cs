namespace TNO.Services.Reporting;

public class ReportingException : Exception
{
    #region Properties
    public ReportingErrors Error { get; }
    #endregion

    #region Constructors
    public ReportingException(ReportingErrors error, string? message, Exception? innerException)
        : base(message, innerException)
    {
        this.Error = error;
    }
    #endregion

    #region Methods
    #endregion
}
