using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Ingest;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Services.Config;

namespace TNO.Services.Managers;

/// <summary>
/// IngestManager class, provides a way to manage several ingest schedules.
/// It will fetch all ingests for the configured ingest types.
/// It will ensure all ingests are being run based on their schedules.
/// </summary>
public abstract class IngestManager<TActionManager, TOption> : ServiceManager<TOption>, IIngestManager, IDisposable
    where TActionManager : IIngestActionManager
    where TOption : IngestServiceOptions
{
    #region Variables
    private readonly IngestManagerFactory<TActionManager, TOption> _factory;
    private readonly Dictionary<int, TActionManager> _ingestManagers = [];
    private readonly IServiceScope _serviceScope;
    #endregion

    #region Properties
    /// <summary>
    /// get - A collection of ingest configurations currently being run.
    /// </summary>
    protected List<IngestModel> Ingests { get; private set; } = [];
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="api">Service to communicate with the api.</param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="factory">Ingest manager factory.</param>
    /// <param name="options">Configuration options.</param>
    /// <param name="logger">Logging client.</param>
    public IngestManager(
        IServiceProvider serviceProvider,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IngestManagerFactory<TActionManager, TOption> factory,
        IOptions<TOption> options,
        ILogger<IngestManager<TActionManager, TOption>> logger)
        : base(api, chesService, chesOptions, options, logger)
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
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                // Stop all running ingest processes.
                await StopAllAsync();
                this.State.Stop();
            }

            if (this.State.Status == ServiceStatus.Failed && this.Options.AutoRestartAfterCriticalFailure)
            {
                // If the service should auto-restart, wait a bit and then resume.
                await Task.Delay(this.Options.RetryAfterCriticalFailureDelayMS);
                this.State.Resume();
            }

            if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running '{Status}'", this.State.Status);
            }
            else if (!this.Ingests.Any(ds => ds.IsEnabled))
            {
                await StopAllAsync();
                // If there are no ingests, then we need to keep the service alive.
                this.Logger.LogWarning("There are no configured ingests for this data location '{Location}'", this.Options.DataLocation);
            }
            else
            {
                for (var index = 0; index < this.Ingests.Count; index++)
                {
                    var ingest = this.Ingests[index];

                    // Update the delay if a schedule has changed and is less than the original value.
                    var delayMS = ingest.IngestSchedules.Where(s => s.Schedule?.DelayMS > 0).Min(s => s.Schedule?.DelayMS) ?? delay;
                    delay = delayMS < delay ? delayMS : delay;

                    // Maintain a dictionary of managers for each ingest.
                    // Fire event for the ingest scheduler.
                    if (!_ingestManagers.ContainsKey(ingest.Id)) _ingestManagers.Add(ingest.Id, _factory.Create(ingest, _serviceScope));
                    var manager = _ingestManagers[ingest.Id];

                    // Ask all live threads to stop.
                    if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
                    {
                        await StopAllAsync();
                        this.State.Stop();
                        break;
                    }
                    else
                    {
                        // Fetch the latest version of the ingest for checking if the location is still valid or not.
                        var theLatest = await this.Api.HandleRequestFailure(async () => await this.Api.GetIngestAsync(ingest.Id), this.Options.ReuseIngests, ingest);
                        if (theLatest != null)
                        {
                            // Update local array.
                            this.Ingests[index] = theLatest;
                            manager.Ingest = theLatest;

                            if (!theLatest.IsEnabled ||
                                !theLatest.IngestSchedules.Any(d => d.Schedule?.IsEnabled == true) ||
                                !theLatest.DataLocations.Any(d => d.Name.Equals(Options.DataLocation, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                await manager.StopAsync();
                                continue;
                            }
                        }
                    }

                    // If the service isn't running, don't make additional requests.
                    if (this.State.Status != ServiceStatus.Running) break;

                    try
                    {
                        if (ingest.FailedAttempts >= ingest.RetryLimit)
                        {
                            if (ingest.ResetRetryAfterDelayMs > 0)
                            {
                                // Ingest is configured for auto-reset
                                // Check whether we should reset or wait longer.
                                if (ingest.LastRanOn.HasValue
                                    && ingest.LastRanOn.Value.AddMilliseconds(ingest.ResetRetryAfterDelayMs) <= DateTime.UtcNow)
                                {
                                    this.Logger.LogInformation("Ingest [{name}] failure auto reset after [{resetRetryDelay}] seconds", ingest.Name, ingest.ResetRetryAfterDelayMs);
                                    manager.Ingest.FailedAttempts = 0;
                                    // reset the Service.State.Failures as well, or the Service will be stuck
                                    this.State.ResetFailures();
                                }
                                else
                                {
                                    // Auto-reset time delay hasn't passed yet.
                                    this.Logger.LogWarning("Ingest [{name}] has reached maximum failure limit. Auto-reset will occur at [{timestamp}]", ingest.Name, ingest.LastRanOn!.Value.AddMilliseconds(ingest.ResetRetryAfterDelayMs).ToLocalTime());
                                    continue;
                                }
                            }
                            else
                            {
                                // Ingest is NOT configured for auto-reset
                                this.Logger.LogWarning("Ingest [{name}] has reached maximum failure limit and is NOT configured for Auto-reset.  Ingest is stopped as of [{lastRan}].", ingest.Name, ingest.LastRanOn!.Value.ToLocalTime());
                                continue;
                            }
                        }

                        // TODO: This needs to run asynchronously so that many ingests are being acted upon at one time.
                        await manager.RunAsync();

                        // Successful run clears any errors.
                        this.State.ResetFailures();
                    }
                    catch (Exception ex)
                    {
                        if (ex is HttpClientRequestException httpEx)
                        {
                            var response = httpEx.Response?.Content.ReadAsStringAsync().Result;
                            this.Logger.LogError(ex, "Ingest [{name}] failed to run. This is failure [{failures}] out of [{maxFailures}] maximum retries. Response: {Data}", ingest.Name, manager.Ingest.FailedAttempts + 1, manager.Ingest.RetryLimit, response);
                        }
                        else
                            this.Logger.LogError(ex, "Ingest [{name}] failed to run. This is failure [{failures}] out of [{maxFailures}] maximum retries.", ingest.Name, manager.Ingest.FailedAttempts + 1, manager.Ingest.RetryLimit);

                        // Update ingest with failure.
                        await manager.RecordFailureAsync(ex);
                        this.State.RecordFailure();
                        // Reached limit return to ingest manager, send email.
                        if (manager.Ingest.FailedAttempts >= manager.Ingest.RetryLimit)
                        {
                            await this.SendErrorEmailAsync($"Ingest ['{ingest.Name}'] failed. Reached [{manager.Ingest.RetryLimit}] maximum retries.", ex);
                        }
                    }
                }
            }

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all ingest schedules, it could mean some ingests are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            await Task.Delay(delay);

            await RefreshIngestsAsync();
        }
    }

    /// <summary>
    /// Fetch the latest ingests from the API.
    /// Update the local list and dictionary with the latest ingest configuration updates.
    /// </summary>
    /// <returns></returns>
    private async Task RefreshIngestsAsync()
    {
        // Fetch all ingests again to determine if there are any changes to the list.
        var ingests = (await GetIngestsAsync()).ToArray();
        for (var index = 0; index < ingests.Length; index++)
        {
            var latestIngest = ingests[index];
            var currentIngestIndex = this.Ingests.FindIndex(i => i.Id == latestIngest.Id);
            if (currentIngestIndex != -1)
            {
                var currentIngest = this.Ingests[currentIngestIndex];
                if (latestIngest.Version > currentIngest.Version)
                {
                    this.Ingests[currentIngestIndex] = latestIngest;

                    if (_ingestManagers.TryGetValue(latestIngest.Id, out var value))
                        value.Ingest = latestIngest;
                }
            }
            else
            {
                this.Ingests.Add(latestIngest);
            }
        }
    }

    /// <summary>
    /// Tell all ingests to stop.
    /// </summary>
    /// <returns></returns>
    private async Task StopAllAsync()
    {
        this.Logger.LogInformation("Stopping all ingests");
        foreach (var manager in _ingestManagers.Values)
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
                ingests.AddRange(results.Where(i => i.DataLocations.Any(d => d.Name.Equals(this.Options.DataLocation, StringComparison.CurrentCultureIgnoreCase))));
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to fetch ingests for ingest type '{type}'", ingestType);
                this.State.RecordFailure();
                await this.SendErrorEmailAsync($"Failed to fetch ingests for ingest type '{ingestType}", ex);
            }
        }

        return ingests;
    }

    /// <summary>
    /// Dispose this object.
    /// </summary>
    public void Dispose()
    {
        _serviceScope.Dispose();
        GC.SuppressFinalize(this);
    }
    #endregion
}
