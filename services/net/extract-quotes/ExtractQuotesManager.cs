using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using HtmlAgilityPack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.Minister;
using TNO.Ches;
using TNO.Ches.Configuration;
using TNO.Core.Exceptions;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Services.ExtractQuotes.Config;
using TNO.Services.ExtractQuotes.CoreNLP.models;
using TNO.Services.Managers;
using TNO.Services.NLP.ExtractQuotes;

namespace TNO.Services.ExtractQuotes;



/// <summary>
/// ExtractQuotesManager class, provides a Kafka Consumer service which extracts quotes from content from all active topics.
/// </summary>
public partial class ExtractQuotesManager : ServiceManager<ExtractQuotesOptions>
{
    #region Variables
    [GeneratedRegex("^\"")]
    private static partial Regex MyRegex();
    [GeneratedRegex("\"$")]
    private static partial Regex MyRegex1();

    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;

    // Set to track processed content IDs to prevent duplicate processing
    private readonly HashSet<long> _processedContentIds = new HashSet<long>();
    // Maximum size for the processed content IDs set to prevent unlimited growth
    private const int MaxProcessedContentIdsCount = 10000;

    // Add memory cache
    private readonly IMemoryCache _memoryCache;
    // Define cache key
    private const string MinistersCacheKey = "Ministers_List";
    // Define cache expiration time (days)
    private const int MinistersCacheExpirationDays = 1;
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
        IMemoryCache memoryCache,
        ILogger<ExtractQuotesManager> logger)
        : base(api, chesService, chesOptions, extractQuotesOptions, logger)
    {
        Listener = listener;
        Listener.IsLongRunningJob = true;
        Listener.OnError += ListenerErrorHandler;
        Listener.OnStop += ListenerStopHandler;

        CoreNLPService = coreNLPService;
        _memoryCache = memoryCache;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Clean up resources when the service is stopping
    /// </summary>
    private void CleanupResources()
    {
        Logger.LogInformation("Cleaning up quote extraction service resources...");

        // Clear processed content IDs
        lock (_processedContentIds)
        {
            _processedContentIds.Clear();
            Logger.LogInformation("Cleared processed content IDs tracking set");
        }
    }

    /// <summary>
    /// Listen to active topics and import content.
    /// </summary>
    /// <returns></returns>
    public override async Task RunAsync()
    {
        var delay = Options.DefaultDelayMS;

        // Always keep looping until an unexpected failure occurs.
        while (true)
        {
            if (State.Status == ServiceStatus.RequestSleep || State.Status == ServiceStatus.RequestPause)
            {
                // An API request or failures have requested the service to stop.
                Logger.LogInformation("The service is stopping: '{Status}'", State.Status);
                State.Stop();

                // The service is stopping or has stopped, consume should stop too.
                Listener.Stop();
            }
            else if (State.Status != ServiceStatus.Running)
            {
                Logger.LogDebug("The service is not running: '{Status}'", State.Status);
            }
            else
            {
                try
                {
                    var topics = Options.Topics.Split(',', StringSplitOptions.RemoveEmptyEntries);

                    if (topics.Length != 0)
                    {
                        Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else if (topics.Length == 0)
                    {
                        Listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Service had an unexpected failure.");
                    State.RecordFailure();
                    await SendErrorEmailAsync("Service had an Unexpected Failure", ex);
                }
            }

            // The delay ensures we don't have a run away thread.
            Logger.LogDebug("Service sleeping for {delay} ms", delay);
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
        while (State.Status == ServiceStatus.Running &&
            _cancelToken?.IsCancellationRequested == false)
        {
            await Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        Listener.Stop();
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
            State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
                Listener.Stop();
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

        // No batch processing to clean up

        // Clean up resources
        CleanupResources();
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
            if (State.Status != ServiceStatus.Running)
            {
                Listener.Stop();
                State.Stop();
            }
            else
            {
                // Process the message without timeout
                await ProcessIndexRequestAsync(result);

                // Inform Kafka this message is completed.
                Listener.Commit(result);

                // Successful run clears any errors.
                State.ResetFailures();
                _retries = 0;
            }
        }
        catch (Exception ex)
        {
            if (ex is HttpClientRequestException httpEx)
            {
                Logger.LogError(ex, "HTTP exception - Topic: {topic}, Content ID: {key}, Response: {response}",
                    result.Topic, result.Message.Key, httpEx.Data["body"] ?? "");
                await SendErrorEmailAsync("HTTP exception while consuming. {response}", ex);
            }
            else
            {
                Logger.LogError(ex, "Message processing failed - Topic: {topic}, Content ID: {key}",
                    result.Topic, result.Message.Key);
                await SendErrorEmailAsync("Failed to handle message", ex);
            }
            ListenerErrorHandler(this, new ErrorEventArgs(ex));

            // Still commit the message to avoid getting stuck on it
            Listener.Commit(result);
        }
        finally
        {
            // Always resume consumption in the finally block
            if (State.Status == ServiceStatus.Running)
            {
                Listener.Resume();
            }
        }
    }

    /// <summary>
    /// Process the report request.
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private async Task ProcessIndexRequestAsync(ConsumeResult<string, IndexRequestModel> result)
    {
        var model = result.Message.Value;

        // Log detailed decision information
        Logger.LogInformation("Received message - Topic: {topic}, Action: {action}, Content ID: {key}, Extract on Index: {onIndex}, Extract on Publish: {onPublish}",
            result.Topic, model.Action, result.Message.Key, Options.ExtractQuotesOnIndex, Options.ExtractQuotesOnPublish);

        if (model.Action == IndexAction.Index && Options.ExtractQuotesOnIndex ||
            model.Action == IndexAction.Publish && Options.ExtractQuotesOnPublish)
        {
            Logger.LogInformation("Starting quote extraction from topic: {topic}, Action: {action}, Content ID: {key}", result.Topic, model.Action, result.Message.Key);
            // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
            var content = await Api.FindContentByIdAsync(result.Message.Value.ContentId);

            if (content != null) // need to check here if this content uses a transcript and if it does, skip if transcript is not approved
            {
                // If the content was published before the specified offset, ignore it.
                if (Options.IgnoreContentPublishedBeforeOffset.HasValue
                    && Options.IgnoreContentPublishedBeforeOffset > 0
                    && content.PublishedOn.HasValue
                    && content.PublishedOn.Value < DateTime.UtcNow.AddDays(-1 * Options.IgnoreContentPublishedBeforeOffset.Value))
                    return;

                // Get ministers list from cache, if not exists, fetch from API and cache
                if (!_memoryCache.TryGetValue(MinistersCacheKey, out IEnumerable<MinisterModel>? ministers))
                {
                    Logger.LogInformation("Ministers list not found in cache. Fetching from API.");
                    ministers = await Api.GetMinistersAsync();

                    // Cache the ministers list for 1 day
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromDays(MinistersCacheExpirationDays));

                    _memoryCache.Set(MinistersCacheKey, ministers, cacheOptions);
                    Logger.LogInformation("Ministers list cached for {days} days. Count: {count}",
                        MinistersCacheExpirationDays, ministers?.Count() ?? 0);
                }
                else
                {
                    Logger.LogDebug("Using cached ministers list. Count: {count}", ministers?.Count() ?? 0);
                }

                // Ensure ministers is not null before using it
                ministers ??= Enumerable.Empty<MinisterModel>();
                await ProcessContentItemAsync(content, ministers, result);
            }
            else
            {
                Logger.LogWarning("Content does not exists. Content ID: {ContentId}", result.Message.Value.ContentId);
            }
        }
        else
        {
            // Log detailed reason for skipping
            string skipReason = model.Action == IndexAction.Index ?
                $"Skipped - Quote extraction for Index operation is disabled (ExtractQuotesOnIndex={Options.ExtractQuotesOnIndex})" :
                $"Skipped - Quote extraction for Publish operation is disabled (ExtractQuotesOnPublish={Options.ExtractQuotesOnPublish})";

            Logger.LogInformation("{skipReason} - Topic: {topic}, Action: {action}, Content ID: {key}",
                skipReason, result.Topic, model.Action, result.Message.Key);
        }
    }

    private Dictionary<string, List<string>> ExtractSpeakersAndQuotes(IEnumerable<MinisterModel> ministers, AnnotationResponse annotations)
    {
        var speakersAndQuotes = new Dictionary<string, List<string>>();

        foreach (var quote in annotations.Quotes)
        {
            var canonicalSpeaker = quote.canonicalSpeaker;
            // if nlp isn't willing to guess a speaker, see if we can find a PERSON mentioned in the same sentence as the Quote
            if (canonicalSpeaker.Equals("Unknown"))
            {
                var speakerInSameSentenceAsQuote = annotations.Sentences[quote.beginSentence].entityMentions.FirstOrDefault((m) => m.ner.Equals("PERSON"));
                if (speakerInSameSentenceAsQuote != null) canonicalSpeaker = speakerInSameSentenceAsQuote.text;
            }

            var quotedMinister = ministers.FirstOrDefault((m) => m.Name.Contains(canonicalSpeaker));
            if (quotedMinister != null) canonicalSpeaker = quotedMinister.Name;

            if (!speakersAndQuotes.ContainsKey(canonicalSpeaker)) speakersAndQuotes.Add(canonicalSpeaker, new List<string>());

            var cleanedQuoteText = quote.text;
            cleanedQuoteText = MyRegex().Replace(cleanedQuoteText, string.Empty); // remove quote from start
            cleanedQuoteText = MyRegex1().Replace(cleanedQuoteText, string.Empty); // remove quote from end

            speakersAndQuotes[canonicalSpeaker].Add(cleanedQuoteText);
        }

        return speakersAndQuotes;
    }



    /// <summary>
    /// Process a single content item for quote extraction
    /// </summary>
    private async Task ProcessContentItemAsync(ContentModel content, IEnumerable<MinisterModel> ministers, ConsumeResult<string, IndexRequestModel> result)
    {
        var contentId = content.Id;

        // Check if this content has already been processed
        lock (_processedContentIds)
        {
            if (_processedContentIds.Contains(contentId))
            {
                Logger.LogWarning("Content ID {contentId} has already been processed - skipping duplicate processing", contentId);
                return;
            }

            // Check if the set has reached the maximum size
            if (_processedContentIds.Count >= MaxProcessedContentIdsCount)
            {
                // Clear the set when it reaches the maximum size
                _processedContentIds.Clear();
                Logger.LogInformation("Cleared processed content IDs tracking set due to size limit ({maxSize})", MaxProcessedContentIdsCount);
            }

            // Mark as processed
            _processedContentIds.Add(contentId);
        }

        Logger.LogInformation("Starting to process content ID: {contentId} - Using {serviceType} service",
            contentId,
            Options.UseLLM ? "LLM" : "CoreNLP");

        var text = new StringBuilder();
        // Only use Summary if Body is empty
        if (string.IsNullOrWhiteSpace(content.Body) && !string.IsNullOrWhiteSpace(content.Summary))
        {
            var html = new HtmlDocument();
            html.LoadHtml(content.Summary);
            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//text()"))
            {
                text.AppendLine(node.InnerText);
            }
        }

        if (!string.IsNullOrWhiteSpace(content.Body))
        {
            var html = new HtmlDocument();
            html.LoadHtml(content.Body);
            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//text()"))
            {
                text.AppendLine(node.InnerText);
            }
        }

        var annotationInput = text.ToString();
        if (annotationInput.Length == 0)
        {
            Logger.LogInformation("Content ID: {contentId} has no text for quote extraction", content.Id);
            return;
        }

        try
        {
            Logger.LogInformation("Starting quote extraction using {serviceType} service - Content ID: {contentId}, Text length: {length} characters",
                Options.UseLLM ? "LLM" : "CoreNLP", content.Id, annotationInput.Length);

            var annotations = await CoreNLPService.PerformAnnotation(annotationInput);
            if (annotations != null && annotations.Quotes.Count > 0)
            {
                Logger.LogInformation("Extracted {quoteCount} quotes from content ID: {contentId}", content.Id, annotations.Quotes.Count);

                var speakersAndQuotes = ExtractSpeakersAndQuotes(ministers, annotations);

                var quotesToAdd = new List<QuoteModel>();
                foreach (var speaker in speakersAndQuotes.Keys)
                {
                    foreach (var quote in speakersAndQuotes[speaker])
                    {
                        // only add quotes which don't match any previously captured
                        if (!content.Quotes.Any((q) => q.Byline.Equals(speaker) && q.Statement.Equals(quote)))
                        {
                            quotesToAdd.Add(new QuoteModel { Id = 0, ContentId = content.Id, Byline = speaker, Statement = quote });
                        }
                    }
                }

                if (quotesToAdd.Count > 0)
                {
                    Logger.LogInformation("Adding {count} new quotes to content ID: {contentId}", quotesToAdd.Count, content.Id);
                    await Api.AddQuotesToContentAsync(content.Id, quotesToAdd);
                }
                else
                {
                    Logger.LogInformation("Content ID: {contentId} has no new quotes to add", content.Id);
                }
            }
            else
            {
                Logger.LogInformation("No quotes extracted from content ID: {contentId}", content.Id);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Quote extraction failed for content ID: {contentId}", content.Id);
            throw;
        }
    }

    #endregion
}
