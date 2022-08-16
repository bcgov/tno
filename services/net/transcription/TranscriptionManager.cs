using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using TNO.Services.Managers;
using TNO.Services.Transcription.Config;
using TNO.API.Areas.Services.Models.Content;
using TNO.Models.Kafka;
using Confluent.Kafka;
using TNO.API.Areas.Editor.Models.Lookup;
using System.Text.Json;
using System.Text;
using TNO.Models.Extensions;

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
    protected IKafkaListener<string, SourceContent> Consumer { get; private set; }

    /// <summary>
    /// get - Lookup values from the API.
    /// </summary>
    public string[] Topics { get; private set; } = Array.Empty<string>();

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
        IKafkaListener<string, SourceContent> kafka,
        IApiService api,
        IOptions<TranscriptionOptions> options,
        ILogger<TranscriptionManager> logger)
        : base(api, options, logger)
    {
        this.Consumer = kafka;
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
                this.Topics = Array.Empty<string>();
            }
            else if (this.State.Status != ServiceStatus.Running)
            {
                this.Logger.LogDebug("The service is not running: '{Status}'", this.State.Status);
            }
            else
            {
                try
                {
                    // Listen to every enabled data source with a topic.
                    var topics = _options.Topics.Split(',');

                    if (topics.Length != 0)
                    {
                        var tps = String.Join(',', topics);
                        this.Logger.LogInformation("Consuming topics: {tps}", tps);

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
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
    {
        if (this.State.Status != ServiceStatus.Running)
        {
            this.Consumer.Stop();
            this.State.Stop();
        }

        this.Logger.LogInformation("Updating transcription from: {Topic}, Uid: {Key}", result.Topic, result.Message.Key);

        var content = await _api.FindContentByUidAsync(result.Value.Uid, result.Value.Source);

        // Upload the file to the API.
        if (content != null && !String.IsNullOrWhiteSpace(result.Value.FilePath))
        {
            // TODO: Handle different storage locations.
            // Remote storage locations may not be easily accessible by this service.
            // TODO: This allows the captured stream to be converted to text - fix.
            var sourcePath = Path.Join(_options.FilePath, result.Value.FilePath);
            if (File.Exists(sourcePath))
            {
                var fileBytes = File.ReadAllBytes(sourcePath);
                var transcript = await GetTranscriptionAsync(fileBytes, result.Value.Language);

                content = await _api.FindContentByUidAsync(result.Value.Uid, result.Value.Source);
                content.Transcription = transcript;
                content = await _api.UpdateContentAsync(content);

                this.Logger.LogDebug("Content Updated.  Content ID: {Id}", content?.Id);
            }
            else
            {
                this.Logger.LogError("File not found.  Content ID: {Id}, File: {sourcePath}", content.Id, sourcePath);
            }
        }
        else
        {
            this.Logger.LogDebug("Content does not exist for this message. Content Source: {Source}, UID: {Uid}", content.Source, content.Uid);
        }
    }

    /// <summary>
    /// Stream the audio file to Azure and return the speech to text output.
    /// </summary>
    /// <param name="fileBytes"></param>
    /// <returns></returns>
    private async Task<string> GetTranscriptionAsync(byte[] fileBytes, string language)
    {
        var sem = new Semaphore(0, 1);
        var sb = new StringBuilder("");
        var config = SpeechTranslationConfig.FromSubscription(_options.AzureCognitiveServicesKey, _options.AzureRegion);
        config.SpeechRecognitionLanguage = language;

        // TODO: media format should be based on configuration
        var audioStreamFormat = AudioStreamFormat.GetCompressedFormat(AudioStreamContainerFormat.MP3);

        using var audioStream = PushAudioInputStream.CreatePushStream(audioStreamFormat);
        var audioConfig = AudioConfig.FromStreamInput(audioStream);
        using var recognizer = new SpeechRecognizer(config, audioConfig);

        audioStream.Write(fileBytes);

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
