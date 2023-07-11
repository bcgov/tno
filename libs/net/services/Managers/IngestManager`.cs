using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Services.Config;

namespace TNO.Services.Managers;

/// <summary>
/// IngestManager class, provides a way to manage several ingest schedules.
/// It will fetch all ingests for the configured ingest types.
/// It will ensure all ingests are being run based on their schedules.
/// </summary>
public abstract class IngestManager<TIngestServiceActionManager, TOption> : ServiceManager<TOption>, IIngestManager, IDisposable
    where TIngestServiceActionManager : IIngestServiceActionManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IngestManagerFactory<TIngestServiceActionManager, TOption> _factory;
    private readonly Dictionary<int, TIngestServiceActionManager> _ingests = new();
    private readonly IServiceScope _serviceScope;
    #endregion

    #region Properties
    /// <summary>
    /// get - A collection of ingest configurations currently being run.
    /// </summary>
    protected List<IngestModel> Ingests { get; private set; } = new List<IngestModel>();
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
        IServiceProvider serviceProvider,
        IApiService api,
        IngestManagerFactory<TIngestServiceActionManager, TOption> factory,
        IOptions<TOption> options,
        ILogger<IngestManager<TIngestServiceActionManager, TOption>> logger)
        : base(api, options, logger)
    {
        _factory = factory;
        _serviceScope = serviceProvider.CreateScope();
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
        this.Ingests.AddRange(await GetIngestsAsync());

        // Run at the shortest interval of all schedules.
        var delay = this.Ingests.Min(ds => ds.IngestSchedules.Where(s => s.Schedule?.DelayMS != 0 && s.Schedule?.IsEnabled == true).Min(s => s.Schedule?.DelayMS)) ?? this.Options.DefaultDelayMS;
        if (delay == 0) delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                await StopAllAsync(this.Ingests);
                this.State.Stop();
            }

            if (this.State.Status == ServiceStatus.Sleeping || this.State.Status == ServiceStatus.Paused)
            {
                this.Logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else if (!this.Ingests.Any(ds => ds.IsEnabled))
            {
                await StopAllAsync(this.Ingests);
                // If there are no ingests, then we need to keep the service alive.
                this.Logger.LogWarning("There are no configured ingests for this data location '{Location}'", this.Options.DataLocation);
            }
            else
            {
                foreach (var ingest in this.Ingests)
                {
                    // Update the delay if a schedule has changed and is less than the original value.
                    var delayMS = ingest.IngestSchedules.Where(s => s.Schedule?.DelayMS > 0).Min(s => s.Schedule?.DelayMS) ?? delay;
                    delay = delayMS < delay ? delayMS : delay;

                    // Maintain a dictionary of managers for each ingest.
                    // Fire event for the ingest scheduler.
                    var hasKey = _ingests.ContainsKey(ingest.Id);
                    if (!hasKey) _ingests.Add(ingest.Id, _factory.Create(ingest, _serviceScope));
                    var manager = _ingests[ingest.Id];

                    // Ask all live threads to stop.
                    if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
                    {
                        await StopAllAsync(_ingests.Values);
                        this.State.Stop();
                        break;
                    }
                    else
                    {
                        // Fetch the latest version of the ingest for checking if the location is still valid or not.
                        var theLatest = await Api.GetIngestAsync(ingest.Id);

                        if (theLatest == null ||
                            !theLatest.IsEnabled ||
                            !theLatest.IngestSchedules.Any(d => d.Schedule?.IsEnabled == true) ||
                            !theLatest.DataLocations.Any(d => d.Name.ToLower() == Options.DataLocation.ToLower()))
                        {
                            await manager.StopAsync();
                            continue;
                        }
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
            await Task.Delay(delay);

            // Fetch all ingests again to determine if there are any changes to the list.
            var ingests = await GetIngestsAsync();
            this.Ingests.Clear();
            this.Ingests.AddRange(ingests);
        }
    }

    private async Task StopAllAsync(List<IngestModel> ingests)
    {
        foreach (var ingest in ingests)
        {
            if (!_ingests.ContainsKey(ingest.Id)) _ingests.Add(ingest.Id, _factory.Create(ingest));
        }
        await StopAllAsync(_ingests.Values);
    }

    private async Task StopAllAsync(Dictionary<int, TIngestServiceActionManager>.ValueCollection managers)
    {
        foreach (var manager in managers)
        {
            await manager.StopAsync();
        }
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch ingests for the configured ingest types.
    /// </summary>
    /// <returns></returns>
    public virtual async Task<IEnumerable<IngestModel>> GetIngestsAsync()
    {
        var ingests = new List<IngestModel>();
        foreach (var ingestType in this.Options.GetIngestTypes())
        {
            try
            {
                var results = await this.Api.HandleRequestFailure<IEnumerable<IngestModel>>(
                    async () => await this.Api.GetIngestsForIngestTypeAsync(ingestType),
                    this.Options.ReuseIngests,
                    this.Ingests.Where(i => i.IngestType?.Name == ingestType).ToArray());
                // Only add the ingest configured for this data location.
                ingests.AddRange(results.Where(i => i.DataLocations.Any(d => d.Name.ToLower() == this.Options.DataLocation.ToLower())));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to fetch ingests for ingest type", ingestType);
                this.State.RecordFailure();
            }
        }

        return ingests;
    }

    /// <summary>
    /// Make an HTTP request to the api to fetch ingests for the configured ingest type.
    /// If the API fails it will configured to use the existing ingests if configured to do so.
    /// </summary>
    /// <param name="ingestType"></param>
    /// <returns></returns>
    protected virtual async Task<IEnumerable<IngestModel>> GetOrReuseIngestsAsync(string ingestType)
    {
        try
        {
            return await this.Api.GetIngestsForIngestTypeAsync(ingestType);
        }
        catch (Exception ex)
        {
            // If configured to reuse existing ingests it will ignore the error and continue running.
            if (!this.Options.ReuseIngests)
                throw;

            this.Logger.LogError(ex, "Ignoring error and reusing existing ingests");
            return this.Ingests.ToArray();
        }
    }

    public void Dispose()
    {
        _serviceScope.Dispose();
    }
    #endregion
}
