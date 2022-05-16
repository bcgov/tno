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
public abstract class DataSourceManager<TDataSourceIngestManager, TOption> : ServiceManager<TOption>
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
            if (this.State.Status != ServiceStatus.Running)
            {
                _logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else if (!dataSources.Any(ds => ds.IsEnabled))
            {
                // If there are no data sources, then we need to keep the service alive.
                _logger.LogWarning("There are no configured data sources");
            }
            else
            {
                foreach (var dataSource in dataSources)
                {
                    // Update the delay if a schedule has changed and is less than the original value.
                    var delayMS = dataSource.DataSourceSchedules.Where(s => s.Schedule?.DelayMS > 0).Min(s => s.Schedule?.DelayMS) ?? delay;
                    delay = delayMS < delay ? delayMS : delay;

                    // If the service isn't running, don't make additional requests.
                    if (this.State.Status != ServiceStatus.Running) continue;

                    // Maintain a dictionary of managers for each data source.
                    // Fire event for the data source scheduler.
                    var hasKey = _dataSources.ContainsKey(dataSource.Id);
                    if (!hasKey) _dataSources.Add(dataSource.Id, _factory.Create(dataSource));
                    var manager = _dataSources[dataSource.Id];

                    try
                    {
                        if (dataSource.FailedAttempts >= dataSource.RetryLimit)
                        {
                            _logger.LogWarning("Data source '{Code}' has reached maximum failure limit", dataSource.Code);
                            continue;
                        }

                        // TODO: This needs to run asynchronously so that many data sources are being acted upon at one time.
                        // TODO: Need to propogate service status change to running threads.
                        await manager.RunAsync();

                        // Successful run clears any errors.
                        this.State.ResetFailures();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process data source '{Code}'", dataSource.Code);

                        // Update data source with failure.
                        await manager.RecordFailureAsync();
                        this.State.RecordFailure();
                    }
                }
            }

            // The delay ensures we don't have a run away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            _logger.LogDebug("Service sleeping for {delay} ms", delay);
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
    public async Task<IEnumerable<DataSourceModel>> GetDataSourcesAsync()
    {
        var dataSources = new List<DataSourceModel>();
        foreach (var mediaType in _options.MediaTypes)
        {
            try
            {
                // If the service isn't running, don't make additional requests.
                if (this.State.Status != ServiceStatus.Running) continue;

                dataSources.AddRange(await _api.GetDataSourcesAsync(mediaType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch data sources for media type", mediaType);
                this.State.RecordFailure();
            }
        }

        return dataSources;
    }
    #endregion
}
