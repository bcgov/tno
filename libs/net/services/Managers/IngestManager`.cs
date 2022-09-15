using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Services.Config;

namespace TNO.Services.Managers;

/// <summary>
/// IngestManager class, provides a way to manage several ingest schedules.
/// It will fetch all ingests for the configured media types.
/// It will ensure all ingests are being run based on their schedules.
/// </summary>
public abstract class IngestManager<TIngestServiceActionManager, TOption> : ServiceManager<TOption>, IIngestManager
    where TIngestServiceActionManager : IIngestServiceActionManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IngestManagerFactory<TIngestServiceActionManager, TOption> _factory;
    private readonly Dictionary<int, TIngestServiceActionManager> _ingests = new();
    #endregion

    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api">Service to communicate with the api.</param>
    /// <param name="factory">Ingest manager factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logging client.</param>
    public IngestManager(
        IApiService api,
        IngestManagerFactory<TIngestServiceActionManager, TOption> factory,
        IOptions<TOption> options,
        ILogger<IngestManager<TIngestServiceActionManager, TOption>> logger)
        : base(api, options, logger)
    {
        _factory = factory;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the service manager.
    /// Fetch the ingests for this service.
    /// Iterate through each ingest and run or stop the action managers associated with each.
    /// Keep track of errors and respond to API state changes.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var ingests = await GetIngestsAsync();

        // Run at the shortest interval of all schedules.
        var delay = ingests.Min(ds => ds.IngestSchedules.Where(s => s.Schedule?.DelayMS != 0).Min(s => s.Schedule?.DelayMS)) ?? _options.DefaultDelayMS;
        if (delay == 0) delay = _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause) this.State.Stop();
            if (this.State.Status == ServiceStatus.Sleeping || this.State.Status == ServiceStatus.Paused)
            {
                this.Logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else if (!ingests.Any(ds => ds.IsEnabled))
            {
                // If there are no ingests, then we need to keep the service alive.
                this.Logger.LogWarning("There are no configured ingests");
            }
            else
            {
                foreach (var ingest in ingests)
                {
                    // Update the delay if a schedule has changed and is less than the original value.
                    var delayMS = ingest.IngestSchedules.Where(s => s.Schedule?.DelayMS > 0).Min(s => s.Schedule?.DelayMS) ?? delay;
                    delay = delayMS < delay ? delayMS : delay;

                    // Maintain a dictionary of managers for each ingest.
                    // Fire event for the ingest scheduler.
                    var hasKey = _ingests.ContainsKey(ingest.Id);
                    if (!hasKey) _ingests.Add(ingest.Id, _factory.Create(ingest));
                    var manager = _ingests[ingest.Id];

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
                        if (ingest.FailedAttempts >= ingest.RetryLimit)
                        {
                            this.Logger.LogWarning("Ingest '{name}' has reached maximum failure limit", ingest.Name);
                            continue;
                        }

                        // TODO: This needs to run asynchronously so that many ingests are being acted upon at one time.
                        await manager.RunAsync();

                        // Successful run clears any errors.
                        this.State.ResetFailures();
                    }
                    catch (HttpRequestException ex)
                    {
                        this.Logger.LogError(ex, "Ingest '{name}' failed to run: {Data}", ingest.Name, ex.Data["Body"]);

                        // Update ingest with failure.
                        await manager.RecordFailureAsync();
                        this.State.RecordFailure();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Ingest '{name}' failed to run", ingest.Name);

                        // Update ingest with failure.
                        await manager.RecordFailureAsync();
                        this.State.RecordFailure();
                    }
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all ingest schedules, it could mean some ingests are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            // await Thread.Sleep(new TimeSpan(0, 0, 0, delay));
            await Task.Delay(delay);

            // Fetch all ingests again to determine if there are any changes to the list.
            ingests = await GetIngestsAsync();
        }
    }

    /// <summary>
    /// Make an AJAX request to the api to fetch ingests for the configured media types.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var ingests = new List<IngestModel>();
        foreach (var mediaType in _options.GetMediaTypes())
        {
            try
            {
                // If the service isn't running, don't make additional requests.
                if (this.State.Status == ServiceStatus.Paused || this.State.Status == ServiceStatus.Sleeping) continue;

                ingests.AddRange(await _api.GetIngestsForMediaTypeAsync(mediaType));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to fetch ingests for media type", mediaType);
                this.State.RecordFailure();
            }
        }

        return ingests;
    }
    #endregion
}
