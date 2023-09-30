using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Services.Actions.Managers;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandIngestActionManager class, provides a way to manage the command ingestion process for this data source.
/// </summary>
public abstract class CommandIngestActionManager<TOptions> : IngestActionManager<TOptions>
    where TOptions : CommandOptions
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandIngestActionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="ingest"></param>
    /// <param name="api"></param>
    /// <param name="ches"></param>
    /// <param name="chesOptions"></param>
    /// <param name="action"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public CommandIngestActionManager(
        IngestModel ingest,
        IApiService api,
        IChesService ches,
        IOptions<ChesOptions> chesOptions,
        IIngestAction<TOptions> action,
        IOptions<TOptions> options,
        ILogger<IServiceActionManager> logger)
        : base(ingest, api, ches, chesOptions, action, options, logger)
    {
    }
    #endregion

    #region Methods
    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    public override async Task RunAsync()
    {
        var run = await PreRunAsync();
        try
        {
            if (run)
            {
                this.IsRunning = true;
                await HandleActionAsync("start");
            }
            else if (!run && this.IsRunning)
            {
                await HandleActionAsync("stop");
                this.IsRunning = false;
            }
        }
        catch
        {
            this.IsRunning = false;
            throw;
        }
    }

    /// <summary>
    /// Stop the running service action.
    /// </summary>
    /// <returns></returns>
    public override async Task StopAsync()
    {
        try
        {
            if (this.IsRunning)
            {
                await HandleActionAsync("stop");
            }
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

    /// <summary>
    /// Based on the schedule run the process for this data source.
    /// </summary>
    /// <param name="name">A way to identify different run actions.</param>
    private async Task HandleActionAsync(string? name = null)
    {
        var result = await PerformActionAsync(name);

        this.RanCounter++;

        await PostRunAsync(result);
    }

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyIngest()
    {
        if (!this.Ingest.Configuration.ContainsKey("cmd")) return false;

        var value = (JsonElement)this.Ingest.Configuration["cmd"];

        if (value.ValueKind == JsonValueKind.String)
            return !String.IsNullOrWhiteSpace(value.GetString());

        return false;
    }
    #endregion
}
