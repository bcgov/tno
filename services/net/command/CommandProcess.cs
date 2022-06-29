namespace TNO.Services.Command;

/// <summary>
/// CommandProcess class, provides a way to store a process and related information.
/// </summary>
public class CommandProcess : ICommandProcess
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandProcess object, initializes with specified paraemters.
    /// </summary>
    /// <param name="process"></param>
    public CommandProcess(System.Diagnostics.Process process) : this(process, DateTime.Now)
    {
    }

    /// <summary>
    /// Creates a new instance of a CommandProcess object, initializes with specified paraemters.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="createdOn"></param>
    public CommandProcess(System.Diagnostics.Process process, DateTime createdOn)
    {
        this.Process = process;
        this.CreatedOn = createdOn;
    }
    #endregion
}
