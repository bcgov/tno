using System.Web;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.FileCopy.Config;
using TNO.Services.Managers;

namespace TNO.Services.FileCopy;

/// <summary>
/// FileCopyManager class, provides a Kafka Consumer service which will copy files to the locally mapped volume so that users can access them directly.
/// </summary>
public class FileCopyManager : ServiceManager<FileCopyOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly WorkOrderStatus[] _ignoreWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled, WorkOrderStatus.Failed };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, FileRequestModel> Listener { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FileCopyManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FileCopyManager(
        IKafkaListener<string, FileRequestModel> listener,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<FileCopyOptions> options,
        ILogger<FileCopyManager> logger)
        : base(api, chesService, chesOptions, options, logger)
    {
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Listen to active topics and import content.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = this.Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause || this.State.Status == ServiceStatus.RequestFailed)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Listener.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    var topics = this.Options.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (topics.Length != 0)
                    {
                        this.Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Service had an unexpected failure.");
                    this.State.RecordFailure();
                }
            }

            // The delay ensures we don't have a run away thread.
            this.Logger.LogDebug("Service sleeping for {delay} ms", delay);
            await Task.Delay(delay);
        }
    }

    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new Task if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            // Make sure the prior task is cancelled before creating a new one.
            if (_cancelToken?.IsCancellationRequested == false)
                _cancelToken?.Cancel();
            _cancelToken = new CancellationTokenSource();
            _consumer = Task.Run(ListenerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ListenerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Listener.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private void ListenerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                this.Listener.Stop();
        }
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken != null && !_cancelToken.IsCancellationRequested)
        {
            _cancelToken.Cancel();
        }
    }

    /// <summary>
    /// Handle the request to copy a remote file.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, FileRequestModel> result)
    {
        try
        {
            var request = result.Message.Value;
            // The service has stopped, so to should consuming messages.
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Listener.Stop();
                this.State.Stop();
            }
            else
            {
                // TODO: Handle multi-threading so that more than one file copy can be performed at a time.
                await CopyFileAsync(request);

                // Inform Kafka this message is completed.
                this.Listener.Commit(result);
                this.Listener.Resume();

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
            }
        }
        catch (Exception ex)
        {
            if (ex is HttpClientRequestException httpEx)
            {
                this.Logger.LogError(ex, "HTTP exception while consuming. {response}", httpEx.Data["body"] ?? "");
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Copy a remote file to the locally mapped volume.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task CopyFileAsync(FileRequestModel request)
    {
        // TODO: Handle blocked work orders stuck in progress.
        var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
        if (workOrder == null || workOrder.Status != WorkOrderStatus.Submitted)
        {
            this.Logger.LogWarning("File copy request ignored. Work Order ID: {id}, Path: {path}", request.WorkOrderId, request.Path);
            return;
        }

        await UpdateWorkOrderAsync(request, WorkOrderStatus.InProgress);

        // Remote storage locations may not be easily accessible by this service.
        var location = await this.Api.GetDataLocationAsync(request.LocationId);
        var path = HttpUtility.UrlDecode(request.Path).MakeRelativePath();

        // Only attempt to copy a file from a remote location.
        if (location?.Connection != null && location.Connection.Name != this.Options.DataLocation)
        {
            if (location.Connection.Name == this.Options.DataLocation)
            {
                this.Logger.LogError("Ignoring file copy request from this location");
            }
            else if (location.Connection.ConnectionType == ConnectionType.SSH)
            {
                var configuration = location.Connection.Configuration;
                var remoteLocationRoot = configuration.GetDictionaryJsonValue<string>("path") ?? "";
                using var client = SftpHelper.CreateSftpClient(configuration);
                try
                {
                    client.Connect();
                    var remotePath = Path.Combine(remoteLocationRoot, path);
                    if (!client.Exists(remotePath))
                    {
                        this.Logger.LogError("File does not exist.  Path: {path}", remotePath);
                        await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
                        return;
                    }

                    // Copy file from remote to locally mapped volume.
                    var localPath = Path.Combine(this.Options.VolumePath, "_tmp", request.Path);
                    var localDirectory = Path.GetDirectoryName(localPath) ?? throw new ConfigurationException("Local path is invalid");
                    if (!Directory.Exists(localDirectory)) Directory.CreateDirectory(localDirectory);

                    this.Logger.LogInformation("File copy started. Path: {path}", remotePath);
                    SftpHelper.CopyFile(client, remotePath, localPath);
                    this.Logger.LogInformation("File copy completed. Path: {path}", remotePath);

                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Unable to complete file copy request.  Path: {path}", request.Path);
                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            }
            else
            {
                this.Logger.LogError("Location connection type has not been implemented '{type}'.", location.Connection.ConnectionType);
                await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
            }
        }
        else
        {
            this.Logger.LogDebug("Location does not exist.");
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
        }
    }

    /// <summary>
    /// Update the work order (if it exists) with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task<bool> UpdateWorkOrderAsync(FileRequestModel request, WorkOrderStatus status)
    {
        if (request.WorkOrderId > 0)
        {
            var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
            if (workOrder != null && !_ignoreWorkOrders.Contains(workOrder.Status))
            {
                workOrder.Status = status;
                await this.Api.UpdateWorkOrderAsync(workOrder);
                return true;
            }
        }
        return false;
    }
    #endregion
}
