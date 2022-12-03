using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
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
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandAction, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandAction(IApiService api, IOptions<TOptions> options, ILogger<CommandAction<TOptions>> logger) : base(api, options, logger)
    {
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
    public override async Task PerformActionAsync<T>(IIngestServiceActionManager manager, string? name = null, T? data = null, CancellationToken cancellationToken = default) where T : class
    {
        this.Logger.LogDebug("Performing ingestion service action for data source '{name}'", manager.Ingest.Name);

        // Each schedule will have its own process.
        foreach (var schedule in GetSchedules(manager.Ingest))
        {
            var process = await GetProcessAsync(manager, schedule);
            var isRunning = IsRunning(process);

            // Override the original action name based on the schedule.
            name = manager.VerifySchedule(schedule) ? "start" : "stop";

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
    /// <param name="ingest"></param>
    /// <returns></returns>
    protected virtual IEnumerable<ScheduleModel> GetSchedules(IngestModel ingest)
    {
        return ingest.IngestSchedules.Where(s =>
            s.Schedule != null
        ).Select(ds => ds.Schedule!);
    }

    /// <summary>
    /// Generate a process key to identify it.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual string GenerateProcessKey(IngestModel ingest, ScheduleModel schedule)
    {
        return $"{ingest.Source?.Code}-{schedule.Name}:{schedule.Id}";
    }

    /// <summary>
    /// Run the specified process.
    /// Do not wait for the process to exit.
    /// This will leave a process running.
    /// </summary>
    /// <param name="process"></param>
    /// <exception cref="Exception"></exception>
    protected void RunProcess(ICommandProcess process)
    {
        var cmd = process.Process.StartInfo.Arguments;
        this.Logger.LogInformation("Starting process for command: {cmd}", cmd);

        if (!process.Process.Start()) this.Logger.LogError("Unable to start service command for data source '{Code}'.", process.Process.StartInfo.Verb);
        process.Process.BeginOutputReadLine();
        process.Process.BeginErrorReadLine();

        // We can't wait because it would block all other Command service cmds.  So we test for an early exit.
        if (process.Process.HasExited) throw new Exception($"Failed to start command: {cmd}");
    }

    /// <summary>
    /// Run the specified process and wait for it to complete.
    /// Wait for the process to exit.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="Exception"></exception>
    protected async Task RunProcessAsync(ICommandProcess process, CancellationToken cancellationToken = default)
    {
        RunProcess(process);
        await process.Process.WaitForExitAsync(cancellationToken);
    }

    /// <summary>
    /// Stop the specified process.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected virtual async Task StopProcessAsync(ICommandProcess process, CancellationToken cancellationToken = default)
    {
        var args = process.Process.StartInfo.Arguments;
        this.Logger.LogInformation("Stopping process for command '{args}'", args);
        if (IsRunning(process) && !process.Process.HasExited)
        {
            process.Process.Kill(true);
            await process.Process.WaitForExitAsync(cancellationToken);
        }
        process.Process.Dispose();
    }

    /// <summary>
    /// Get the process for the specified data source.
    /// Creates a new one if it doesn't already exist.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual ICommandProcess GetProcess(IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        var key = GenerateProcessKey(manager.Ingest, schedule);
        if (manager.Values.GetValueOrDefault(key) is not ICommandProcess value)
        {
            var process = new System.Diagnostics.Process();
            value = new CommandProcess(process);

            process.StartInfo.Verb = key;
            var cmd = GetCommand(manager.Ingest);
            process.StartInfo.FileName = String.IsNullOrWhiteSpace(cmd) ? this.Options.Command : cmd;
            process.StartInfo.Arguments = GenerateCommandArgumentsAsync(value, manager, schedule).Result;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.EnableRaisingEvents = true;
            process.Exited += async (sender, e) => await OnExitedAsync(sender, manager, e);
            process.ErrorDataReceived += (sender, e) => OnErrorReceived(sender, manager, e);
            process.OutputDataReceived += (sender, e) => OnOutputReceived(sender, manager, e);

            // Keep a reference to the running process.
            manager.Values[key] = value;
        }

        return value;
    }

    /// <summary>
    /// Get the process for the specified data source.
    /// Creates a new one if it doesn't already exist.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual Task<ICommandProcess> GetProcessAsync(IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        return Task.FromResult(GetProcess(manager, schedule));
    }

    /// <summary>
    /// Remove the process from memory.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    protected virtual void RemoveProcess(IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        manager.Values.Remove(GenerateProcessKey(manager.Ingest, schedule));
    }

    /// <summary>
    /// Log exit information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected virtual void OnOutputReceived(object? sender, IIngestServiceActionManager? manager, DataReceivedEventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(e.Data))
        {
            this.Logger.LogDebug("{data}", e.Data);
        }
    }

    /// <summary>
    /// Log error information for the process.
    /// Many processes send all their output to the error output so override accordingly.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected virtual void OnErrorReceived(object? sender, IIngestServiceActionManager? manager, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (!String.IsNullOrWhiteSpace(e.Data))
        {
            if (sender is System.Diagnostics.Process process)
            {
                var args = process.StartInfo.Arguments;
                this.Logger.LogError("Service arguments '{args}' failure: {Data}", args, e.Data);
            }
        }
    }

    /// <summary>
    /// Log exit information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="manager"></param>
    /// <param name="e"></param>
    protected async Task OnExitedAsync(object? sender, IIngestServiceActionManager manager, EventArgs e)
    {
        if (sender is System.Diagnostics.Process process)
        {
            var args = process.StartInfo.Arguments;
            if (process.ExitCode != 0 && process.ExitCode != 137)
            {
                this.Logger.LogError("Service arguments '{args}' exited", args);
                await manager.RecordFailureAsync();
            }
            else
            {
                this.Logger.LogDebug("Service arguments '{args}' exited", args);
            }

            // The process has exited, remove it from the manager so that it can get recreated again.
            var key = process.StartInfo.Verb;
            if (manager.Values.ContainsKey(key))
                manager.Values.Remove(key);
            process.Dispose();
        }
    }

    /// <summary>
    /// Determine if the process is running.
    /// </summary>
    /// <param name="process"></param>
    /// <returns></returns>
    protected static bool IsRunning(ICommandProcess process)
    {
        try
        {
            System.Diagnostics.Process.GetProcessById(process.Process.Id);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Generate the command arguments for this service action.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual string GenerateCommandArguments(ICommandProcess process, IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        // TODO: This should be only arguments.
        return GetCommand(manager.Ingest)?.Replace("\"", "'") ?? "";
    }

    /// <summary>
    /// Generate the command arguments for the service action.
    /// </summary>
    /// <param name="process"></param>
    /// <param name="manager"></param>
    /// <param name="schedule"></param>
    /// <returns></returns>
    protected virtual Task<string> GenerateCommandArgumentsAsync(ICommandProcess process, IIngestServiceActionManager manager, ScheduleModel schedule)
    {
        return Task.FromResult(GenerateCommandArguments(process, manager, schedule));
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="ingest"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string? GetCommand(IngestModel ingest)
    {
        return ingest.GetConfigurationValue<string?>("cmd");
    }
    #endregion
}
