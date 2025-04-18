using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using Confluent.Kafka;
using HtmlAgilityPack;
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
/// Class to hold content batch item information
/// </summary>
public class ContentBatchItem
{
    /// <summary>
    /// Gets or sets the content ID
    /// </summary>
    public long ContentId { get; set; }

    /// <summary>
    /// Gets or sets the content model
    /// </summary>
    public API.Areas.Services.Models.Content.ContentModel Content { get; set; }

    /// <summary>
    /// Gets or sets the Kafka message result
    /// </summary>
    public ConsumeResult<string, IndexRequestModel> KafkaResult { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this item was added to the batch
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

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

    // Batch processing variables
    private readonly ConcurrentDictionary<long, ContentBatchItem> _batchQueue = new ConcurrentDictionary<long, ContentBatchItem>();
    private readonly SemaphoreSlim _batchSemaphore = new SemaphoreSlim(1, 1);
    private Timer? _batchTimer;
    private bool _processingBatch = false;
    private DateTime? _processingBatchStartTime = null;

    // TPL Dataflow variables
    private BatchBlock<ContentBatchItem>? _batchBlock;
    private ITargetBlock<ContentBatchItem>? _pipeline;
    private bool _dataflowInitialized = false;
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
        Listener = listener;
        Listener.IsLongRunningJob = true;
        Listener.OnError += ListenerErrorHandler;
        Listener.OnStop += ListenerStopHandler;

        CoreNLPService = coreNLPService;

        // Log configuration settings
        Logger.LogInformation("Quote extraction configuration - Extract quotes for Index operation: {onIndex}, Extract quotes for Publish operation: {onPublish}",
            Options.ExtractQuotesOnIndex, Options.ExtractQuotesOnPublish);

        // Initialize batch timer if batch processing is enabled
        if (Options.UseBatchProcessing)
        {
            Logger.LogInformation("Batch processing enabled - Batch size: {batchSize}, Timeout: {timeoutMs}ms", Options.BatchSize, Options.BatchTimeoutMs);
            _batchTimer = new Timer(BatchTimerCallback, null, Timeout.Infinite, Timeout.Infinite);

            // Initialize TPL Dataflow pipeline
            InitializeDataflowPipeline();
        }
        else
        {
            Logger.LogInformation("Batch processing disabled - Will process each content item immediately");
        }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Clean up resources when the service is stopping
    /// </summary>
    private void CleanupResources()
    {
        Logger.LogInformation("Cleaning up quote extraction service resources...");

        // Clean up TPL Dataflow resources
        if (Options.UseBatchProcessing && _batchBlock != null)
        {
            try
            {
                Logger.LogInformation("Closing dataflow pipeline");

                // Complete the batch block
                _batchBlock.Complete();

                // Wait for completion with timeout
                var completionTask = _batchBlock.Completion;
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));

                var completedTask = Task.WhenAny(completionTask, timeoutTask).GetAwaiter().GetResult();
                if (completedTask == timeoutTask)
                {
                    Logger.LogWarning("Timeout waiting for dataflow pipeline completion");
                }
                else
                {
                    Logger.LogInformation("Dataflow pipeline successfully closed");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error occurred while closing dataflow pipeline");
            }
        }

        // Dispose the batch timer
        _batchTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _batchTimer?.Dispose();
        _batchTimer = null;
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

        // Process any remaining items in the batch queue before shutting down
        if (Options.UseBatchProcessing && _batchQueue.Count > 0)
        {
            try
            {
                // Process the batch synchronously to ensure it completes before shutdown
                ProcessBatchAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error processing batch during shutdown");
            }
        }

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
                Logger.LogDebug("Starting to process message - Topic: {topic}, Content ID: {key}", result.Topic, result.Message.Key);

                // Set a timeout for processing to prevent hanging
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout
                var processingTask = ProcessIndexRequestAsync(result);

                try
                {
                    // Wait for processing to complete or timeout
                    await processingTask.WaitAsync(timeoutCts.Token);

                    // Inform Kafka this message is completed.
                    Listener.Commit(result);

                    // Successful run clears any errors.
                    State.ResetFailures();
                    _retries = 0;

                    Logger.LogDebug("Message processing completed - Topic: {topic}, Content ID: {key}", result.Topic, result.Message.Key);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogWarning("Message processing timed out - Topic: {topic}, Content ID: {key}", result.Topic, result.Message.Key);
                    // Still commit the message to avoid getting stuck on it
                    Listener.Commit(result);
                }
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
                Logger.LogDebug("Resuming consumption - Topic: {topic}", result.Topic);
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
        Logger.LogDebug("ProcessIndexRequestAsync:BEGIN:{key}", result.Message.Key);

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

                // If batch processing is enabled, add to batch queue instead of processing immediately
                if (Options.UseBatchProcessing)
                {
                    await AddToBatchQueueAsync(result, content);
                }
                else
                {
                    // Process immediately using the original approach
                    var ministers = await Api.GetMinistersAsync();
                    var batchItem = new ContentBatchItem
                    {
                        ContentId = content.Id,
                        Content = content,
                        KafkaResult = result
                    };
                    await ProcessContentItemAsync(batchItem, ministers);
                }
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
        Logger.LogDebug("ProcessIndexRequestAsync:END:{key}", result.Message.Key);
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

    #region Batch Processing Methods

    /// <summary>
    /// Initialize the TPL Dataflow pipeline for batch processing
    /// </summary>
    private void InitializeDataflowPipeline()
    {
        // Create a batch block to collect items into batches
        _batchBlock = new BatchBlock<ContentBatchItem>(Options.BatchSize);

        // Create a transform block to process each batch
        var processBatchBlock = new TransformBlock<ContentBatchItem[], ContentBatchItem[]>(
            async batch =>
            {
                Logger.LogInformation("Starting batch processing - Contains {count} content items", batch.Length);

                try
                {
                    // Get ministers once for the entire batch
                    var ministers = await Api.GetMinistersAsync();
                    Logger.LogDebug("Retrieved {count} ministers", ministers.Count());

                    // Process each item in the batch sequentially
                    int successCount = 0;
                    for (int i = 0; i < batch.Length; i++)
                    {
                        var item = batch[i];
                        try
                        {
                            Logger.LogInformation("Processing content ID in batch: {contentId} ({current}/{total})",
                                item.ContentId, i + 1, batch.Length);

                            await ProcessContentItemAsync(item, ministers);
                            successCount++;

                            Logger.LogDebug("Successfully processed content ID: {contentId}", item.ContentId);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Failed to process content ID {contentId}", item.ContentId);
                        }
                        finally
                        {
                            // Always commit the message to avoid getting stuck
                            Listener.Commit(item.KafkaResult);
                        }
                    }

                    Logger.LogInformation("Batch processing completed - Total: {total}, Success: {success}, Failed: {failed}",
                        batch.Length, successCount, batch.Length - successCount);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Exception occurred during batch processing");

                    // Commit all messages to avoid getting stuck
                    foreach (var item in batch)
                    {
                        Listener.Commit(item.KafkaResult);
                        Logger.LogWarning("Content ID committed due to exception: {contentId}", item.ContentId);
                    }
                }

                return batch; // Return the batch for the next block
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1, // Ensure sequential processing
                BoundedCapacity = 1        // Only process one batch at a time
            });

        // Create a completion block to handle post-processing
        var completionBlock = new ActionBlock<ContentBatchItem[]>(
            batch =>
            {
                // Ensure Kafka consumption is resumed
                if (State.Status == ServiceStatus.Running)
                {
                    Logger.LogDebug("Ensuring Kafka consumption is resumed");
                    Listener.Resume();
                }

                // Check if there are more items to process
                if (_batchQueue.Count > 0)
                {
                    Logger.LogInformation("Queue still has {count} items to process - continuing batch processing", _batchQueue.Count);

                    // Get the next batch of items from the queue
                    var nextBatch = _batchQueue.Values.Take(Options.BatchSize).ToList();
                    foreach (var item in nextBatch)
                    {
                        if (_batchQueue.TryRemove(item.ContentId, out _))
                        {
                            // Send to the pipeline
                            _batchBlock!.Post(item);
                        }
                    }

                    // Trigger batch processing if we don't have enough items
                    if (nextBatch.Count < Options.BatchSize)
                    {
                        Logger.LogInformation("Forcing processing of current batch - Item count: {count}", nextBatch.Count);
                        _batchBlock!.TriggerBatch();
                    }
                }
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1 // Ensure sequential processing
            });

        // Link the blocks together
        _batchBlock.LinkTo(processBatchBlock, new DataflowLinkOptions { PropagateCompletion = true });
        processBatchBlock.LinkTo(completionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // Set the pipeline entry point
        _pipeline = _batchBlock;

        _dataflowInitialized = true;
        Logger.LogInformation("Dataflow pipeline initialized - Batch size: {batchSize}", Options.BatchSize);
    }

    /// <summary>
    /// Callback method for the batch timer
    /// </summary>
    private async void BatchTimerCallback(object? state)
    {
        // Stop the timer to prevent overlapping executions
        _batchTimer?.Change(Timeout.Infinite, Timeout.Infinite);

        try
        {
            int queueSize = _batchQueue.Count;
            Logger.LogDebug("Batch processing timeout callback triggered - Queue size: {count}, Processing: {processing}",
                queueSize, _processingBatch);

            // If queue is too large, clear some old items to prevent memory issues
            if (queueSize > 50) // If queue has more than 50 items
            {
                Logger.LogWarning("Batch processing queue too large ({size}) - cleaning up old items", queueSize);

                // Get items sorted by age (oldest first)
                var oldItems = _batchQueue.Values
                    .OrderBy(item => item.AddedAt)
                    .Take(queueSize - 30) // Keep the newest 30 items
                    .ToList();

                foreach (var item in oldItems)
                {
                    if (_batchQueue.TryRemove(item.ContentId, out _))
                    {
                        // Commit the message to avoid getting stuck
                        Listener.Commit(item.KafkaResult);
                        Logger.LogInformation("Content ID removed and committed due to large queue: {contentId}", item.ContentId);
                    }
                }

                Logger.LogInformation("Queue size after cleanup: {size}", _batchQueue.Count);
            }

            // If using TPL Dataflow, trigger batch processing
            if (_batchBlock != null && _batchQueue.Count > 0)
            {
                Logger.LogInformation("Batch processing timeout - triggering dataflow batch processing");
                _batchBlock.TriggerBatch();

                // Make sure Kafka consumption is resumed
                if (State.Status == ServiceStatus.Running)
                {
                    Logger.LogDebug("Ensuring Kafka consumption is resumed after triggering batch processing");
                    Listener.Resume();
                }
            }
            // Fallback to old method if dataflow is not initialized
            else if (_batchQueue.Count > 0 && !_processingBatch)
            {
                // Use a timeout to prevent hanging
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // 30 second timeout

                try
                {
                    // Process batch with timeout
                    var processingTask = ProcessBatchAsync();
                    await processingTask.WaitAsync(timeoutCts.Token);
                }
                catch (OperationCanceledException)
                {
                    Logger.LogWarning("Processing timeout in batch processing timeout callback");

                    // Force reset processing flag in case of timeout
                    _processingBatch = false;

                    // Make sure Kafka consumption is resumed
                    if (State.Status == ServiceStatus.Running)
                    {
                        Logger.LogDebug("Ensuring Kafka consumption is resumed after timeout");
                        Listener.Resume();
                    }
                }
            }
            else if (_processingBatch)
            {
                Logger.LogDebug("Batch processing timeout callback - Batch processing already in progress");

                // If processing has been going on for too long, force reset
                // This is a safety mechanism to prevent permanent deadlocks
                if (_processingBatchStartTime.HasValue &&
                    (DateTime.UtcNow - _processingBatchStartTime.Value).TotalSeconds > 120) // 2 minute timeout
                {
                    Logger.LogWarning("Batch processing running too long ({seconds} seconds) - Forcing reset of processing state",
                        (DateTime.UtcNow - _processingBatchStartTime.Value).TotalSeconds);

                    _processingBatch = false;
                    _processingBatchStartTime = null;

                    // Make sure Kafka consumption is resumed
                    if (State.Status == ServiceStatus.Running)
                    {
                        Logger.LogDebug("Ensuring Kafka consumption is resumed after forced reset");
                        Listener.Resume();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred in batch processing timeout callback");

            // Force reset processing flag in case of error
            _processingBatch = false;

            // Make sure Kafka consumption is resumed
            if (State.Status == ServiceStatus.Running)
            {
                Logger.LogDebug("Ensuring Kafka consumption is resumed after error");
                Listener.Resume();
            }
        }
        finally
        {
            // If there are still items in the queue
            int remainingItems = _batchQueue.Count;
            if (remainingItems > 0)
            {
                // If using TPL Dataflow
                if (_batchBlock != null)
                {
                    // Restart the timer
                    Logger.LogDebug("Restarting batch processing timeout timer - {timeoutMs}ms", Options.BatchTimeoutMs);
                    _batchTimer?.Change(Options.BatchTimeoutMs, Timeout.Infinite);
                }
                // Fallback to old method
                else if (remainingItems >= Options.BatchSize && !_processingBatch)
                {
                    // If we have enough items for a full batch and not currently processing, process immediately
                    Logger.LogInformation("Batch processing timeout callback - Queue has {count} items, starting processing immediately", remainingItems);

                    // Start processing in a new task to avoid blocking
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Wait a short delay to ensure current processing is fully complete
                            await Task.Delay(1000);
                            await ProcessBatchAsync();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex, "Error occurred while processing next batch in batch processing timeout callback");
                        }
                    });
                }
                else
                {
                    // Otherwise restart the timer
                    Logger.LogDebug("Restarting batch processing timeout timer - {timeoutMs}ms", Options.BatchTimeoutMs);
                    _batchTimer?.Change(Options.BatchTimeoutMs, Timeout.Infinite);
                }
            }
        }
    }

    /// <summary>
    /// Add a content item to the batch queue
    /// </summary>
    private async Task AddToBatchQueueAsync(ConsumeResult<string, IndexRequestModel> result, API.Areas.Services.Models.Content.ContentModel content)
    {
        try
        {
            var contentId = content.Id;
            var batchItem = new ContentBatchItem
            {
                ContentId = contentId,
                Content = content,
                KafkaResult = result,
                AddedAt = DateTime.UtcNow
            };

            // Add to the queue for tracking
            _batchQueue[contentId] = batchItem;

            int queueSize = _batchQueue.Count;
            int batchSize = Options.BatchSize;

            Logger.LogInformation("Added content ID {contentId} to batch processing queue - Current queue size: {queueSize}/{batchSize}",
                contentId, queueSize, batchSize);

            // If this is the first item, start the timer
            if (queueSize == 1)
            {
                _batchTimer?.Change(Options.BatchTimeoutMs, Timeout.Infinite);
                Logger.LogInformation("Starting batch processing timeout timer - Timeout: {timeoutMs}ms", Options.BatchTimeoutMs);
            }

            // Send the item to the dataflow pipeline
            if (_batchBlock != null)
            {
                bool accepted = _batchBlock.Post(batchItem);
                if (!accepted)
                {
                    Logger.LogWarning("Unable to send content ID {contentId} to dataflow pipeline - May be full", contentId);
                    // Still commit the message to avoid getting stuck
                    Listener.Commit(result);
                }

                // If we've reached the batch size, trigger batch processing
                if (queueSize >= batchSize && !_batchBlock.TryReceive(out _)) // Check if the block is not empty
                {
                    Logger.LogInformation("Batch size reached ({batchSize}) - Batch processing will start automatically", batchSize);
                }
            }
            else
            {
                // Fallback to old method if dataflow is not initialized
                Logger.LogWarning("Dataflow pipeline not initialized - Using old batch processing method");

                // Use a timeout to prevent hanging
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5)); // 5 second timeout

                try
                {
                    // Process batch with timeout
                    var processingTask = ProcessBatchAsync();
                    await processingTask.WaitAsync(timeoutCts.Token);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error occurred while processing batch - Content ID: {contentId}", contentId);
                    // Still commit the message to avoid getting stuck
                    Listener.Commit(result);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error occurred while adding content ID {contentId} to batch processing queue", content.Id);

            // Still commit the message to avoid getting stuck
            Listener.Commit(result);
        }
    }

    /// <summary>
    /// Process all items in the batch queue
    /// </summary>
    private async Task ProcessBatchAsync()
    {
        // Prevent concurrent batch processing
        if (_processingBatch)
        {
            Logger.LogInformation("Batch processing already in progress - Skipping this processing request");
            return;
        }

        // Use a timeout to prevent hanging
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // 60 second timeout

        // Try to acquire the semaphore with timeout
        bool semaphoreAcquired = false;
        try
        {
            semaphoreAcquired = await _batchSemaphore.WaitAsync(5000); // 5 second timeout
            if (!semaphoreAcquired)
            {
                Logger.LogWarning("Unable to acquire batch processing semaphore - Possible deadlock");
                return;
            }

            _processingBatch = true;
            _processingBatchStartTime = DateTime.UtcNow;

            Logger.LogDebug("Batch processing start time: {startTime}", _processingBatchStartTime);

            // Stop the timer while processing
            _batchTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            if (_batchQueue.Count == 0)
            {
                Logger.LogInformation("Batch processing queue is empty - No processing needed");
                return;
            }

            Logger.LogInformation("Starting batch processing - Contains {count} content items", _batchQueue.Count);

            // Get all items from the queue (limit to a reasonable number to prevent overload)
            var allItems = _batchQueue.Values.ToList();
            int maxItemsToProcess = Math.Min(allItems.Count, 10); // Process at most 10 items

            var batchItems = allItems.Take(maxItemsToProcess).ToList();

            // Remove only the items we're going to process
            foreach (var item in batchItems)
            {
                _batchQueue.TryRemove(item.ContentId, out _);
            }

            Logger.LogInformation("Took {count}/{total} items from queue for processing",
                batchItems.Count, allItems.Count);

            try
            {
                // Get all ministers once for the entire batch
                Logger.LogDebug("Getting minister information for the entire batch");
                var ministers = await Api.GetMinistersAsync().WaitAsync(timeoutCts.Token);
                Logger.LogDebug("Retrieved {count} ministers", ministers.Count());

                // Process each content item
                int successCount = 0;
                foreach (var batchItem in batchItems)
                {
                    // Check for cancellation between items
                    timeoutCts.Token.ThrowIfCancellationRequested();

                    try
                    {
                        Logger.LogInformation("Processing content ID in batch: {contentId}", batchItem.ContentId);

                        // Process with timeout
                        await ProcessContentItemAsync(batchItem, ministers).WaitAsync(timeoutCts.Token);

                        // Commit the Kafka message
                        Listener.Commit(batchItem.KafkaResult);
                        successCount++;

                        Logger.LogDebug("Successfully processed content ID: {contentId}", batchItem.ContentId);
                    }
                    catch (OperationCanceledException)
                    {
                        Logger.LogWarning("Processing content ID: {contentId} timed out", batchItem.ContentId);
                        // Still commit to avoid getting stuck
                        Listener.Commit(batchItem.KafkaResult);
                        // Re-throw to exit the loop
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Failed to process content ID {contentId} in batch", batchItem.ContentId);
                        // Still commit to avoid getting stuck
                        Listener.Commit(batchItem.KafkaResult);
                    }
                }

                Logger.LogInformation("Batch processing completed - Total: {total}, Success: {success}, Failed: {failed}",
                    batchItems.Count, successCount, batchItems.Count - successCount);
            }
            catch (OperationCanceledException)
            {
                Logger.LogWarning("Batch processing timed out - Possible blocking or deadlock");

                // Commit all remaining items to avoid getting stuck
                foreach (var batchItem in batchItems)
                {
                    Listener.Commit(batchItem.KafkaResult);
                    Logger.LogWarning("Content ID committed due to timeout: {contentId}", batchItem.ContentId);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception occurred during batch processing");

                // Commit all remaining items to avoid getting stuck
                foreach (var batchItem in batchItems)
                {
                    Listener.Commit(batchItem.KafkaResult);
                    Logger.LogWarning("Content ID committed due to exception: {contentId}", batchItem.ContentId);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Exception occurred during batch preparation");
        }
        finally
        {
            _processingBatch = false;
            _processingBatchStartTime = null;

            // Release the semaphore if we acquired it
            if (semaphoreAcquired)
            {
                _batchSemaphore.Release();
            }

            // Make sure Kafka consumption is resumed
            if (State.Status == ServiceStatus.Running)
            {
                Logger.LogDebug("Ensuring Kafka consumption is resumed");
                Listener.Resume();
            }

            // If there are new items in the queue, process them immediately
            int remainingItems = _batchQueue.Count;
            if (remainingItems > 0)
            {
                Logger.LogInformation("Queue still has {count} items to process", remainingItems);

                // Always process remaining items immediately
                Logger.LogInformation("Starting to process next batch immediately - Queue still has {count} items", remainingItems);

                // Start processing in a new task to avoid blocking
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Wait a short delay to ensure current batch processing is fully complete
                        await Task.Delay(1000);
                        await ProcessBatchAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error occurred while processing next batch");
                    }
                });
            }
        }
    }

    /// <summary>
    /// Process a single content item for quote extraction
    /// </summary>
    private async Task ProcessContentItemAsync(ContentBatchItem batchItem, IEnumerable<MinisterModel> ministers)
    {
        var content = batchItem.Content;
        var result = batchItem.KafkaResult;

        Logger.LogInformation("Starting to process content ID: {contentId} - Using {serviceType} service",
            content.Id,
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
            Logger.LogDebug("Extracted text from content summary - Length: {length} characters", text.Length);
        }

        if (!string.IsNullOrWhiteSpace(content.Body))
        {
            var html = new HtmlDocument();
            html.LoadHtml(content.Body);
            foreach (HtmlNode node in html.DocumentNode.SelectNodes("//text()"))
            {
                text.AppendLine(node.InnerText);
            }
            Logger.LogDebug("Extracted text from content body - Length: {length} characters", text.Length);
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
                            Logger.LogDebug("New quote - Speaker: '{speaker}', Quote: '{quote}'", speaker, quote);
                        }
                    }
                }

                if (quotesToAdd.Count > 0)
                {
                    Logger.LogInformation("Adding {count} new quotes to content ID: {contentId}", content.Id, quotesToAdd.Count);
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

    #endregion
}
