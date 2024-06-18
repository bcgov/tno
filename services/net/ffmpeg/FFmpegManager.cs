using Confluent.Kafka;
using FTTLib;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.FFmpeg.Config;
using TNO.Services.Managers;

namespace TNO.Services.FFmpeg;

/// <summary>
/// FFmpegManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class FFmpegManager : ServiceManager<FFmpegOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly WorkOrderStatus[] _activeWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Submitted, WorkOrderStatus.InProgress, WorkOrderStatus.Completed };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, IndexRequestModel> Listener { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FFmpegManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public FFmpegManager(
        IKafkaListener<string, IndexRequestModel> listener,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<FFmpegOptions> options,
        ILogger<FFmpegManager> logger)
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
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
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
    /// Retrieve a file from storage and send to Microsoft Cognitive Services. Obtain
    /// the transcription and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, IndexRequestModel> result)
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
                var content = await this.Api.FindContentByIdAsync(request.ContentId);
                if (content != null)
                {
                    // TODO: Handle multi-threading so that more than one transcription can be performed at a time.
                    await UpdateAVAsync(request, content);
                }
                else
                {
                    // Identify requests for transcription for content that does not exist.
                    this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, request.ContentId);
                }

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
    /// Make a request to generate a transcription for the specified 'content'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task UpdateAVAsync(IndexRequestModel request, ContentModel content)
    {
        // TODO: Handle different storage locations.
        // Remote storage locations may not be easily accessible by this service.
        var fileRef = content.FileReferences.FirstOrDefault();
        if (fileRef != null)
        {
            // Check if there is already a work order processing or processed this content.
            var workOrders = await this.Api.FindWorkOrderForContentIdAsync(content.Id);
            if (workOrders.Any(wo => _activeWorkOrders.Contains(wo.Status))) return;

            var sourcePath = Path.Join(this.Options.VolumePath, fileRef.Path.MakeRelativePath());
            if (File.Exists(sourcePath))
            {
                var sourceExt = Path.GetExtension(sourcePath);
                var process = this.Options.Converters.FirstOrDefault(c => c.MediaTypeId == content.MediaTypeId && c.FromFormat.Equals(sourceExt, StringComparison.OrdinalIgnoreCase));
                if (process != null)
                {
                    var workOrder = await AddWorkOrderAsync(request, WorkOrderStatus.InProgress) ?? throw new InvalidOperationException("Work order failed to be returned by API");
                    var contentType = !String.IsNullOrWhiteSpace(process.ToContentType) ? process.ToContentType : FTT.GetMimeType(process.ToFormat.Replace(".", ""));

                    try
                    {
                        this.Logger.LogInformation("Converting file. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
                        var newFile = await ConvertFile(sourcePath, process.ToFormat);
                        if (!String.IsNullOrEmpty(newFile))
                        {
                            fileRef.Path = fileRef.Path.Replace(process.FromFormat, process.ToFormat);
                            fileRef.FileName = Path.GetFileName(fileRef.Path);
                            fileRef.ContentType = contentType;

                            this.Logger.LogInformation("Content has been processed.  Content ID: {id}, Path: {path}", request.ContentId, fileRef.Path);
                            await this.Api.UpdateFileAsync(content, true, request.RequestorId);
                            workOrder.Status = WorkOrderStatus.Completed;
                            await this.Api.UpdateWorkOrderAsync(workOrder);
                            // File.Delete(sourcePath); // TODO: Delete old file
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Converter failed.  Content ID: {id}, Path: {path}", request.ContentId, fileRef.Path);
                        workOrder.Status = WorkOrderStatus.Failed;
                        await this.Api.UpdateWorkOrderAsync(workOrder);
                    }
                }
            }
            else
            {
                this.Logger.LogError("File does not exist for content. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
            }
        }
    }

    /// <summary>
    /// Add the work order with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> AddWorkOrderAsync(IndexRequestModel request, WorkOrderStatus status)
    {
        var workOrder = new API.Areas.Services.Models.WorkOrder.WorkOrderModel()
        {
            WorkType = WorkOrderType.FFmpeg,
            Status = status,
            ContentId = request.ContentId
        };
        return await this.Api.AddWorkOrderAsync(workOrder);
    }

    /// <summary>
    /// Convert file to the specified type.
    /// </summary>
    /// <param name="sourceFile">File to convert</param>
    /// <param name="fileExtension">Output file type</param>
    /// <returns>audio file name</returns>
    private async Task<string> ConvertFile(string sourceFile, string fileExtension)
    {
        var ext = fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";
        var destFile = sourceFile.Replace(Path.GetExtension(sourceFile), ext);
        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = $"Convert File";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i '{sourceFile}' -y '{destFile}' 2>&1 \"";
        // process.StartInfo.Arguments = $"-c \"ffmpeg -i '{sourceFile}' -crf 17 -strict -2 -pix_fmt yuv420p -y '{destFile}' 2>&1 \"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        var result = process.ExitCode;
        if (result != 0)
        {
            this.Logger.LogError("File conversion error. Error code: {error}, Details: {details}", result, output);
        }
        return result == 0 ? destFile : String.Empty;
    }
    #endregion
}
