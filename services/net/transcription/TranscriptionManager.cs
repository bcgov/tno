using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TNO.Services.Managers;
using TNO.Services.Transcription.Config;
using TNO.Kafka.Models;
using Confluent.Kafka;
using System.Text;
using TNO.Kafka;
using TNO.Core.Extensions;
using TNO.API.Areas.Services.Models.Content;
using TNO.Core.Exceptions;

namespace TNO.Services.Transcription;

/// <summary>
/// TranscriptionManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class TranscriptionManager : ServiceManager<TranscriptionOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, TranscriptRequest> Consumer { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TranscriptionManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="consumer"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TranscriptionManager(
        IKafkaListener<string, TranscriptRequest> consumer,
        IApiService api,
        IOptions<TranscriptionOptions> options,
        ILogger<TranscriptionManager> logger)
        : base(api, options, logger)
    {
        this.Consumer = consumer;
        this.Consumer.OnError += ConsumerErrorHandler;
        this.Consumer.OnStop += ConsumerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Listen to active topics and import content.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = _options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (this.State.Status == ServiceStatus.RequestSleep || this.State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                this.Logger.LogInformation("The service is stopping: '{Status}'", this.State.Status);
                this.State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                this.Consumer.Stop();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    var topics = _options.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (topics.Length != 0)
                    {
                        this.Consumer.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        this.Consumer.Stop();
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
            _consumer = Task.Run(ConsumerHandlerAsync, _cancelToken.Token);
        }
    }

    /// <summary>
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ConsumerHandlerAsync()
    {
        while (this.State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await this.Consumer.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Consumer.Stop();
    }

    /// <summary>
    /// The Kafka consumer has failed for some reason, need to record the failure.
    /// Fatal or unexpected errors will result in a request to stop consuming.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns>True if the consumer should retry the message.</returns>
    private ConsumerAction ConsumerErrorHandler(object sender, ErrorEventArgs e)
    {
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            return consume.Error.IsFatal ? ConsumerAction.Stop : ConsumerAction.Retry;
        }

        return _options.RetryLimit > _retries++ ? ConsumerAction.Retry : ConsumerAction.Stop;
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ConsumerStopHandler(object sender, EventArgs e)
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
    private async Task<ConsumerAction> HandleMessageAsync(ConsumeResult<string, TranscriptRequest> result)
    {
        // The service has stopped, so to should consuming messages.
        if (this.State.Status != ServiceStatus.Running)
        {
            this.Consumer.Stop();
            this.State.Stop();
            return ConsumerAction.Stop;
        }
        else
        {
            try
            {
                var content = await _api.FindContentByIdAsync(result.Message.Value.ContentId);
                if (content != null)
                {
                    // TODO: Handle multi-threading so that more than one transcription can be performed at a time.
                    await UpdateTranscriptionAsync(content);
                }
                else
                {
                    // Identify requests for transcription for content that does not exist.
                    this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, result.Message.Value.ContentId);
                }

                // Successful run clears any errors.
                this.State.ResetFailures();
                _retries = 0;
                return ConsumerAction.Proceed;
            }
            catch (HttpClientRequestException ex)
            {
                this.Logger.LogError(ex, "HTTP exception while consuming. {response}", ex.Data["body"] ?? "");
            }

            return _options.RetryLimit > _retries++ ? ConsumerAction.Retry : ConsumerAction.Stop;
        }
    }

    /// <summary>
    /// Make a request to generate a transcription for the specified 'content'.
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task UpdateTranscriptionAsync(ContentModel content)
    {
        // TODO: Handle different storage locations.
        // Remote storage locations may not be easily accessible by this service.
        var path = content.FileReferences.FirstOrDefault()?.Path;
        var safePath = Path.Join(_options.FilePath, path.MakeRelativePath());

        var mediaType = await IsVideoAsync(safePath) ? SourceMediaType.Video : SourceMediaType.Audio;
        if (mediaType == SourceMediaType.Video)
        {
            var convertFilePath = safePath.Replace(Path.GetExtension(safePath), ".mp3");
            await Video2Audio(safePath, convertFilePath);
            safePath = convertFilePath;
        }
        if (File.Exists(safePath))
        {
            this.Logger.LogInformation("Transcription requested.  Content ID: {Id}", content.Id);

            var original = content.Body;
            var fileBytes = File.ReadAllBytes(safePath);
            var transcript = await RequestTranscriptionAsync(fileBytes); // TODO: Extract language from data source.

            // Fetch content again because it may have been updated by an external source.
            // This can introduce issues if the transcript has been edited as now it will overwrite what was changed.
            var result = await _api.FindContentByIdAsync(content.Id);
            if (result != null && !String.IsNullOrWhiteSpace(transcript))
            {
                // The transcription may have been edited during this process and now those changes will be lost.
                if (String.CompareOrdinal(original, result.Body) != 0) this.Logger.LogWarning("Transcription will be overwritten.  Content ID: {Id}", content.Id);

                result.Body = transcript;
                await _api.UpdateContentAsync(result); // TODO: This can result in an editor getting a optimistic concurrency error.
                this.Logger.LogInformation("Transcription updated.  Content ID: {Id}", content.Id);
            }
            else if (String.IsNullOrWhiteSpace(transcript))
            {
                this.Logger.LogWarning("Content did not generate a transcript. Content ID: {Id}", content.Id);
            }
            else
            {
                // The content is no longer available for some reason.
                this.Logger.LogError("Content no longer exists. Content ID: {Id}", content.Id);
            }

        }
        else
        {
            if (mediaType == SourceMediaType.Video) {
                this.Logger.LogError("Video File failed to converted to audio. Content ID: {Id}, Path: {path}", content.Id, safePath);
            } else {
                this.Logger.LogError("File does not exist for content. Content ID: {Id}, Path: {path}", content.Id, safePath);
            }
        }
    }

    /// <summary>
    /// Stream the audio file to Azure and return the speech to text output.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    private async Task<string> RequestTranscriptionAsync(byte[] data, string language = "en-CA")
    {
        var sem = new Semaphore(0, 1);
        var sb = new StringBuilder();
        var config = SpeechTranslationConfig.FromSubscription(_options.AzureCognitiveServicesKey, _options.AzureRegion);
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
                this.Logger.LogDebug("Speech transcription process \"{text}...\"", result.Text?[0..Math.Min(result.Text.Length, 25)]);
            }
            // TODO: Handle other reasons.
        };

        recognizer.Canceled += (s, e) =>
        {
            if (e.Reason == CancellationReason.Error)
            {
                sb.AppendLine("*** SPEECH TRANSCRIPTION ERROR ***");
                this.Logger.LogError("Speech transcription error. {details}", e.ErrorDetails);
                this.State.RecordFailure();
            }
            sem.Release();
        };

        recognizer.SessionStopped += (s, e) =>
        {
            this.Logger.LogDebug("Speech session stopped");
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
    /// Check if the clip file contains a video stream.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    /// The same as Capture and Clip services, to be re-factored
    private async Task<bool> IsVideoAsync(string file)
    {
        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = $"Stream Type";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i {file} 2>&1 | grep Video | awk '{{print $0}}' | tr -d ,\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return !String.IsNullOrWhiteSpace(output);
    }

    /// <summary>
    /// video to audio
    /// </summary>
    /// <param name="file">video file</param>
    /// <returns></returns>
    private async Task<bool> Video2Audio(string srcFile, string destFile)
    {

        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = $"Stream Type";
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = $"-c \"ffmpeg -i {srcFile} -y {destFile} \"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        return !String.IsNullOrWhiteSpace(output);
    }
    #endregion
}
