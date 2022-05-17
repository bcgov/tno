using TNO.Services.Config;

namespace TNO.Services.Actions.Managers;

/// <summary>
/// ServiceActionManager class, provides a way to manage the ingestion process for this data source.
/// </summary>
public abstract class ServiceActionManager<TOptions> : IServiceActionManager
    where TOptions : ServiceOptions
{
    #region Variables
    private readonly IServiceAction<TOptions> _action;
    #endregion

    #region Propeties
    /// <summary>
    /// get - Whether the current manager is running.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// get - The number of times this data source process has been run.
    /// </summary>
    public int RanCounter { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceIngestManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="action"></param>
    public ServiceActionManager(IServiceAction<TOptions> action)
    {
        _action = action;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    public async Task RunAsync()
    {
        var run = await PreRunAsync();
        if (run && !this.IsRunning)
        {
            try
            {
                this.IsRunning = true;

                await PerformActionAsync();

                this.RanCounter++;

                await PostRunAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                this.IsRunning = false;
            }
        }
    }

    /// <summary>
    /// Determine if the run should be executed.
    /// </summary>
    /// <returns></returns>
    protected virtual Task<bool> PreRunAsync()
    {
        return Task.FromResult(true);
    }

    /// <summary>
    /// Call the configured action.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task PerformActionAsync()
    {
        // Perform configured action.
        await _action.PerformActionAsync(this);
    }

    /// <summary>
    /// Perform activity after a successful run.
    /// </summary>
    /// <returns></returns>
    protected virtual async Task PostRunAsync()
    {
        // Inform data source of run.
        await RecordSuccessAsync();
    }

    /// <summary>
    /// Inform data source of successful run.
    /// </summary>
    /// <returns></returns>
    public abstract Task RecordSuccessAsync();

    /// <summary>
    /// Inform data source of failure.
    /// </summary>
    /// <returns></returns>
    public abstract Task RecordFailureAsync();
    #endregion
}
