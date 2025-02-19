using System.Text;
using System.Text.RegularExpressions;
using Confluent.Kafka;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Core.Storage;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Managers;
using TNO.Services.Transcription.Config;
using TNO.Services.Transcription.Exceptions;
namespace TNO.Services.Transcription;

/// <summary>
/// TranscriptionManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class TranscriptionManager : ServiceManager<TranscriptionOptions>
{
    #region Variables
    private readonly IS3StorageService _s3StorageService;
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly WorkOrderStatus[] _ignoreWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Completed };
    private int _retries = 0;

    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, TranscriptRequestModel> Listener { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TranscriptionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="s3StorageService"></param>
    public TranscriptionManager(
        IKafkaListener<string, TranscriptRequestModel> listener,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<TranscriptionOptions> options,
        ILogger<TranscriptionManager> logger,
        IS3StorageService s3StorageService)
        : base(api, chesService, chesOptions, options, logger)
    {
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
        _s3StorageService = s3StorageService;
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
                    await this.SendErrorEmailAsync("Service had an Unexpected Failure", ex);
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
    private async Task HandleMessageAsync(ConsumeResult<string, TranscriptRequestModel> result)
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
                    // If the content was published before the specified offset, ignore it.
                    if (!string.IsNullOrEmpty(Options.OldTnoContentTagName)
                        && content.Tags.Any(x => x.Code.ToUpperInvariant() == Options.OldTnoContentTagName.ToUpperInvariant()))
                    {
                        this.Logger.LogWarning($"The content has been ignored. It was tagged as old TNO content. Key: {result.Message.Key}, Content ID: {request.ContentId}");
                        return;
                    }

                    // TODO: Handle multi-threading so that more than one transcription can be performed at a time.
                    await UpdateTranscriptionAsync(request, content);
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
                await this.SendErrorEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
                await this.SendErrorEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
    }

    /// <summary>
    /// Get local temp directory
    /// </summary>
    /// <returns></returns>
    private string GetTempDirectory()
    {
        var tempPath = Path.Join(this.Options.VolumePath, "temp".MakeRelativePath());
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
        }
        return tempPath;
    }

    /// <summary>
    /// Clean up temp files that are downloaded from s3 or generated from downloaded s3 file
    /// </summary>
    /// <param name="files"></param>
    private void CleanupS3Files(params string[] files)
    {
        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }

    /// <summary>
    /// Download S3 files
    /// </summary>
    /// <param name="s3Path"></param>
    /// <returns></returns>
    private async Task<string> DownloadS3File(string? s3Path)
    {
        if (!string.IsNullOrEmpty(s3Path))
        {
            var tempDir = GetTempDirectory();
            var s3FileStream = await _s3StorageService.DownloadFromS3Async(s3Path);
            if (s3FileStream != null)
            {
                var fileName = Path.GetFileName(s3Path);
                var tmpFilePath = Path.Combine(tempDir, fileName);
                if (File.Exists(tmpFilePath))
                {
                    File.Delete(tmpFilePath);
                }

                using (var fileStream = new FileStream(tmpFilePath, FileMode.Create, FileAccess.Write))
                {
                    s3FileStream.CopyTo(fileStream);
                    this.Logger.LogDebug($"S3 file {s3Path} is downloaded to: {tmpFilePath}");
                    return tmpFilePath;
                }
            }
            else
            {
                this.Logger.LogError($"Cannot download file {s3Path} from S3");
            }
        }
        else
        {
            this.Logger.LogError("S3 file path is empty.");
        }
        return string.Empty;
    }

    /// <summary>
    /// Make a request to generate a transcription for the specified 'content'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private async Task UpdateTranscriptionAsync(TranscriptRequestModel request, ContentModel content)
    {
        var requestContentId = request.ContentId;
        this.Logger.LogInformation("Transcription requested.  Content ID: {Id}", requestContentId);

        // TODO: Handle different storage locations.
        // Remote storage locations may not be easily accessible by this service.
        var contentFile = content.FileReferences.FirstOrDefault();
        var path = contentFile?.Path;
        var safePath = Path.Join(this.Options.VolumePath, path.MakeRelativePath());
        var isSyncedToS3 = contentFile?.IsSyncedToS3;
        var downloadedFile = string.Empty;
        if (isSyncedToS3 == true)
        {
            safePath = contentFile?.S3Path;
            if (!string.IsNullOrEmpty(contentFile?.S3Path))
            {
                downloadedFile = await DownloadS3File(contentFile?.S3Path);
                if (!string.IsNullOrEmpty(downloadedFile))
                {
                    safePath = downloadedFile;
                }
            }
        }

        if (File.Exists(safePath))
        {
            var destFile = safePath.Replace(Path.GetExtension(safePath), ".mp3");
            // convert to audio if it's video file
            var ext = Path.GetExtension(safePath)[1..].ToLower();
            if (this.Options.ConvertToAudio.Contains(ext))
            {
                safePath = await Video2Audio(safePath, destFile);
            }

            if (!String.IsNullOrEmpty(safePath))
            {
                this.Logger.LogInformation("Validating work order.  Content ID: {Id}, Path: {file}", requestContentId, safePath);
                var hasWorkOrder = await UpdateWorkOrderAsync(request, WorkOrderStatus.InProgress);

                if (hasWorkOrder)
                {
                    var original = content.Body;
                    var fileBytes = File.ReadAllBytes(safePath);
                    var transcript = await RequestTranscriptionAsync(requestContentId, fileBytes); // TODO: Extract language from data source.

                    // Fetch content again because it may have been updated by an external source.
                    // This can introduce issues if the transcript has been edited as now it will overwrite what was changed.
                    content = (await this.Api.FindContentByIdAsync(requestContentId))!;
                    if (content != null && !String.IsNullOrWhiteSpace(transcript))
                    {
                        // The transcription may have been edited during this process and now those changes will be lost.
                        if (String.CompareOrdinal(original, content.Body) != 0) this.Logger.LogWarning("Transcription will be overwritten.  Content ID: {Id}", requestContentId);

                        content.Body = GetFormattedTranscript(transcript);
                        await this.Api.UpdateContentAsync(content); // TODO: This can result in an editor getting a optimistic concurrency error.
                        this.Logger.LogInformation("Transcription updated.  Content ID: {Id}", requestContentId);

                        await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
                    }
                    else if (String.IsNullOrWhiteSpace(transcript))
                    {
                        this.Logger.LogWarning("Content did not generate a transcript. Content ID: {Id}", requestContentId);
                        var emptyTranscriptException = new EmptyTranscriptException(requestContentId);
                        await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, emptyTranscriptException);
                    }
                    else
                    {
                        // The content is no longer available for some reason.
                        this.Logger.LogError("Content no longer exists. Content ID: {Id}", requestContentId);
                        var contentNotFoundException = new ContentNotFoundException(requestContentId);
                        await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, contentNotFoundException);
                    }
                }
                else
                {
                    this.Logger.LogWarning("Request ignored because it does not have a work order.  Content ID: {id}, File: {path}", requestContentId, safePath);
                }

                if (isSyncedToS3 == true)
                {
                    CleanupS3Files(new string[] { downloadedFile, destFile });
                }
            }
            else
            {
                this.Logger.LogError("Transcription failed, path to file missing.  Content ID: {Id}, File: {path}", requestContentId, safePath);
            }
        }
        else
        {
            this.Logger.LogError($"File does not exist for content. Content ID: {requestContentId}, Path: {safePath}");
            var workOrderFailedException = new FileMissingException(requestContentId, safePath ?? "");
            await this.SendNoticeEmailAsync($"File missing for Content ID: {requestContentId}", workOrderFailedException);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, workOrderFailedException);
        }
    }

    /// <summary>
    /// Format the transcript to include newlines.
    /// </summary>
    /// <param name="transcript"></param>
    /// <returns></returns>
    private static string GetFormattedTranscript(string transcript)
    {
        var result = transcript;
        if (!string.IsNullOrWhiteSpace(result))
        {
            var pattern = @"\.[A-Z0-9]";
            var matches = Regex.Matches(result, pattern);
            foreach (var match in matches.Cast<Match>())
            {
                result = result.Replace(match.Value, match.Value[0] + Environment.NewLine + match.Value[1]);
            }
        }
        return result;
    }

    /// <summary>
    /// Update the work order (if it exists) with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task<bool> UpdateWorkOrderAsync(TranscriptRequestModel request, WorkOrderStatus status, Exception? reason = null)
    {
        if (request.WorkOrderId > 0)
        {
            var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
            if (workOrder != null && !_ignoreWorkOrders.Contains(workOrder.Status))
            {
                workOrder.Status = status;
                await this.Api.UpdateWorkOrderAsync(workOrder);

                if (status == WorkOrderStatus.Failed && reason != null)
                {
                    await this.SendErrorEmailAsync($"Work order failed for Content ID: {request.ContentId}", reason);
                    this.Logger.LogError(reason, "Work order failed for Content ID: {ContentId}", request.ContentId);
                }

                return true;
            }
        }
        return !this.Options.AcceptOnlyWorkOrders;
    }

    /// <summary>
    /// Stream the audio file to Azure and return the speech to text output.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private async Task<string> RequestTranscriptionAsync(long id, byte[] data, string language = "en-CA")
    {
        var sem = new Semaphore(0, 1);
        var sb = new StringBuilder();
        var config = SpeechTranslationConfig.FromSubscription(this.Options.AzureCognitiveServicesKey, this.Options.AzureRegion);
        config.SpeechRecognitionLanguage = language;

        // TODO: media format should be based on configuration
        var audioStreamFormat = AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.MP3);

        using var audioStream = PushAudioInputStream.CreatePushStream(audioStreamFormat);
        var audioConfig = AudioConfig.FromStreamInput(audioStream);
        using var recognizer = new SpeechRecognizer(config, audioConfig);

        audioStream.Write(data);

        recognizer.Recognized += (s, e) =>
        {
            var result = e.Result;
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                sb.Append(result.Text);
                this.Logger.LogDebug("Speech transcription process. Content: {id}, \"{text}...\"", id, result.Text?[0..Math.Min(result.Text.Length, 25)]);
            }
            // TODO: Handle other reasons.
        };

        recognizer.Canceled += (s, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                sb.AppendLine("*** SPEECH TRANSCRIPTION ERROR ***");
                this.Logger.LogError("Speech transcription error. Content: {id}, {details}", id, e.ErrorDetails);
                this.State.RecordFailure();
            }
            sem.Release();
        };

        recognizer.SessionStopped += (s, e) =>
        {
            this.Logger.LogDebug("Speech session stopped. Content: {id}", id);
            sem.Release();
        };

        // Starts continuous recognition.
        // Uses StopContinuousRecognitionAsync() to stop recognition.
        await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
        audioStream.Close();
        sem.WaitOne();

        // Stops recognition.
        await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);

        return sb.ToString();
    }

    /// <summary>
    /// video to audio
    /// </summary>
    /// <param name="srcFile">source file path</param>
    /// <param name="destFile">destination file path</param>
    /// <returns>destination file name</returns>
    private async Task<string> Video2Audio(string srcFile, string destFile)
    {
        this.Logger.LogInformation("Converting file.  File: {file}", srcFile);
        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = $"Convert File";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i '{srcFile}' -y '{destFile}' 2>&1 \"";
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
        else
        {
            this.Logger.LogDebug("File conversion details: {output}", output);
        }
        return result == 0 ? destFile : string.Empty;
    }
    #endregion
}
