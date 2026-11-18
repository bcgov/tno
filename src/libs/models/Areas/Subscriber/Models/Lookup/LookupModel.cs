using System.Text.Json;
using TNO.API.Areas.Admin.Models.Setting;
using TNO.API.Areas.Subscriber.Models.Content;

namespace TNO.API.Areas.Subscriber.Models.Lookup;

/// <summary>
/// LookupModel class, provides a model that contains all lookup values.
/// </summary>
public class LookupModel
{
    #region Properties
    /// <summary>
    /// get/set - An array of all actions.
    /// </summary>
    public IEnumerable<Action.ActionModel> Actions { get; set; } = Array.Empty<Action.ActionModel>();

    /// <summary>
    /// get/set - An array of all ministers.
    /// </summary>
    public IEnumerable<Minister.MinisterModel> Ministers { get; set; } = Array.Empty<Minister.MinisterModel>();

    /// <summary>
    /// get/set - An array of all contributors.
    /// </summary>
    public IEnumerable<ContributorModel> Contributors { get; set; } = Array.Empty<ContributorModel>();

    /// <summary>
    /// get/set - An array of all topics.
    /// </summary>
    public IEnumerable<Topic.TopicModel> Topics { get; set; } = Array.Empty<Topic.TopicModel>();

    /// <summary>
    /// get/set - An array of all media types.
    /// </summary>
    public IEnumerable<MediaType.MediaTypeModel> MediaTypes { get; set; } = Array.Empty<MediaType.MediaTypeModel>();

    /// <summary>
    /// get/set - An array of all sources.
    /// </summary>
    public IEnumerable<Source.SourceModel> Sources { get; set; } = Array.Empty<Source.SourceModel>();

    /// <summary>
    /// get/set - An array of all licenses.
    /// </summary>
    public IEnumerable<License.LicenseModel> Licenses { get; set; } = Array.Empty<License.LicenseModel>();

    /// <summary>
    /// get/set - An array of all series.
    /// </summary>
    public IEnumerable<Series.SeriesModel> Series { get; set; } = Array.Empty<Series.SeriesModel>();

    /// <summary>
    /// get/set - An array of all tags.
    /// </summary>
    public IEnumerable<Tag.TagModel> Tags { get; set; } = Array.Empty<Tag.TagModel>();

    /// <summary>
    /// get/set - An array of all settings.
    /// </summary>
    public IEnumerable<SettingModel> Settings { get; set; } = Array.Empty<SettingModel>();

    /// <summary>
    /// get/set - An array of all tone pools.
    /// </summary>
    public IEnumerable<TonePool.TonePoolModel> TonePools { get; set; } = Array.Empty<TonePool.TonePoolModel>();

    /// <summary>
    /// get/set - An array of all system messages.
    /// </summary>
    public IEnumerable<SystemMessage.SystemMessageModel> SystemMessages { get; set; } = Array.Empty<SystemMessage.SystemMessageModel>();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupModel.
    /// </summary>
    public LookupModel() { }

    /// <summary>
    /// Creates a new instance of a LookupModel, initialize with specified parameters.
    /// </summary>
    /// <param name="actions"></param>
    /// <param name="topics"></param>
    /// <param name="mediaTypes"></param>
    /// <param name="sources"></param>
    /// <param name="license"></param>
    /// <param name="series"></param>
    /// <param name="tagServices"></param>
    /// <param name="settings"></param>
    /// <param name="tonePools"></param>
    /// <param name="ministers"></param>
    /// <param name="contributors"></param>
    /// <param name="systemMessages"></param>
    /// <param name="options"></param>

    public LookupModel(
        IEnumerable<Entities.Action> actions,
        IEnumerable<Entities.Topic> topics,
        IEnumerable<Entities.MediaType> mediaTypes,
        IEnumerable<Entities.Source> sources,
        IEnumerable<Entities.License> license,
        IEnumerable<Entities.Series> series,
        IEnumerable<Entities.Tag> tagServices,
        IEnumerable<Entities.Setting> settings,
        IEnumerable<Entities.TonePool> tonePools,
        IEnumerable<Entities.Minister> ministers,
        IEnumerable<Entities.Contributor> contributors,
        IEnumerable<Entities.SystemMessage> systemMessages,
        JsonSerializerOptions options)
    {
        this.Actions = actions.Select(a => new Action.ActionModel(a));
        this.Topics = topics.Select(a => new Topic.TopicModel(a));
        this.MediaTypes = mediaTypes.Select(a => new MediaType.MediaTypeModel(a));
        this.Sources = sources.Select(a => new Source.SourceModel(a, options));
        this.Licenses = license.Select(a => new License.LicenseModel(a));
        this.Series = series.Select(a => new Series.SeriesModel(a));
        this.Tags = tagServices.Select(a => new Tag.TagModel(a));
        this.Settings = settings.Select(a => new SettingModel(a));
        this.TonePools = tonePools.Select(a => new TonePool.TonePoolModel(a));
        this.Ministers = ministers.Select(a => new Minister.MinisterModel(a));
        this.Contributors = contributors.Select(a => new ContributorModel(a));
        this.SystemMessages = systemMessages.Select(a => new SystemMessage.SystemMessageModel(a));
    }
    #endregion
}
