using Microsoft.Extensions.Options;
using System.Text.Json;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Actions.Managers;
using TNO.Services.Command.Config;

namespace TNO.Services.Command;

/// <summary>
/// CommandDataSourceManager class, provides a way to manage the command ingestion process for this data source.
/// </summary>
public abstract class CommandDataSourceManager<TOptions> : DataSourceIngestManager<TOptions>
    where TOptions : CommandOptions
{
    #region Constructors
    /// <summary>
    /// Creates a new instance of a CommandDataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <param name="action"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    public CommandDataSourceManager(DataSourceModel dataSource, IApiService api, IIngestAction<TOptions> action, IOptions<TOptions> options)
        : base(dataSource, api, action, options)
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
        await PerformActionAsync(name);

        this.RanCounter++;

        await PostRunAsync();
    }

    /// <summary>
    /// Verify that the specified data source ingestion action should be run.
    /// </summary>
    /// <returns></returns>
    public override bool VerifyDataSource()
    {
        if (!this.DataSource.Connection.ContainsKey("cmd")) return false;

        var value = (JsonElement)this.DataSource.Connection["cmd"];

        if (value.ValueKind == JsonValueKind.String)
            return !String.IsNullOrWhiteSpace(value.GetString());

        return false;
    }
    #endregion
}
