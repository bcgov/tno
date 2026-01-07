using System.Globalization;
using System.Text;
using Confluent.Kafka;
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
using TNO.Services.AutoClipper.Azure;
using TNO.Services.AutoClipper.Config;
using TNO.Services.AutoClipper.Exceptions;
using TNO.Services.AutoClipper.LLM;
using TNO.Services.AutoClipper.Pipeline;
using TNO.Services.Managers;
using TNO.Services.Runners;

namespace TNO.Services.AutoClipper;

/// <summary>
/// AutoClipperManager class, provides a Kafka Consumer service which imports audio from all active topics.
/// </summary>
public class AutoClipperManager : ServiceManager<AutoClipperOptions>
{
    #region Variables
    private readonly IS3StorageService _s3StorageService;
    private readonly ClipProcessingPipeline _processingPipeline;
    private readonly IStationConfigurationService _stationConfiguration;
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private readonly WorkOrderStatus[] _ignoreWorkOrders = new WorkOrderStatus[] { WorkOrderStatus.Completed, WorkOrderStatus.Cancelled };
    private int _retries = 0;
    private string? _etag = null;
    private API.Areas.Editor.Models.Tag.TagModel[]? _tags = [];
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka Consumer object.
    /// </summary>
    protected IKafkaListener<string, ClipRequestModel> Listener { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a AutoClipperManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="api"></param>
    /// <param name="chesService"></param>
    /// <param name="chesOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    /// <param name="s3StorageService"></param>
    public AutoClipperManager(
        IKafkaListener<string, ClipRequestModel> listener,
        ClipProcessingPipeline processingPipeline,
        IStationConfigurationService stationConfigurationService,
        IApiService api,
        IChesService chesService,
        IOptions<ChesOptions> chesOptions,
        IOptions<AutoClipperOptions> options,
        ILogger<AutoClipperManager> logger,
        IS3StorageService s3StorageService)
        : base(api, chesService, chesOptions, options, logger)
    {
        this.Listener = listener;
        this.Listener.IsLongRunningJob = true;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
        _s3StorageService = s3StorageService;
        _processingPipeline = processingPipeline;
        _stationConfiguration = stationConfigurationService;
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
    private async Task HandleMessageAsync(ConsumeResult<string, ClipRequestModel> result)
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
                    await ProcessClipRequestAsync(request, content);
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
                    this.Logger.LogDebug("S3 file {path} is downloaded to: {file}", s3Path, tmpFilePath);
                    return tmpFilePath;
                }
            }
            else
            {
                this.Logger.LogError("Cannot download file {file} from S3", s3Path);
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
    private async Task ProcessClipRequestAsync(ClipRequestModel request, ContentModel content)
    {
        var requestContentId = request.ContentId;
        this.Logger.LogInformation("Auto clipper request received.  Content ID: {Id}", requestContentId);

        var stationCode = content.Source?.Code ?? content.Source?.Name ?? "default";
        var stationProfile = _stationConfiguration.GetProfile(stationCode);

        var contentFile = content.FileReferences.FirstOrDefault();
        var relativePath = contentFile?.Path;
        var clipSourcePath = !string.IsNullOrWhiteSpace(relativePath) ? Path.Join(this.Options.VolumePath, relativePath.MakeRelativePath()) : string.Empty;
        var isSyncedToS3 = contentFile?.IsSyncedToS3 == true;
        var downloadedFile = string.Empty;
        var generatedClipFiles = new List<string>();

        if (isSyncedToS3)
        {
            clipSourcePath = contentFile?.S3Path ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(contentFile?.S3Path))
            {
                downloadedFile = await DownloadS3File(contentFile.S3Path);
                if (!string.IsNullOrWhiteSpace(downloadedFile))
                {
                    clipSourcePath = downloadedFile;
                }
            }
        }

        if (!File.Exists(clipSourcePath))
        {
            this.Logger.LogError("File does not exist for content. Content ID: {Id}, Path: {Path}", requestContentId, clipSourcePath);
            var workOrderFailedException = new FileMissingException(requestContentId, clipSourcePath ?? string.Empty);
            await this.SendNoticeEmailAsync($"File missing for Content ID: {requestContentId}", workOrderFailedException);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, workOrderFailedException);
            return;
        }

        var workOrder = await UpdateWorkOrderAsync(request, content.IsApproved ? WorkOrderStatus.Cancelled : WorkOrderStatus.InProgress);
        var originalBody = content.Body;
        if (workOrder?.Status != WorkOrderStatus.InProgress)
        {
            if (workOrder?.Status == WorkOrderStatus.Cancelled)
                this.Logger.LogWarning("Work order has been cancelled.  Content ID: {id}", requestContentId);
            else
                this.Logger.LogWarning("Request ignored because it does not have a work order.  Content ID: {id}", requestContentId);
            CleanupLocalFiles(generatedClipFiles);
            CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
            return;
        }

        var targetSampleRate = stationProfile.Transcription.SampleRate > 0 ? stationProfile.Transcription.SampleRate : stationProfile.SampleRate;
        var processingContext = new ClipProcessingContext(
            clipSourcePath,
            stationProfile,
            request,
            targetSampleRate);
        var pipelineResult = await _processingPipeline.ExecuteAsync(processingContext, _cancelToken?.Token ?? CancellationToken.None);
        var segments = pipelineResult.Segments;
        var clipDefinitions = pipelineResult.ClipDefinitions?.OrderBy(c => c.Start).ToArray() ?? Array.Empty<ClipDefinition>();

        if (segments.Count == 0)
        {
            var exception = new EmptyTranscriptException(requestContentId);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, exception);
            CleanupLocalFiles(generatedClipFiles);
            CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
            return;
        }

        if (clipDefinitions.Length == 0)
        {
            clipDefinitions = new[] { new ClipDefinition("Full Program", "AutoClipper fallback", TimeSpan.Zero, segments[^1].End) };
        }

        workOrder = await this.Api.FindWorkOrderAsync(workOrder.Id);
        if (workOrder?.Status == WorkOrderStatus.Cancelled)
        {
            this.Logger.LogWarning("Work order has been cancelled during processing.  Content ID: {id}", requestContentId);
            CleanupLocalFiles(generatedClipFiles);
            CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
            return;
        }

        content = (await this.Api.FindContentByIdAsync(requestContentId))!;
        if (content == null)
        {
            var exception = new ContentNotFoundException(requestContentId);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Failed, exception);
            CleanupLocalFiles(generatedClipFiles);
            CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
            return;
        }

        if (content.IsApproved)
        {
            this.Logger.LogWarning("Content is approved; transcription will not be updated.  Content ID: {Id}", requestContentId);
            await UpdateWorkOrderAsync(request, WorkOrderStatus.Cancelled);
            CleanupLocalFiles(generatedClipFiles);
            CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
            return;
        }

        var transcriptBody = BuildTranscriptDocument(segments);
        if (!string.Equals(originalBody, content.Body, StringComparison.Ordinal))
            this.Logger.LogWarning("Transcript will be overwritten.  Content ID: {Id}", requestContentId);

        content.Body = transcriptBody;
        await this.Api.UpdateContentAsync(content);
        this.Logger.LogInformation("Primary transcript updated.  Content ID: {Id}", requestContentId);

        // Fetch tags once for all clips from the API.
        var tagsResponse = await this.Api.GetTagsResponseWithEtagAsync(_etag ?? "");
        if (tagsResponse != null && tagsResponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
            _tags = await this.Api.GetResponseDataAsync<API.Areas.Editor.Models.Tag.TagModel[]>(tagsResponse);
            _etag = this.Api.GetResponseEtag(tagsResponse);
        }

        var clipIndex = 1;
        foreach (var definition in clipDefinitions)
        {
            var normalized = NormalizeClipDefinition(definition, segments);
            if (normalized == null)
            {
                this.Logger.LogWarning("Skipped invalid clip definition for content {Id}", requestContentId);
                continue;
            }

            var clipTranscriptSegments = ExtractTranscriptRange(segments, normalized.Start, normalized.End);
            if (clipTranscriptSegments.Count == 0)
            {
                this.Logger.LogWarning("No transcript rows found for clip {clip}", definition.Title);
                continue;
            }

            var clipTranscript = BuildTranscriptDocument(clipTranscriptSegments);
            if (string.IsNullOrWhiteSpace(clipTranscript))
            {
                this.Logger.LogWarning("Empty transcript for clip definition {clip}", definition.Title);
                continue;
            }

            string clipPath;
            try
            {
                clipPath = await CreateClipFileAsync(clipSourcePath, $"autoclip_{requestContentId}", normalized.Start, normalized.End);
                generatedClipFiles.Add(clipPath);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to generate clip media for content {Id}", requestContentId);
                continue;
            }

            try
            {
                await CreateClipContentAsync(content, normalized, clipTranscript, clipPath, clipIndex);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to create clip content for content {Id}", requestContentId);
            }

            clipIndex++;
        }

        await UpdateWorkOrderAsync(request, WorkOrderStatus.Completed);
        CleanupLocalFiles(generatedClipFiles);
        CleanupTemporaryFiles(isSyncedToS3, downloadedFile);
    }

    private ClipDefinition? NormalizeClipDefinition(ClipDefinition definition, IReadOnlyList<TimestampedTranscript> segments)
    {
        if (segments.Count == 0) return null;
        var maxEnd = segments[^1].End;
        var start = definition.Start < TimeSpan.Zero ? TimeSpan.Zero : definition.Start;
        var end = definition.End > maxEnd ? maxEnd : definition.End;
        if (end <= start) return null;

        var first = segments.FirstOrDefault(s => s.End > start);
        var last = segments.LastOrDefault(s => s.Start < end);
        if (first == null || last == null) return null;
        start = first.Start;
        end = last.End;
        if (end <= start) return null;

        return definition with { Start = start, End = end };
    }

    private IReadOnlyList<TimestampedTranscript> ExtractTranscriptRange(IReadOnlyList<TimestampedTranscript> segments, TimeSpan start, TimeSpan end)
    {
        return segments.Where(s => s.End > start && s.Start < end).ToArray();
    }

    private async Task<string> CreateClipFileAsync(string srcFile, string outputPrefix, TimeSpan start, TimeSpan end)
    {
        var directory = Path.GetDirectoryName(srcFile) ?? Path.GetTempPath();
        var ext = Path.GetExtension(srcFile);
        var clipName = $"{outputPrefix}_{Guid.NewGuid():N}{ext}";
        var clipPath = Path.Combine(directory, clipName);
        var duration = Math.Max(1, (end - start).TotalSeconds);
        var process = new System.Diagnostics.Process();
        process.StartInfo.Verb = "clip";
        if (OperatingSystem.IsWindows())
        {
            process.StartInfo.FileName = "cmd";
            process.StartInfo.Arguments = $"/c ffmpeg -y -ss {start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} -i \"{srcFile}\" -t {duration.ToString("0.###", CultureInfo.InvariantCulture)} -c copy \"{clipPath}\"";
        }
        else
        {
            process.StartInfo.FileName = "/bin/sh";
            process.StartInfo.Arguments = $"-c \"ffmpeg -y -ss {start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} -i '{srcFile}' -t {duration.ToString("0.###", CultureInfo.InvariantCulture)} -c copy '{clipPath}' 2>&1\"";
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"ffmpeg failed to create clip: {output}");
        return clipPath;
    }

    private async Task CreateClipContentAsync(ContentModel parentContent, ClipDefinition definition, string transcriptBody, string clipPath, int clipIndex)
    {
        var clipContent = await BuildClipContentModelAsync(parentContent, definition, transcriptBody, clipIndex);
        var created = await this.Api.AddContentAsync(clipContent);
        if (created == null) return;

        await using var clipStream = File.OpenRead(clipPath);
        created = await this.Api.UploadFileAsync(created.Id, created.Version ?? 0, clipStream, Path.GetFileName(clipPath));
        this.Logger.LogInformation("Clip content created. Parent Content: {ParentId}, Clip Content: {ClipId}", parentContent.Id, created?.Id);
    }

    private async Task<ContentModel> BuildClipContentModelAsync(ContentModel sourceContent, ClipDefinition definition, string transcriptBody, int clipIndex)
    {
        var clipSummary = string.IsNullOrWhiteSpace(definition.Summary)
            ? $"Clip covering {FormatTimestamp(definition.Start)} to {FormatTimestamp(definition.End)}"
            : definition.Summary;

        var autoTags = _tags?.Where(t => this.Options.ApplyTags.Contains(t.Code));
        var tags = autoTags != null ? sourceContent.Tags.AppendRange(autoTags.Select(at => new ContentTagModel(at.Id, at.Code, at.Name))) : sourceContent.Tags;

        // Calculate clip time = parent content publish time + clip start offset
        var clipTime = sourceContent.PublishedOn?.Add(definition.Start);
        var timePrefix = clipTime?.ToString("HH:mm");

        return new ContentModel
        {
            ContentType = sourceContent.ContentType,
            SourceId = sourceContent.SourceId,
            Source = sourceContent.Source,
            OtherSource = sourceContent.OtherSource,
            SeriesId = sourceContent.SeriesId,
            Series = sourceContent.Series,
            MediaTypeId = sourceContent.MediaTypeId,
            LicenseId = sourceContent.LicenseId,
            OwnerId = sourceContent.OwnerId,
            ContributorId = sourceContent.ContributorId,
            Contributor = sourceContent.Contributor,
            Byline = sourceContent.Byline,
            Status = ContentStatus.Draft,
            Uid = BaseService.GetContentHash(sourceContent.Source?.Code ?? "AutoClipper", $"{sourceContent.Uid}-clip-{clipIndex}", sourceContent.PublishedOn),
            Headline = string.IsNullOrEmpty(timePrefix) ? definition.Title : $"{timePrefix} - {definition.Title}",
            Summary = $"[AutoClipper:{definition.Category}]\n{clipSummary}",
            Body = transcriptBody,
            SourceUrl = sourceContent.SourceUrl,
            PublishedOn = clipTime ?? sourceContent.PublishedOn,
            PostedOn = DateTime.UtcNow,
            Tags = tags,
            Topics = sourceContent.Topics,
            Actions = sourceContent.Actions,
            Labels = sourceContent.Labels,
            IsHidden = false
        };
    }

    private void CleanupLocalFiles(IEnumerable<string> files)
    {
        CleanupS3Files(files.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray());
    }

    private void CleanupTemporaryFiles(bool isSyncedToS3, params string[] files)
    {
        if (!isSyncedToS3) return;
        CleanupS3Files(files);
    }

    /// <summary>
    /// Format the transcript to include newlines.
    /// </summary>
    /// <param name="transcript"></param>
    /// <returns></returns>
    private static string BuildTranscriptDocument(IReadOnlyList<TimestampedTranscript> segments)
    {
        if (segments == null || segments.Count == 0) return string.Empty;

        var sb = new StringBuilder();
        var index = 1;
        foreach (var segment in segments)
        {
            if (string.IsNullOrWhiteSpace(segment.Text)) continue;
            // sb.AppendLine(index.ToString(CultureInfo.InvariantCulture));
            // sb.AppendLine($"{FormatTimestamp(segment.Start)} --> {FormatTimestamp(segment.End)}");
            sb.AppendLine(segment.Text.Trim());
            sb.AppendLine();
            index++;
        }

        return sb.ToString().Trim();
    }

    private static string FormatTimestamp(TimeSpan value)
    {
        var hours = (int)Math.Floor(value.TotalHours);
        return string.Format(CultureInfo.InvariantCulture, "{0:00}:{1:00}:{2:00}.{3:000}", hours, value.Minutes, value.Seconds, value.Milliseconds);
    }

    /// <summary>
    /// Update the work order (if it exists) with the specified 'status'.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="status"></param>
    /// <returns>Whether a work order exists or is not required.</returns>
    private async Task<API.Areas.Services.Models.WorkOrder.WorkOrderModel?> UpdateWorkOrderAsync(ClipRequestModel request, WorkOrderStatus status, Exception? reason = null)
    {
        if (request.WorkOrderId > 0)
        {
            var workOrder = await this.Api.FindWorkOrderAsync(request.WorkOrderId);
            if (workOrder != null && !_ignoreWorkOrders.Contains(workOrder.Status))
            {
                workOrder.Status = status;
                workOrder = await this.Api.UpdateWorkOrderAsync(workOrder);

                if (status == WorkOrderStatus.Failed && reason != null)
                {
                    await this.SendErrorEmailAsync($"Work order failed for Content ID: {request.ContentId}", reason);
                    this.Logger.LogError(reason, "Work order failed for Content ID: {ContentId}", request.ContentId);
                }
            }
            return workOrder;
        }
        return null;
    }

    /// <summary>
    /// Stream the audio file to Azure and return the speech to text output.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <param name="language"></param>
    /// <returns></returns>
    /// <summary>
    /// video to audio
    /// </summary>
    /// <param name="srcFile">source file path</param>
    /// <param name="destFile">destination file path</param>
    /// <returns>destination file name</returns>
    #endregion
}

