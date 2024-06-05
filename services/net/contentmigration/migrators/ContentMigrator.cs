using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Areas.Editor.Models.MediaType;
using TNO.API.Areas.Editor.Models.Source;
using TNO.API.Areas.Editor.Models.Topic;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.Entities;
using TNO.Kafka.Models;
using TNO.Services.ContentMigration.Config;
using TNO.Services.ContentMigration.Extensions;
using TNO.Services.ContentMigration.Models;
using TNO.Services.ContentMigration.Sources.Oracle;

namespace TNO.Services.ContentMigration.Migrators;

/// <summary>
///
/// </summary>
/// <typeparam name="TOptions"></typeparam>
public abstract class ContentMigrator<TOptions> : IContentMigrator
    where TOptions : ContentMigrationOptions
{
    #region Properties
    /// <summary>
    /// get - The service to communicate with the api.
    /// </summary>
    public IApiService Api { get; private set; }

    /// <summary>
    /// get - The ingest configuration options.
    /// </summary>
    public TOptions Options { get; private set; }

    /// <summary>
    /// get/set - Logger.
    /// </summary>
    protected ILogger Logger { get; private set; }

    /// <summary>
    /// stores options specific to this ContentMigrator
    /// </summary>
    protected MigratorOptions MigratorOptions { get; private set; }

    /// <summary>
    /// which Ingests this Migrator supports
    /// </summary>
    public IEnumerable<string> SupportedIngests
    {
        get
        {
            return this.MigratorOptions.SupportedIngests;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance of a ServiceAction object, initializes with specified parameters.
    /// </summary>
    /// <param name="api"></param>
    /// <param name="migratorOptions"></param>
    /// <param name="options"></param>
    /// <param name="logger"></param>
    public ContentMigrator(IApiService api, IOptionsSnapshot<MigratorOptions> migratorOptions, IOptions<TOptions> options, ILogger<ContentMigrator<TOptions>> logger)
    {
        this.Api = api;
        this.Options = options.Value;
        this.Logger = logger;
        // as a convention the Named Option should be the same as the class name which needs to consume it
        this.MigratorOptions = migratorOptions.Get(this.GetType().Name);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets the base filter for excluding items from the dbContext search
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="contentType"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual Expression<Func<T, bool>> GetBaseFilter<T>(ContentType contentType)
        where T : BaseNewsItem => throw new NotImplementedException();

    /// <summary>
    /// Creates a SourceContent item
    /// </summary>
    /// <param name="lookups"></param>
    /// <param name="source"></param>
    /// <param name="mediaType"></param>
    /// <param name="contentType"></param>
    /// <param name="newsItem"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    public virtual SourceContent CreateSourceContent(LookupModel lookups, SourceModel source, MediaTypeModel mediaType, ContentType contentType, NewsItem newsItem, string defaultTimeZone) => throw new NotImplementedException();

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates an ContentReferenceModel from a NewsItem
    /// </summary>
    /// <param name="source"></param>
    /// <param name="topic"></param>
    /// <param name="newsItem"></param>
    /// <param name="uid"></param>
    /// <param name="defaultTimeZone"></param>
    /// <returns></returns>
    public ContentReferenceModel CreateContentReference(SourceModel source, string topic, NewsItem newsItem, string uid, string defaultTimeZone)
    {
        return new ContentReferenceModel()
        {
            Source = source.Code,
            Uid = uid,
            PublishedOn = this.GetSourceDateTime(newsItem.GetPublishedDateTime(), defaultTimeZone).ToUniversalTime(),
            Topic = topic,
            Status = (int)WorkflowStatus.InProgress,
            Metadata = new Dictionary<string, object> {
                { ContentReferenceMetaDataKeys.MetadataKeyOriginalIngestSource, "TNO" },
                { ContentReferenceMetaDataKeys.MetadataKeyIngestSource, source!.Code },
                { ContentReferenceMetaDataKeys.MetadataKeyUpdatedOn, newsItem.UpdatedOn.HasValue ? this.GetSourceDateTime(newsItem.UpdatedOn.Value, defaultTimeZone).ToUniversalTime().ToString("yyyy-MM-dd h:mm:ss tt") : DateTime.MinValue },
                { ContentReferenceMetaDataKeys.MetadataKeyIsContentPublished, newsItem.Published.ToString() },
            }
        };
    }

    /// <summary>
    /// Get mapped topics from lookup
    /// </summary>
    /// <param name="topics"></param>
    /// <param name="newsItemTopic"></param>
    /// <returns></returns>
    public TopicModel? GetTopicMapping(IEnumerable<TopicModel> topics, string? newsItemTopic)
    {
        return topics.Where(s => s.Name == newsItemTopic).FirstOrDefault();
    }

    /// <summary>
    /// Extracts Authors from a source string, using any combination of delimters plus the word " and "
    /// </summary>
    /// <param name="authors"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static IEnumerable<string> ExtractAuthors(string authors, string? source)
    {
        string[] delimiters = new[] { ",", ";", " ,", " & ", " and " };
        var splitArray = authors.Split(delimiters, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return splitArray.Where(s => !s.Equals(source));
    }

    /// <summary>
    /// extracts Tags from a source string
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    internal static IEnumerable<string> ExtractTags(string source)
    {
        Regex tagsBetweenBracketsRegex = new(@"\[([^\]]*)\]", RegexOptions.RightToLeft);
        string[] tags = Array.Empty<string>();
        var tagMatches = tagsBetweenBracketsRegex.Matches(source);
        if (tagMatches.Count > 0)
        {
            var rawTags = tagMatches[0].Groups[1].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            // KGM : Fail on tags extract if any of the tags are not between 3 and 4 chars long
            // Do we need to make this validation better?
            if (rawTags.All(s => s.Length >= 3 && s.Length <= 4))
                tags = rawTags;
        }
        return tags.Distinct();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemSource"></param>
    /// <returns></returns>
    public SourceModel? GetSourceMapping(IEnumerable<SourceModel> lookup, string? newsItemSource)
    {
        var source = lookup.FirstOrDefault(s => s.Name.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase)
                                                 || s.Code.Equals(newsItemSource, StringComparison.InvariantCultureIgnoreCase));

        // if the Name doesn't match one of our sources, use the extra mappings from the config
        if (source == null && newsItemSource != null)
        {
            this.MigratorOptions.IngestSourceMappings.TryGetValue(newsItemSource, out string? customMapping);
            source = lookup.Where(s => s.Code == customMapping).FirstOrDefault();
        }

        return source;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="lookup"></param>
    /// <param name="newsItemMediaType"></param>
    /// <returns></returns>
    public MediaTypeModel? GetMediaTypeMapping(IEnumerable<MediaTypeModel> lookup, string newsItemMediaType)
    {
        // TODO: KGM - what to do if we have no mapping - make nullable so we can skip it on migration
        MediaTypeModel? mediaType = lookup.Where(s => s.Name == newsItemMediaType).FirstOrDefault();

        // if the Name doesn't match one of our media types, use the extra mappings from the config
        if (mediaType == null)
        {
            this.MigratorOptions.MediaTypeMappings.TryGetValue(newsItemMediaType, out string? customMapping);
            mediaType = lookup.Where(s => s.Name == customMapping).FirstOrDefault();
        }

        return mediaType;
    }

    /// <summary>
    /// map possible Action values to Actions
    /// </summary>
    /// <param name="frontPageStory"></param>
    /// <param name="wapTopStory"></param>
    /// <param name="alert"></param>
    /// <param name="commentary"></param>
    /// <param name="commentaryTimeout"></param>
    /// <returns></returns>
    public IEnumerable<Kafka.Models.Action> GetActionMappings(bool frontPageStory, bool wapTopStory, bool alert, bool commentary, double? commentaryTimeout)
    {
        List<Kafka.Models.Action> mappedActions = new();
        string actionName;
        ActionType actionType;

        if (frontPageStory)
        {
            actionType = ActionType.Homepage;
            actionName = this.Options.ActionNameMappings.ContainsKey(actionType)
                ? this.Options.ActionNameMappings[actionType]
                : actionType.ToString();
            mappedActions.Add(new Kafka.Models.Action(actionName, Boolean.TrueString.ToLower()));
        }

        if (wapTopStory)
        {
            actionType = ActionType.TopStory;
            actionName = this.Options.ActionNameMappings.ContainsKey(actionType)
                ? this.Options.ActionNameMappings[actionType]
                : actionType.ToString();
            mappedActions.Add(new Kafka.Models.Action(actionName, Boolean.TrueString.ToLower()));
        }

        // KGM: 2023-09-25 - Added an option switch for triggering alerts in MMI
        // if content has been flagged with Alert in TNO 1.0
        if (alert && this.Options.GenerateAlertsOnContentMigration)
        {
            actionType = ActionType.Alert;
            actionName = this.Options.ActionNameMappings.ContainsKey(actionType)
                ? this.Options.ActionNameMappings[actionType]
                : actionType.ToString();
            mappedActions.Add(new Kafka.Models.Action(actionName, Boolean.TrueString.ToLower()));
        }

        if (commentary && commentaryTimeout.HasValue)
        {
            actionType = ActionType.Commentary;
            actionName = this.Options.ActionNameMappings.ContainsKey(actionType)
                ? this.Options.ActionNameMappings[actionType]
                : actionType.ToString();
            mappedActions.Add(new Kafka.Models.Action(actionName, commentaryTimeout.Value.ToString()));
        }

        return mappedActions;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <param name="contentStagingFolderName"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public async Task CopyFileAsync(FileMigrationModel request, string contentStagingFolderName)
    {
        var localPath = Path.Combine(this.Options.VolumePath, contentStagingFolderName, request.Path, request.Filename);
        var localDirectory = Path.GetDirectoryName(localPath) ?? throw new ConfigurationException("Local path for Content Migration is invalid");

        if (!Directory.Exists(localDirectory)) Directory.CreateDirectory(localDirectory);

        // we only need to download the file if it's not already staged.
        if (!File.Exists(localPath))
        {
            UriBuilder uriBuilder = new(this.Options.MediaHostRootUri);
            var contentPath = Path.Combine(uriBuilder.ToString(), request.Path, request.Filename).Replace("\\", "/");

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            // client.DefaultRequestHeaders.Add("authorization", access_token); //if any
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(request.ContentType));
            HttpResponseMessage response = await client.GetAsync(contentPath);

            if (response.IsSuccessStatusCode)
            {
                HttpContent content = response.Content;
                var contentStream = await content.ReadAsStreamAsync();
                using var fs = new FileStream(localPath, FileMode.OpenOrCreate);
                await response.Content.CopyToAsync(fs);
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
    }

    /// <summary>
    /// Get the date and time for the source timezone.
    /// </summary>
    /// <param name="date"></param>
    /// <param name="timeZoneId"></param>
    /// <returns></returns>
    internal virtual DateTime GetSourceDateTime(DateTime date, string timeZoneId)
    {
        return date.ToTimeZone(timeZoneId);
    }
    #endregion
}
