using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.Managers;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.NLP.ExtractQuotes;
using TNO.API.Areas.Services.Models.Minister;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using System.Text;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace TNO.Services.ExtractQuotes;

/// <summary>
/// ExtractQuotesManager class, provides a Kafka Consumer service which extracts quotes from content from all active topics.
/// </summary>
public class ExtractQuotesManager : ServiceManager<ExtractQuotesOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer.
    /// </summary>
    protected IKafkaListener<string, IndexRequestModel> Listener { get; }

    /// <summary>
    /// get - Core NLP Service wrapper.
    /// </summary>
    protected ICoreNLPService CoreNLPService { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ExtractQuotesManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="user"></param>
    /// <param name="reportEngine"></param>
    /// <param name="coreNLPService"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="serializationOptions"></param>
    /// <param name="extractQuotesOptions"></param>
    /// <param name="logger"></param>
    public ExtractQuotesManager(
        IKafkaListener<string, IndexRequestModel> listener,
        IApiService api,
        ICoreNLPService coreNLPService,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<ExtractQuotesOptions> extractQuotesOptions,
        ILogger<ExtractQuotesManager> logger)
        : base(api, chesService, chesOptions, extractQuotesOptions, logger)
    {
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;

        this.CoreNLPService = coreNLPService;
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
                    await this.SendEmailAsync("Service had an Unexpected Failure", ex);
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
    /// the report and update the content record accordingly.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task HandleMessageAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        try
        {
            // The service has stopped, so to should consuming messages.
            if (this.State.Status != ServiceStatus.Running)
            {
                this.Listener.Stop();
                this.State.Stop();
            }
            else
            {
                await ProcessIndexRequestAsync(result);

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
                await this.SendEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                this.Logger.LogError(ex, "Failed to handle message");
                await this.SendEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));
        }
        finally
        {
            if (State.Status == ServiceStatus.Running) Listener.Resume();
        }
    }

    /// <summary>
    /// Process the report request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessIndexRequestAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        this.Logger.LogDebug($"ProcessIndexRequestAsync:BEGIN:{result.Message.Key}");

        var model = result.Message.Value;

        if ((model.Action == IndexAction.Index && this.Options.ExtractQuotesOnIndex) || 
            (model.Action == IndexAction.Publish && this.Options.ExtractQuotesOnPublish))
        {
            this.Logger.LogInformation("Extracting Quotes from Topic: {Topic}, Action: {Action}, Content ID: {Key}", result.Topic, model.Action, result.Message.Key);
            // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
            var content = await this.Api.FindContentByIdAsync(result.Message.Value.ContentId);
            var ministers = await this.Api.GetMinistersAsync();
            if (content != null) // need to check here if this content uses a transcript and if it does, skip if transcript is not approved
            {
                var text = new StringBuilder();
                // Only use Summary if Body is empty
                if (String.IsNullOrWhiteSpace(content.Body) && !String.IsNullOrWhiteSpace(content.Summary)) {
                    var html = new HtmlDocument();
                    html.LoadHtml(content.Summary);
                    foreach (HtmlNode node in html.DocumentNode.SelectNodes("//text()"))
                    {
                        text.AppendLine(node.InnerText);
                    }
                }

                if (!String.IsNullOrWhiteSpace(content.Body)) {
                    var html = new HtmlDocument();
                    html.LoadHtml(content.Body);
                    foreach (HtmlNode node in html.DocumentNode.SelectNodes("//text()"))
                    {
                        text.AppendLine(node.InnerText);
                    }
                }
                var annotationInput = text.ToString();
                if (annotationInput.Length == 0) {
                    this.Logger.LogInformation("Content ID: {Key} has no text to extract quotes from.", result.Message.Key);
                    return;
                }

                try {
                    var annotations = await CoreNLPService.PerformAnnotation(annotationInput);
                    if (annotations != null && annotations.Quotes.Any()) {
                        this.Logger.LogInformation("Extracted [{quoteCount}] Quotes from Content ID: {Key}", annotations.Quotes.Count, result.Message.Key);

                        var speakersAndQuotes = ExtractSpeakersAndQuotes(ministers, annotations);

                        List<API.Areas.Services.Models.Content.QuoteModel> quotesToAdd = new List<API.Areas.Services.Models.Content.QuoteModel>();
                        foreach(var speaker in speakersAndQuotes.Keys) {
                            foreach(var quote in speakersAndQuotes[speaker]) {
                                // only add quotes which dont match any previously captured
                                if (!content.Quotes.Any((q) => q.Byline.Equals(speaker) && q.Statement.Equals(quote)))
                                    quotesToAdd.Add(new API.Areas.Services.Models.Content.QuoteModel() {Id = 0, ContentId = content.Id, Byline = speaker, Statement = quote});
                            }
                        }
                        content = await this.Api.AddQuotesToContentAsync(content.Id, quotesToAdd);
                    } else {
                        this.Logger.LogInformation("Extracted [{quoteCount}] Quotes from Content ID: {Key}", 0, result.Message.Key);
                    }
                } catch (Exception) {
                    this.Logger.LogError("Quote Extraction failed for Content ID: {Key}", result.Message.Key);
                    throw;
                }
            }
            else
            {
                this.Logger.LogWarning("Content does not exists. Content ID: {ContentId}", result.Message.Value.ContentId);
            }
        } else {
            this.Logger.LogInformation("SKIPPED: Extracting Quotes from Topic: {Topic}, Action: {Action}, Content ID: {Key}", result.Topic, model.Action, result.Message.Key);
        }
        this.Logger.LogDebug($"ProcessIndexRequestAsync:END:{result.Message.Key}");
    }

    private Dictionary<string, List<string>> ExtractSpeakersAndQuotes(IEnumerable<MinisterModel> ministers, AnnotationResponse annotations)
    {
        Dictionary<string, List<string>> speakersAndQuotes = new Dictionary<string, List<string>>();

        foreach(var quote in annotations.Quotes) {
            var canonicalSpeaker = quote.canonicalSpeaker;
            // if nlp isnt willing to guess a speaker, see if we can find a PERSON mentioned in the same sentence as the Quote
            if (canonicalSpeaker.Equals("Unknown")) {
                var speakerInSameSentenceAsQuote = annotations.Sentences[quote.beginSentence].entityMentions.FirstOrDefault((m) => m.ner.Equals("PERSON"));
                if (speakerInSameSentenceAsQuote != null) canonicalSpeaker = speakerInSameSentenceAsQuote.text;
            }
            
            var quotedMinister = ministers.FirstOrDefault((m) => m.Name.Contains(canonicalSpeaker));
            if (quotedMinister != null) canonicalSpeaker = quotedMinister.Name;

            if (!speakersAndQuotes.ContainsKey(canonicalSpeaker)) speakersAndQuotes.Add(canonicalSpeaker, new List<string>());

            var cleanedQuoteText = quote.text;
            cleanedQuoteText = Regex.Replace(cleanedQuoteText, "^\"", string.Empty); // remove quote from start
            cleanedQuoteText = Regex.Replace(cleanedQuoteText, "\"$", string.Empty); // remove quote from end

            speakersAndQuotes[canonicalSpeaker].Add(cleanedQuoteText);
        }

        return speakersAndQuotes;
    }

    #endregion
}
