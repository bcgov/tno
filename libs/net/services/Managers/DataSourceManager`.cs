using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.DataSource;
using TNO.Services.Config;

namespace TNO.Services.Managers;

/// <summary>
/// DataSourceManager class, provides a way to manage several data source schedules.
/// It will fetch all data sources for the configured media types.
/// It will ensure all data sources are being run based on their schedules.
/// </summary>
public abstract class DataSourceManager<TDataSourceIngestManager, TOption> : ServiceManager<TOption>, IDataSourceManager
    where TDataSourceIngestManager : IDataSourceIngestManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly DataSourceIngestManagerFactory<TDataSourceIngestManager, TOption> _factory;
    private readonly Dictionary<int, TDataSourceIngestManager> _dataSources = new();
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a DataSourceManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api">Service to communicate with the api.</param>
    /// <param name="factory">Data source manager factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logging client.</param>
    public DataSourceManager(
        IApiService api,
        DataSourceIngestManagerFactory<TDataSourceIngestManager, TOption> factory,
        IOptions<TOption> options,
        ILogger<DataSourceManager<TDataSourceIngestManager, TOption>> logger)
        : base(api, options, logger)
    {
        _factory = factory;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service manager.
    /// Fetch the data sources for this service.
    /// Iterate through each data source and run or stop the action managers associated with each.
    /// Keep track of errors and respond to API state changes.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var dataSources = await GetDataSourcesAsync();

        // Run at the shortest interval of all schedules.
        var delay = dataSources.Min(ds => ds.DataSourceSchedules.Where(s => s.Schedule?.DelayMS != 0).Min(s => s.Schedule?.DelayMS)) ?? _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.Sleeping || this.State.Status == ServiceStatus.Paused)
            {
                this.Logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else if (!dataSources.Any(ds => ds.IsEnabled))
            {
                // If there are no data sources, then we need to keep the service alive.
                this.Logger.LogWarning("There are no configured data sources");
            }
            else
            {
                foreach (var dataSource in dataSources)
                {
                    // Update the delay if a schedule has changed and is less than the original value.
                    var delayMS = dataSource.DataSourceSchedules.Where(s => s.Schedule?.DelayMS > 0).Min(s => s.Schedule?.DelayMS) ?? delay;
                    delay = delayMS < delay ? delayMS : delay;

                    // Maintain a dictionary of managers for each data source.
                    // Fire event for the data source scheduler.
                    var hasKey = _dataSources.ContainsKey(dataSource.Id);
                    if (!hasKey) _dataSources.Add(dataSource.Id, _factory.Create(dataSource));
                    var manager = _dataSources[dataSource.Id];

                    // Ask all live threads to stop.
                    if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
                    {
                        await manager.StopAsync();
                        this.State.Stop();
                    }

                    // If the service isn't running, don't make additional requests.
                    if (this.State.Status != ServiceStatus.Running) continue;

                    try
                    {
                        if (dataSource.FailedAttempts >= dataSource.RetryLimit)
                        {
                            this.Logger.LogWarning("Data source '{Code}' has reached maximum failure limit", dataSource.Code);
                            continue;
                        }

                        // TODO: This needs to run asynchronously so that many data sources are being acted upon at one time.
                        await manager.RunAsync();

                        // Successful run clears any errors.
                        this.State.ResetFailures();
                    }
                    catch (HttpRequestException ex)
                    {
                        this.Logger.LogError(ex, "Data source '{Code}' failed to run: {Data}", dataSource.Code, ex.Data["Body"]);

                        // Update data source with failure.
                        await manager.RecordFailureAsync();
                        this.State.RecordFailure();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Data source '{Code}' failed to run", dataSource.Code);

                        // Update data source with failure.
                        await manager.RecordFailureAsync();
                        this.State.RecordFailure();
                    }
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            // await Thread.Sleep(new TimeSpan(0, 0, 0, delay));
            await Task.Delay(delay);

            // Fetch all data sources again to determine if there are any changes to the list.
            dataSources = await GetDataSourcesAsync();
        }
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch data sources for the configured media types.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync()
    {
        var dataSources = new List<DataSourceModel>();
        foreach (var mediaType in _options.MediaTypes)
        {
            try
            {
                // If the service isn't running, don't make additional requests.
                if (this.State.Status == ServiceStatus.Paused || this.State.Status == ServiceStatus.Sleeping) continue;

                dataSources.AddRange(await _api.GetDataSourcesAsync(mediaType));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to fetch data sources for media type", mediaType);
                this.State.RecordFailure();
            }
        }

        return dataSources;
    }
    #endregion
}
