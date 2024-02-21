using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Renci.SshNet.Common;
using TNO.API.Areas.Services.Models.Content;
using TNO.API.Areas.Services.Models.DataLocation;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka;
using TNO.Kafka.Models;
using TNO.Models.Extensions;
using TNO.Services.Content.Config;
using TNO.Services.Managers;

namespace TNO.Services.Content;

/// <summary>
/// ContentManager class, provides a Kafka Listener service which imports content from all active topics.
/// </summary>
public class ContentManager : ServiceManager<ContentOptions>
{
    #region Variables
    private CancellationTokenSource? _cancelToken;
    private Task? _consumer;
    private readonly TaskStatus[] _notRunning = new TaskStatus[] { TaskStatus.Canceled, TaskStatus.Faulted, TaskStatus.RanToCompletion };
    private int _retries = 0;
    #endregion

    #region Properties
    /// <summary>
    /// get - Kafka admin client.
    /// </summary>
    protected IKafkaAdmin KafkaAdmin { get; private set; }

    /// <summary>
    /// get - Kafka message consumer.
    /// </summary>
    protected IKafkaListener<string, SourceContent> Listener { get; private set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentManager object, initializes with specified parameters.
    /// </summary>
    /// <param name="kafkaAdmin"></param>
    /// <param name="kafkaListener"></param>
    /// <param name="api"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentManager(
        IKafkaAdmin kafkaAdmin,
        IKafkaListener<string, SourceContent> kafkaListener,
        IApiService api,
        IOptions<ContentOptions> options,
        ILogger<ContentManager> logger)
        : base(api, options, logger)
    {
        this.KafkaAdmin = kafkaAdmin;
        this.Listener = kafkaListener;
        this.Listener.IsLongRunningJob = false;
        this.Listener.OnError += ListenerErrorHandler;
        this.Listener.OnStop += ListenerStopHandler;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Continually poll for updated data source configuration.
    /// When there are topics to listen too it will initialize a Kafka consumer.
    /// When configuration updates result in changes to topics, it will update which topics it is subscribing too.
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
                    // TODO: Handle e-tag.
                    var ingest = (await this.Api.GetIngestsAsync()).ToArray();

                    // Listen to every enabled data source with a topic that is configured to produce content.
                    var topics = this.Options.GetContentTopics(ingest
                        .Where(i => i.IsEnabled &&
                            !String.IsNullOrWhiteSpace(i.Topic) &&
                            i.ImportContent())
                        .Select(i => i.Topic).ToArray());

                    // Only include topics that exist.
                    var kafkaTopics = this.KafkaAdmin.ListTopics();
                    topics = topics.Except(topics.Except(kafkaTopics), StringComparer.OrdinalIgnoreCase).ToArray();

                    // finally exclude any topics that are set to exclude
                    topics = topics.Except(this.Options.GetContentTopicsToExclude(), StringComparer.OrdinalIgnoreCase).ToArray();

                    if (topics.Length != 0)
                    {
                        this.Listener.Subscribe(topics);
                        ConsumeMessages();
                    }
                    else
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

            // The delay ensures we don't have a run-away thread.
            // With a minimum delay for all data source schedules, it could mean some data sources are pinged more often then required.
            // It could also result in a longer than planned delay if the action manager is awaited (currently it is).
            this.Logger.LogDebug("Service sleeping for {delay:n0} ms", delay);
            await Task.Delay(delay);

            // after the service has slept, it needs to be woken up
            this.State.Resume();
        }
    }

    /// <summary>
    /// Creates a new cancellation token.
    /// Create a new thread if the prior one isn't running anymore.
    /// </summary>
    private void ConsumeMessages()
    {
        if (_consumer == null || _notRunning.Contains(_consumer.Status))
        {
            this.Logger.LogDebug("ConsumeMessages:_consumer is null");
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
            this.Logger.LogDebug("ListenerHandlerAsync:AWAIT");
            await this.Listener.ConsumeAsync(HandleMessageAsync, _cancelToken.Token);
        }

        // The service is stopping or has stopped, consume should stop too.
        this.Logger.LogDebug("ListenerHandlerAsync:STOP");
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
        this.Logger.LogDebug("ListenerErrorHandler:BEGIN");
        this.Logger.LogDebug($"ListenerErrorHandler: Retries={_retries}");
        // Only the first retry will count as a failure.
        if (_retries == 0)
            this.State.RecordFailure();

        if (e.GetException() is ConsumeException consume)
        {
            if (consume.Error.IsFatal)
            {
                this.Listener.Stop();
                this.Logger.LogDebug("ListenerErrorHandler: Stop on IsFatal");
            }
        }
        this.Logger.LogDebug("ListenerErrorHandler:END");
    }

    /// <summary>
    /// The Kafka consumer has stopped which means we need to also cancel the background task associated with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ListenerStopHandler(object sender, EventArgs e)
    {
        this.Logger.LogDebug("ListenerStopHandler:BEGIN");
        if (_consumer != null &&
            !_notRunning.Contains(_consumer.Status) &&
            _cancelToken != null && !_cancelToken.IsCancellationRequested)
        {
            this.Logger.LogDebug("ListenerStopHandler.Cancel");
            _cancelToken.Cancel();
        }
        this.Logger.LogDebug("ListenerStopHandler:END");
    }

    /// <summary>
    /// Import the content.
    /// Copy any file associated with source content.
    /// </summary>
    /// <param name="result"></param>
    /// <returns>Whether to continue with the next message.</returns>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task HandleMessageAsync(ConsumeResult<string, SourceContent> result)
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
                await ProcessSourceContentAsync(result);

                // Inform Kafka this message is completed.
                this.Listener.Commit(result);

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
        finally
        {
            if (State.Status == ServiceStatus.Running) Listener.Resume();
        }
    }

    private async Task ProcessSourceContentAsync(ConsumeResult<string, SourceContent> result)
    {
        this.Logger.LogDebug($"ProcessSourceContentAsync:BEGIN:{result.Message.Key}");
        this.Logger.LogInformation("Importing Content from Topic: {topic}, Uid: {key}", result.Topic, result.Message.Key);

        var model = result.Message.Value;

        // We *should* never get data which has no title as we use the title to generate the hash.
        // A record with no title will never be valid because we can't match it to an updated record.
        if (string.IsNullOrEmpty(model.Title))
        {
            Logger.LogWarning("Skipping bad data: Content has no Title : {source}:{key}", model.Source, result.Message.Key);
            return;
        }

        bool updateSourceContent = false;
        long? existingContentId = null;

        ContentModel? originalContent = await this.Api.FindContentByUidAsync(model.Uid, model.Source);
        ContentModel? content = await this.Api.FindContentByUidAsync(model.Uid, model.Source);
        if (content != null)
        {
            // Only add if doesn't already exist.
            existingContentId = content.Id;

            // KGM - This code should be removed/refactored post PROD deployment most likely
            // IF we are allowing overwrites from the Content Migration Service
            if (result.Topic.Equals(this.Options.MigrationOptions.ContentMigrationIngestSourceCode))
            {
                // AND if the current content was ingested by the Content Migration Service
                // THEN we trigger a complete overwrite of the existing content with the updated content
                if (this.Options.MigrationOptions.AllowSourceContentOverwrite)
                {
                    updateSourceContent = true;
                    Logger.LogInformation("Received updated content from TNO. Forcing an update to the MMI Content : {Source}:{Title}", model.Source, model.Title);
                }
                else
                {
                    updateSourceContent = false;
                    Logger.LogInformation("Received updated content from TNO, but SourceContentOverwrite is disabled : {Source}:{Title}", model.Source, model.Title);
                }
            }
        }

        if (content == null || updateSourceContent)
        {
            // TODO: Failures after receiving the message from Kafka will result in missing content.  Need to handle this scenario.
            // TODO: Handle e-tag.
            var source = await this.Api.GetSourceForCodeAsync(model.Source);
            var lookups = await this.Api.GetLookupsAsync();

            IEnumerable<API.Areas.Editor.Models.Action.ActionModel>? actions = lookups?.Actions;
            IEnumerable<API.Areas.Editor.Models.Tag.TagModel>? tags = lookups?.Tags;
            IEnumerable<API.Areas.Editor.Models.TonePool.TonePoolModel>? tonePools = lookups?.TonePools;
            IEnumerable<API.Areas.Editor.Models.Topic.TopicModel>? topics = lookups?.Topics;
            IEnumerable<API.Areas.Editor.Models.Series.SeriesModel>? series = lookups?.Series;

            if (model.MediaTypeId == 0)
            {
                // Messages in Kafka are missing information, replace with best guess.
                var ingests = await this.Api.GetIngestsForTopicAsync(result.Topic);
                model.MediaTypeId = ingests.FirstOrDefault()?.MediaTypeId ?? throw new InvalidOperationException($"Unable to find an ingest for the topic '{result.Topic}'");
            }

            content = new ContentModel()
            {
                Status = model.Status,
                SourceId = source?.Id,
                OtherSource = model.Source,
                ContentType = model.ContentType,
                MediaTypeId = model.MediaTypeId,
                LicenseId = source?.LicenseId ?? 1,  // TODO: Default license by configuration.
                SeriesId = null, // TODO: Provide default series from Data Source config settings.
                OtherSeries = null, // TODO: Provide default series from Data Source config settings.
                OwnerId = model.RequestedById ?? source?.OwnerId,
                Headline = model.Title,
                Uid = model.Uid,
                Page = model.Page[0..Math.Min(model.Page.Length, 10)], // TODO: Temporary workaround to deal FileMonitor Service.
                Summary = String.IsNullOrWhiteSpace(model.Summary) ? "" : model.Summary,
                Body = !String.IsNullOrWhiteSpace(model.Body) ? model.Body : model.ContentType == ContentType.AudioVideo ? "" : model.Summary,
                SourceUrl = model.Link,
                PublishedOn = model.PublishedOn,
                Section = model.Section,
                Byline = string.Join(",", model.Authors.Select(a => a.Name[0..Math.Min(a.Name.Length, 200)])) // TODO: Temporary workaround to deal with regression issue in Syndication Service.
            };

            if (model.Actions.Any())
            {
                IEnumerable<ContentActionModel> mappedContentActionModels = GetActionMappings(actions!, model.Actions, existingContentId ?? 0);
                if (mappedContentActionModels.Any())
                {
                    content.Actions = mappedContentActionModels.ToArray();
                }
            }

            if (model.Labels.Any())
            {
                content.Labels = model.Labels.Select(c => new ContentLabelModel(c.Key, c.Value));
            }

            if (!string.IsNullOrEmpty(model.Series))
            {
                var targetSeries = GetSeriesModel(series!, model.Series);
                if (targetSeries == null)
                {
                    content.OtherSeries = model.Series;
                }
                else
                {
                    content.Series = targetSeries;
                    content.SeriesId = targetSeries.Id;
                }
            }

            if (model.Tags.Any())
            {
                var mappedContentTagModels = new List<ContentTagModel>();
                foreach (var tag in model.Tags)
                {
                    var mapping = GetTagMapping(tags!, tag.Key, tag.Value);
                    if (mapping != null)
                    {
                        mappedContentTagModels.Add(mapping);
                    }
                }
                if (mappedContentTagModels.Any())
                {
                    content.Tags = mappedContentTagModels.ToArray();
                }
            }

            if (model.TonePools.Any())
            {
                var mappedTonePools = new List<ContentTonePoolModel>();
                foreach (var tonePool in model.TonePools)
                {
                    var mapping = GetTonePoolMapping(tonePools!, (int)tonePool.Value, tonePool.UserIdentifier);
                    if (mapping != null)
                    {
                        mappedTonePools.Add(mapping);
                    }
                }
                if (mappedTonePools.Any())
                {
                    content.TonePools = mappedTonePools.ToArray();
                }
            }

            if (model.Topics.Any())
            {
                var mappedContentTopicModels = new List<ContentTopicModel>();
                foreach (var topic in model.Topics)
                {
                    var mapping = GetTopicMapping(topics!, topic.TopicType, topic.Name);
                    if (mapping != null)
                    {
                        mappedContentTopicModels.Add(mapping);
                    }
                }
                if (mappedContentTopicModels.Any())
                {
                    content.Topics = mappedContentTopicModels.ToArray();
                }
            }

            if (model.TimeTrackings.Any())
            {
                content.TimeTrackings = model.TimeTrackings.Select(t => new API.Areas.Services.Models.Content.TimeTrackingModel(t.UserId, t.Effort, t.Activity)).ToArray();
            }

            if (updateSourceContent && (existingContentId != null))
            {
                // before saving, reinstate some values from the original content object
                if (originalContent != null)
                {
                    content.Id = originalContent.Id;
                    content.Source = originalContent.Source;
                    content.MediaType = originalContent.MediaType;
                    content.Version = originalContent.Version;
                    content.CreatedBy = originalContent.CreatedBy;
                    content.CreatedOn = originalContent.CreatedOn;
                    content.UpdatedBy = originalContent.UpdatedBy;
                    content.UpdatedOn = originalContent.UpdatedOn;
                }

                content = await this.Api.UpdateContentAsync(content, updateSourceContent) ?? throw new InvalidOperationException($"Updating content failed {content.OtherSource}:{content.Uid}");
                this.Logger.LogInformation("Content Updated.  Content ID: {id}, Pub: {published}", content.Id, content.PublishedOn);
            }
            else
            {
                content = await this.Api.AddContentAsync(content) ?? throw new InvalidOperationException($"Adding content failed {content.OtherSource}:{content.Uid}");
                this.Logger.LogInformation("Content Imported.  Content ID: {id}, Pub: {published}", content.Id, content.PublishedOn);
            }

            var isUploadSuccess = true;

            // Upload the file to the API.
            if (!String.IsNullOrWhiteSpace(model.FilePath))
            {
                // A service needs to know it's context so that it can import files.
                // If this service is running in the same location as the content it will be local.
                var dataLocation = this.Options.DataLocation == model.DataLocation ? null : (await this.Api.GetDataLocationAsync(model.DataLocation))
                    ?? throw new ConfigurationException("Service data location is not configured correctly");
                try
                {
                    // TODO: It isn't ideal to copy files via the API as large files will block this service.  Need to figure out a more performant process at some point.
                    content = (dataLocation?.Connection?.ConnectionType) switch
                    {
                        ConnectionType.NAS => throw new NotImplementedException(),
                        ConnectionType.HTTP => throw new NotImplementedException(),
                        ConnectionType.FTP => throw new NotImplementedException(),
                        ConnectionType.SFTP => throw new NotImplementedException(),
                        ConnectionType.Azure => throw new NotImplementedException(),
                        ConnectionType.AWS => throw new NotImplementedException(),
                        ConnectionType.SSH => await CopyFileWithSSHAsync(dataLocation, model, content),
                        ConnectionType.LocalVolume => await CopyFileFromLocalVolumeAsync(model, content),
                        _ => await CopyFileFromLocalVolumeAsync(model, content),
                    };
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Exception while uploading the file '{path}' to the API.", model.FilePath);
                    isUploadSuccess = false;
                }
            }

            // Update the status of the content reference - after re-fetching it.
            var reference = await this.Api.FindContentReferenceAsync(model.Source, model.Uid);
            if (reference != null)
            {
                try
                {
                    var newStatus = isUploadSuccess ? (int)WorkflowStatus.Imported : (int)WorkflowStatus.Failed;
                    if (reference.Status != newStatus)
                    {
                        reference.Status = newStatus;
                        await Api.UpdateContentReferenceAsync(reference);
                    }
                }
                catch
                {
                    Logger.LogError("{class}.{method}: [version: {version};]",
                        nameof(ContentManager), nameof(HandleMessageAsync),
                        reference.Version);
                    throw;
                }
            }
            else
                this.Logger.LogWarning("Content reference is missing {source}:{uid}", content.OtherSource, content.Uid);

            if (!isUploadSuccess) return;
        }
        else
        {
            // TODO: Content could be updated by source, however this could overwrite local editor changes.  Need a way to handle this.
            this.Logger.LogWarning("Content already exists. Content Source: {source}, UID: {uid}", model.Source, model.Uid);
        }
        this.Logger.LogDebug($"ProcessSourceContentAsync:END:{result.Message.Key}");
    }

    /// <summary>
    /// Copy the files from the local volume storage to the API.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    private async Task<ContentModel> CopyFileFromLocalVolumeAsync(SourceContent model, ContentModel content)
    {
        var fullPath = this.Options.VolumePath.CombineWith(model.FilePath.MakeRelativePath());
        if (File.Exists(fullPath))
        {
            var file = File.OpenRead(fullPath);
            var fileName = Path.GetFileName(fullPath);
            return await this.Api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName) ?? content;
        }
        else
        {
            // TODO: Not sure if this should be considered a failure or not.
            this.Logger.LogWarning("File not found.  Content ID: {id}, File: {path}", content.Id, fullPath);
        }
        return content;
    }

    /// <summary>
    /// Connect to remote location via SSH and copy files to the API.
    /// </summary>
    /// <param name="dataLocation"></param>
    /// <param name="model"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    private async Task<ContentModel> CopyFileWithSSHAsync(DataLocationModel dataLocation, SourceContent model, ContentModel content)
    {
        var connection = dataLocation.Connection ?? throw new ConfigurationException("The data location is missing connection information");
        var hostname = connection.GetConfigurationValue("hostname");
        var username = connection.GetConfigurationValue("username");
        var password = connection.GetConfigurationValue("password");
        var keyFileName = connection.GetConfigurationValue("keyFileName");
        var path = connection.GetConfigurationValue("path");

        AuthenticationMethod authMethod;
        if (!String.IsNullOrWhiteSpace(password))
        {
            authMethod = new PasswordAuthenticationMethod(username, password);
        }
        else if (!String.IsNullOrWhiteSpace(keyFileName))
        {
            var sshKeyFile = this.Options.PrivateKeysPath.CombineWith(keyFileName);
            if (File.Exists(sshKeyFile))
            {
                var keyFile = new PrivateKeyFile(sshKeyFile);
                var keyFiles = new[] { keyFile };
                authMethod = new PrivateKeyAuthenticationMethod(username, keyFiles);
            }
            else
            {
                throw new ConfigurationException($"SSH Private key file does not exist: {sshKeyFile}");
            }
        }
        else throw new ConfigurationException("Data location connection settings are missing");

        using var client = new SftpClient(new ConnectionInfo(hostname, username, authMethod));
        client.HostKeyReceived += (object? sender, HostKeyEventArgs e) =>
        {
            e.CanTrust = true;
        };
        client.Connect();
        Stream? file = null;
        try
        {
            var fullPath = path.CombineWith(model.FilePath.MakeRelativePath()).Replace("~/", $"{client.WorkingDirectory}/");
            if (client.Exists(fullPath))
            {
                var fileName = Path.GetFileName(fullPath);
                file = client.OpenRead(fullPath);
                content = await this.Api.UploadFileAsync(content.Id, content.Version ?? 0, file, fileName) ?? content;
            }
            else
            {
                this.Logger.LogWarning("File does not exist in data location '{location}', '{path}'", dataLocation.Name, fullPath);
            }
            return content;
        }
        catch
        {
            this.Logger.LogError("Failed to copy file from remote data location");
            throw;
        }
        finally
        {
            client.Disconnect();
            file?.Close();
        }
    }
    #endregion

    #region Helper Methods
    private ContentTonePoolModel? GetTonePoolMapping(IEnumerable<API.Areas.Editor.Models.TonePool.TonePoolModel> tonePoolsLookup, int toneValue, string? userIdentifier)
    {
        // use the "Default" TonePool
        if (string.IsNullOrEmpty(userIdentifier))
        {
            // the "Default" tone should always be present
            if (string.IsNullOrEmpty(this.Options.MigrationOptions!.DefaultTonePool))
                throw new ArgumentException("Default Tone Pool Name cannot be empty");
            var tonePoolDefault = tonePoolsLookup.Where(s => s.Name.Equals(this.Options.MigrationOptions.DefaultTonePool)).FirstOrDefault()
                ?? throw new ArgumentException($"Cannot find Tone Pool with Name [{this.Options.MigrationOptions.DefaultTonePool}]");

            return new ContentTonePoolModel()
            {
                ContentId = 0,
                Value = toneValue,
                Id = tonePoolDefault.Id,
                Name = tonePoolDefault.Name,
                OwnerId = tonePoolDefault.OwnerId
            };
        }
        else
        {
            // map a specific tone pool
            // TODO: lookup OwnerId if UserIdentifier is set
            return null;
        }
    }

    private static ContentTopicModel? GetTopicMapping(IEnumerable<API.Areas.Editor.Models.Topic.TopicModel> topicsLookup, TopicType topicType, string topicName)
    {
        var topicModel = new ContentTopicModel()
        {
            ContentId = 0, // for new content
            Score = 0 // TODO: dig score out of TNO 1.0
        };
        var topic = topicsLookup.Where(s => s.Name.Equals(topicName, StringComparison.InvariantCultureIgnoreCase) && s.TopicType == topicType).FirstOrDefault();
        if (topic != null)
        {
            topicModel.Id = topic.Id;
            topicModel.Name = topic.Name;
            topicModel.TopicType = topic.TopicType;
        }
        else
        {
            // build a new placeholder which will be added at the same time as the Content
            topicModel.Id = 0;
            topicModel.Name = topicName;
            topicModel.TopicType = topicType;
        }
        return topicModel;
    }

    private ContentTagModel? GetTagMapping(IEnumerable<API.Areas.Editor.Models.Tag.TagModel> tagsLookup, string code, string topicName)
    {
        var tagModel = new ContentTagModel()
        {
            ContentId = 0, // for new content
        };
        // Tags come in from the Content Migrator with no name, so we can only match on Code
        var tag = tagsLookup.Where(s => s.Code.Equals(code, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
        if (tag != null)
        {
            tagModel.Id = tag.Id;
            tagModel.Code = tag.Code;
            tagModel.Name = tag.Name;
        }
        else
        {
            // build a new placeholder which will be added at the same time as the Content
            tagModel.Id = 0;
            tagModel.Code = code;
            tagModel.Name = this.Options.MigrationOptions.DefaultTagName ?? string.Empty;
        }
        return tagModel;
    }

    private TNO.API.Areas.Services.Models.Content.SeriesModel? GetSeriesModel(IEnumerable<API.Areas.Editor.Models.Series.SeriesModel> seriesLookup, string seriesName)
    {
        var targetSeries = seriesLookup.FirstOrDefault(s => s.Name == seriesName);

        if (targetSeries == null) return null;

        return new TNO.API.Areas.Services.Models.Content.SeriesModel()
        {
            Id = targetSeries.Id,
            Description = targetSeries.Description,
            IsEnabled = targetSeries.IsEnabled,
            Name = targetSeries.Name,
            SortOrder = targetSeries.SortOrder,
            UseInTopics = targetSeries.UseInTopics,
            SourceId = targetSeries.SourceId
        };
    }

    private static IEnumerable<ContentActionModel> GetActionMappings(IEnumerable<API.Areas.Editor.Models.Action.ActionModel> actionsLookup,
        IEnumerable<Kafka.Models.Action> actions, long contentId)
    {
        List<ContentActionModel> mappedActions = new();
        foreach (var action in actions)
        {
            var targetAction = actionsLookup.Where(a => a.Name.Equals(action.ActionLabel, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (targetAction != null)
            {
                mappedActions.Add(new ContentActionModel
                {
                    ContentId = contentId,
                    Id = targetAction.Id,
                    Name = targetAction.Name,
                    Value = action.ActionValue,
                    ValueType = targetAction.ValueType
                });
            }
        }
        return mappedActions;
    }

    #endregion
}
