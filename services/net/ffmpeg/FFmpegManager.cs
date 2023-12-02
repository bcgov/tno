using System.IO;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Models.Settings;
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
    private readonly WorkOrderStatus[] _ignoreWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled, WorkOrderStatus.Failed };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, FFmpegRequestModel> Listener { get; private set; }
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
        IKafkaListener<string, FFmpegRequestModel> listener,
        IApiService api,
        IOptions<FFmpegOptions> options,
        ILogger<FFmpegManager> logger)
            : base(api, options, logger)
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
    private async Task HandleMessageAsync(ConsumeResult<string, FFmpegRequestModel> result)
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
    private async Task UpdateAVAsync(FFmpegRequestModel request, ContentModel content)
    {
        // TODO: Handle different storage locations.
        // Remote storage locations may not be easily accessible by this service.
        var fileRef = content.FileReferences.FirstOrDefault();
        if (fileRef != null)
        {
            var sourcePath = Path.Join(this.Options.VolumePath, fileRef.Path.MakeRelativePath());
            if (File.Exists(sourcePath))
            {
                var sourceExt = Path.GetExtension(sourcePath);
                await UpdateWorkOrderAsync(request, WorkOrderStatus.InProgress);
                var completed = true;
                foreach (var action in request.Actions)
                {
                    if (action.Action == FFmpegAction.Convert)
                    {
                        var convertFrom = action.Arguments.ContainsKey("from") ? action.Arguments["from"] : "";
                        var convertTo = action.Arguments.ContainsKey("to") ? action.Arguments["to"] : "";
                        var contentType = action.Arguments.ContainsKey("contentType") ? action.Arguments["contentType"] : MimeTypeMap.GetMimeType(convertTo.Replace(".", ""));
                        if (convertFrom.Equals(sourceExt, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!String.IsNullOrWhiteSpace(convertTo))
                            {
                                this.Logger.LogDebug("Converting file. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
                                var newFile = await ConvertFile(sourcePath, convertTo);
                                if (!String.IsNullOrEmpty(newFile))
                                {
                                    fileRef.Path = fileRef.Path.Replace(convertFrom, convertTo);
                                    fileRef.FileName = Path.GetFileName(fileRef.Path);
                                    fileRef.ContentType = contentType;
                                }
                                else
                                {
                                    completed = false;
                                    break;
                                }
                            }
                            else
                            {
                                this.Logger.LogError("Convert to extension required. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
                                completed = false;
                                break;
                            }
                        }
                        else
                        {
                            this.Logger.LogDebug("Convert from extension action did not match content. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
                        }
                    }
                }
                if (completed)
                {
                    this.Logger.LogInformation("Content has been processed.  Content ID: {id}, Path: {path}", request.ContentId, fileRef.Path);
                    await this.Api.UpdateFileAsync(content, true, request.RequestorId);
                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
                    // File.Delete(sourcePath);
                }
                else
                {
                    await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
                }
            }
            else
            {
                this.Logger.LogError("File does not exist for content. Content ID: {Id}, Path: {path}", request.ContentId, sourcePath);
                await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
            }
        }
        else
        {
            this.Logger.LogWarning("File does not included in model for content. Content ID: {Id}", request.ContentId);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed);
        }
    }

    /// <summary>
    /// Update the work order (if it exists) with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task UpdateWorkOrderAsync(FFmpegRequestModel request, WorkOrderStatus status)
    {
        if (request.WorkOrderId > 0)
        {
            var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
            if (workOrder != null && !_ignoreWorkOrders.Contains(workOrder.Status))
            {
                workOrder.Status = status;
                await this.Api.UpdateWorkOrderAsync(workOrder);
            }
        }
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
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        var result = process.ExitCode;
        if (result != 0)
        {
            this.Logger.LogError("File conversion error. Error code: {errorcode}, Details: {details}", result, output);
        }
        return result == 0 ? destFile : String.Empty;
    }
    #endregion
}
