using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Core.Extensions;
using TNO.Services.Actions;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandAction class, performs the command ingestion action.
/// Fetch command feed.
/// Send message to Kafka.
/// Inform api of new content.
/// </summary>
public abstract class CommandAction<TOptions> : IngestAction<TOptions>
    where TOptions : CommandOptions
{
    #region Variables
    /// <summary>
    /// The 'cmd' values key name.
    /// </summary>
    public const string PROCESS_KEY = "cmd";
    private readonly ILogger _logger;
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
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Perform the ingestion service action.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public override async Task PerformActionAsync(IDataSourceIngestManager manager, string? name = null)
    {
        _logger.LogDebug("Performing ingestion service action for data source '{Code}'", manager.DataSource.Code);

        var process = GetProcess(manager, PROCESS_KEY);

        // TODO: Not sure if a terminated process is considered running.
        var isRunning = IsRunning(process);

        if (name == "start" && !isRunning)
        {
            var cmd = process.StartInfo.Arguments;
            _logger.LogInformation("Starting process for command: {cmd}", cmd);
            if (!process.Start()) _logger.LogError("Unable to start service command for data source '{Code}'.", manager.DataSource.Code);

            // We can't wait because it would block all other Command service cmds.  So we test for an early exit.
            // The exit failure should be commandd and logged by the event listener.
            if (process.HasExited) throw new Exception($"Failed to start command: {cmd}");
        }
        else if (name == "stop")
        {
            if (!process.HasExited)
            {
                process.Kill(true);
                await process.WaitForExitAsync();
            }
            process.Exited -= OnExited;
            process.ErrorDataReceived -= OnError;
            process.Dispose();
            manager.Values.Remove(PROCESS_KEY);
        }
    }

    /// <summary>
    /// Get the process for the specified data source.
    /// Creates a new one if it doesn't already exist.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    private System.Diagnostics.Process GetProcess(IDataSourceIngestManager manager, string key)
    {
        if (manager.Values.GetValueOrDefault(key) is not System.Diagnostics.Process process)
        {
            process = new System.Diagnostics.Process();
            process.StartInfo.Verb = $"Command={manager.DataSource.Code}";
            process.StartInfo.FileName = "/bin/sh";
            process.StartInfo.Arguments = $"-c \"{GenerateCommand(manager)}\"";
            process.StartInfo.UseShellExecute = false;
            process.EnableRaisingEvents = true;
            process.Exited += OnExited;
            process.ErrorDataReceived += OnError;
            // process.StartInfo.RedirectStandardOutput = true;
            // process.StartInfo.RedirectStandardError = true;

            // Keep a reference to the running process.
            manager.Values[key] = process;
        }

        return process;
    }

    /// <summary>
    /// Log exit information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnExited(object? sender, EventArgs e)
    {
        if (sender is System.Diagnostics.Process process)
        {
            var cmd = process.StartInfo.Arguments;
            if (process.ExitCode != 0 && process.ExitCode != 137)
            {
                _logger.LogError("Service command '{cmd}' exited", cmd);
            }
            else
            {
                _logger.LogDebug("Service command '{cmd}' exited", cmd);
            }
        }
    }

    /// <summary>
    /// Log error information for the process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnError(object? sender, System.Diagnostics.DataReceivedEventArgs e)
    {
        if (sender is System.Diagnostics.Process process)
        {
            var cmd = process.StartInfo.Arguments;
            _logger.LogError("Service command '{cmd}' failure: {Data}", cmd, e.Data);
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
        }
        catch
        {
            return false;
        }
        return true;
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
    /// <returns></returns>
    protected virtual string GenerateCommand(IDataSourceIngestManager manager)
    {
        return GetCommand(manager.DataSource).Replace("\"", "'");
    }

    /// <summary>
    /// Get the other arguments from the connection settings.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetCommand(DataSourceModel dataSource)
    {
        if (!dataSource.Connection.TryGetValue("cmd", out object? element)) throw new InvalidOperationException("Data source connection information is missing 'cmd'.");

        var value = (JsonElement)element;
        if (value.ValueKind == JsonValueKind.String)
        {
            if (String.IsNullOrWhiteSpace(value.GetString())) throw new InvalidOperationException("Data source connection information is missing 'cmd'.");
            return value.GetString()!;
        }

        throw new InvalidOperationException("Data source connection 'cmd' is not a valid string value");
    }
    #endregion
}
