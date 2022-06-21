using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Models.Extensions;
using TNO.Services.Actions;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandAction class, performs the command ingestion action.
/// Execute configured cli command.
/// </summary>
public abstract class CommandAction<TOptions> : IngestAction<TOptions>
    where TOptions : CommandOptions
{
    #region Properties
    /// <summary>
    /// get - The logger for the command action.
    /// </summary>
    public ILogger Logger { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandAction(IApiService api, IOptions<TOptions> options, ILogger<CommandAction<TOptions>> logger) : base(api, options)
    {
        this.Logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <param name="data"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync<T>(IDataSourceIngestManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.DataSource))
        {
            var process = await GetProcessAsync(manager, schedule);
            var isRunning = IsRunning(process);

            if (name == "start" && !isRunning)
            {
                RunProcess(process);
            }
            else if (name == "stop")
            {
                await StopProcessAsync(process, cancellationToken);
                RemoveProcess(manager, schedule);
            }
        }
    }

    /// <summary>
    /// Only return schedules that relevant.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    protected virtual IEnumerable<ScheduleModel> GetSchedules(DataSourceModel dataSource)
    {
        return dataSource.DataSourceSchedules.Where(s =>
            s.Schedule != null
        ).Select(ds => ds.Schedule!);
    }

    /// <summary>
    /// Generate a process key to identify it.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual string GenerateProcessKey(DataSourceModel dataSource, ScheduleModel schedule)
    {
        return $"{dataSource.Code}-{schedule.Name}={schedule.Id}";
    }

    /// <summary>
    /// Run the specified process.
    /// Do not wait for the process to exit.
    /// This will leave a process running.
    /// </summary>
    /// <param name="process"></param>
    /// <exception cref="Exception"></exception>
    protected void RunProcess(System.Diagnostics.Process process)
    {
        var cmd = process.StartInfo.Arguments;
        this.Logger.LogInformation("Starting process for command: {cmd}", cmd);
        if (!process.Start()) this.Logger.LogError("Unable to start service command for data source '{Code}'.", process.StartInfo.Verb);

        // We can't wait because it would block all other Command service cmds.  So we test for an early exit.
        if (process.HasExited) throw new Exception($"Failed to start command: {cmd}");
    }

    /// <summary>
    /// Run the specified process and wait for it to complete.
    /// Wait for the process to exit.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="Exception"></exception>
    protected async Task RunProcessAsync(System.Diagnostics.Process process, CancellationToken cancellationToken = default)
    {
        RunProcess(process);
        await process.WaitForExitAsync(cancellationToken);
    }

    /// <summary>
    /// Stop the specified process.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task StopProcessAsync(System.Diagnostics.Process process, CancellationToken cancellationToken = default)
    {
        if (!process.HasExited)
        {
            process.Kill(true);
            await process.WaitForExitAsync(cancellationToken);
        }
        process.Dispose();
    }

    /// <summary>
    /// Get the process for the specified data source.
    /// Creates a new one if it doesn't already exist.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual System.Diagnostics.Process GetProcess(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        var key = GenerateProcessKey(manager.DataSource, schedule);
        if (manager.Values.GetValueOrDefault(key) is not System.Diagnostics.Process process)
        {
            var cmd = GenerateCommandAsync(manager, schedule).Result;
            process = new System.Diagnostics.Process();
            process.StartInfo.Verb = key;
            process.StartInfo.FileName = "/bin/sh";
            process.StartInfo.Arguments = $"-c \"{cmd}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.EnableRaisingEvents = true;
            process.Exited += async (sender, e) => await OnExitedAsync(sender, manager, e);
            process.ErrorDataReceived += OnError;
            // process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.RedirectStandardError = true;

            // Keep a reference to the running process.
            manager.Values[key] = process;
        }

        return process;
    }

    /// <summary>
    /// Get the process for the specified data source.
    /// Creates a new one if it doesn't already exist.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual Task<System.Diagnostics.Process> GetProcessAsync(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        return Task.FromResult(GetProcess(manager, schedule));
    }

    /// <summary>
    /// Remove the process from memory.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    protected virtual void RemoveProcess(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        manager.Values.Remove(GenerateProcessKey(manager.DataSource, schedule));
    }

    /// <summary>
    /// Log exit information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected async Task OnExitedAsync(object? sender, IDataSourceIngestManager manager, EventArgs e)
    {
        if (sender is System.Diagnostics.Process process)
        {
            var cmd = process.StartInfo.Arguments;
            if (process.ExitCode != 0 && process.ExitCode != 137)
            {
                this.Logger.LogError("Service command '{cmd}' exited", cmd);
                await manager.RecordFailureAsync();
            }
            else
            {
                this.Logger.LogDebug("Service command '{cmd}' exited", cmd);
            }
        }
    }

    /// <summary>
    /// Log error information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void OnError(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (sender is System.Diagnostics.Process process)
        {
            var cmd = process.StartInfo.Arguments;
            this.Logger.LogError("Service command '{cmd}' failure: {Data}", cmd, e.Data);
        }
    }

    /// <summary>
    /// Determine if the process is running.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    protected static bool IsRunning(System.Diagnostics.Process process)
    {
        try
        {
            System.Diagnostics.Process.GetProcessById(process.Id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Convert to timezone and return as local.
    /// Dates should be stored in the timezone of the data source.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="date"></param>
    /// <returns></returns>
    protected DateTime GetLocalDateTime(DataSourceModel dataSource, DateTime date)
    {
        return date.ToTimeZone(CommandDataSourceManager.GetTimeZone(dataSource, this.Options.TimeZone));
    }

    /// <summary>
    /// Generate the command for this service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual string GenerateCommand(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        return GetCommand(manager.DataSource).Replace("\"", "'");
    }

    /// <summary>
    /// Generate the command for the service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual Task<string> GenerateCommandAsync(IDataSourceIngestManager manager, ScheduleModel schedule)
    {
        return Task.FromResult(GenerateCommand(manager, schedule));
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetCommand(DataSourceModel dataSource)
    {
        return dataSource.GetConnectionValue("cmd");
    }
    #endregion
}
