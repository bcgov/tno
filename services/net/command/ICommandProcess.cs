namespace TNO.Services.Command;

/// <summary>
/// ICommandProcess interface, provides a way to store a process and related information.
/// </summary>
public interface ICommandProcess
{
    #region Properties
    /// <summary>
    /// get/set - The process for this command.
    /// </summary>
    public System.Diagnostics.Process Process { get; set; }

    /// <summary>
    /// get/set - When the process was created.
    /// </summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// get - Data included in the command process.
    /// </summary>
    public Dictionary<string, object> Data { get; }
    #endregion
}
