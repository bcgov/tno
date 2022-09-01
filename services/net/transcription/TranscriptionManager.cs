using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TNO.Services.Managers;
using TNO.Services.Transcription.Config;
using TNO.Models.Kafka;
using Confluent.Kafka;
using System.Text;
using TNO.Kafka;
using TNO.Core.Extensions;
using TNO.API.Areas.Services.Models.Content;

namespace TNO.Services.Transcription;

/// <summary>
/// TranscriptionManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class TranscriptionManager : ServiceManager<TranscriptionOptions>
{
    #region Variables
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
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
    /// <param name="kafka"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public TranscriptionManager(
        IKafkaListener<string, TranscriptRequest> kafka,
        IApiService api,
        IOptions<TranscriptionOptions> options,
        ILogger<TranscriptionManager> logger)
        : base(api, options, logger)
    {
        this.Consumer = kafka;
        this.Consumer.OnError += ConsumerErrorHandler;
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
            if (this.State.Status != ServiceStatus.Running)
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
                        this.Logger.LogInformation("Consuming topics: {tps}", _options.Topics);

                        if (!this.Consumer.IsReady) this.Consumer.Open();

                        // TODO: Not sure if the consumer should stop before changing its subscription.
                        this.Consumer.Subscribe(topics);

                        // Create a new thread if the prior one isn't running anymore.
                        if (_consumer == null || _notRunning.Contains(_consumer.Status))
                        {
                            _consumer = Task.Factory.StartNew(() => ConsumerHandler());
                        }

                        // Successful run clears any errors.
                        this.State.ResetFailures();
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
    /// Keep consuming messages from Kafka until the service stops running.
    /// </summary>
    /// <returns></returns>
    private async Task ConsumerHandler()
    {
        while (this.State.Status == ServiceStatus.Running && this.Consumer.IsReady)
        {
            await this.Consumer.ConsumeAsync(HandleMessageAsync);
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
    private bool ConsumerErrorHandler(object sender, ErrorEventArgs e)
    {
        this.State.RecordFailure();
        if (e.GetException() is ConsumeException ex)
        {
            return ex.Error.IsFatal;
        }

        // Inform the consumer it should stop.
        return this.State.Status != ServiceStatus.Running;
    }

    /// <summary>
    /// Retrieve a file from storage and send to Microsoft Cognitive Services. Obtain
    /// the transcription and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, TranscriptRequest> result)
    {
        // The service has stopped, so to should consuming messages.
        if (this.State.Status != ServiceStatus.Running)
        {
            this.Consumer.Stop();
            this.State.Stop();
        }

        var content = await _api.FindContentByIdAsync(result.Message.Value.ContentId);
        if (content != null)
        {
            await UpdateTranscriptionAsync(content);
        }
        else
        {
            // Identify requests for transcription for content that does not exist.
            this.Logger.LogWarning("Content does not exist for this message. Key: {Key}, Content ID: {ContentId}", result.Message.Key, result.Message.Value.ContentId);
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
        if (File.Exists(safePath))
        {
            this.Logger.LogInformation("Transcription requested.  Content ID: {Id}", content.Id);

            var original = content.Transcription;
            var fileBytes = File.ReadAllBytes(safePath);
            var transcript = await RequestTranscriptionAsync(fileBytes); // TODO: Extract language from data source.

            // Fetch content again because it may have been updated by an external source.
            // This can introduce issues if the transcript has been edited as now it will overwrite what was changed.
            var result = await _api.FindContentByIdAsync(content.Id);
            if (result != null && !String.IsNullOrWhiteSpace(transcript))
            {
                // The transcription may have been edited during this process and now those changes will be lost.
                if (original != result.Transcription) this.Logger.LogWarning("Transcription will be overwritten.  Content ID: {Id}", content.Id);

                result.Transcription = transcript;
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
            this.Logger.LogError("File does not exist for content. Content ID: {Id}, Path: {path}", content.Id, path);
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
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            sem.Release();
        };

        recognizer.SessionStopped += (s, e) =>
        {
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
    #endregion
}
