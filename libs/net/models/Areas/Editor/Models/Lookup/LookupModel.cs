using System.Text.Json;

namespace TNO.API.Areas.Editor.Models.Lookup;

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
    /// get/set - An array of all topics.
    /// </summary>
    public IEnumerable<Topic.TopicModel> Topics { get; set; } = Array.Empty<Topic.TopicModel>();

    /// <summary>
    /// get/set - An array of all topics score rules.
    /// </summary>
    public IEnumerable<TopicScoreRule.TopicScoreRuleModel> Rules { get; set; } = Array.Empty<TopicScoreRule.TopicScoreRuleModel>();

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
    /// get/set - An array of all ingest types.
    /// </summary>
    public IEnumerable<IngestType.IngestTypeModel> IngestTypes { get; set; } = Array.Empty<IngestType.IngestTypeModel>();

    /// <summary>
    /// get/set - An array of all roles.
    /// </summary>
    public IEnumerable<Role.RoleModel> Roles { get; set; } = Array.Empty<Role.RoleModel>();

    /// <summary>
    /// get/set - An array of all series.
    /// </summary>
    public IEnumerable<Series.SeriesModel> Series { get; set; } = Array.Empty<Series.SeriesModel>();

    /// <summary>
    /// get/set - An array of all contributors.
    /// </summary>
    public IEnumerable<Contributor.ContributorModel> Contributors { get; set; } = Array.Empty<Contributor.ContributorModel>();

    /// <summary>
    /// get/set - An array of all source metrics.
    /// </summary>
    public IEnumerable<Metric.MetricModel> Metrics { get; set; } = Array.Empty<Metric.MetricModel>();

    /// <summary>
    /// get/set - An array of all tags.
    /// </summary>
    public IEnumerable<Tag.TagModel> Tags { get; set; } = Array.Empty<Tag.TagModel>();

    /// <summary>
    /// get/set - An array of all tone pools.
    /// </summary>
    public IEnumerable<TonePool.TonePoolModel> TonePools { get; set; } = Array.Empty<TonePool.TonePoolModel>();

    /// <summary>
    /// get/set - An array of all users.
    /// </summary>
    public IEnumerable<User.UserModel> Users { get; set; } = Array.Empty<User.UserModel>();

    /// <summary>
    /// get/set - An array of all data locations.
    /// </summary>
    public IEnumerable<DataLocation.DataLocationModel> DataLocations { get; set; } = Array.Empty<DataLocation.DataLocationModel>();

    /// <summary>
    /// get/set - An array of all settings.
    /// </summary>
    public IEnumerable<Setting.SettingModel> Settings { get; set; } = Array.Empty<Setting.SettingModel>();

    /// <summary>
    /// get/set - An array of holidays for the current year.
    /// </summary>
    public IEnumerable<HolidayModel> Holidays { get; set; } = Array.Empty<HolidayModel>();
    
    /// <summary>
    /// get/set - An array of all organizations.
    /// </summary>
    public IEnumerable<Organization.OrganizationModel> Organizations { get; set; } = Array.Empty<Organization.OrganizationModel>();
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
    /// <param name="rules"></param>
    /// <param name="claims"></param>
    /// <param name="mediaTypes"></param>
    /// <param name="sources"></param>
    /// <param name="license"></param>
    /// <param name="ingestTypes"></param>
    /// <param name="roles"></param>
    /// <param name="series"></param>
    /// <param name="contributors"></param>
    /// <param name="sourceMetrics"></param>
    /// <param name="tagServices"></param>
    /// <param name="tonePools"></param>
    /// <param name="users"></param>
    /// <param name="dataLocations"></param>
    /// <param name="settings"></param>
    /// <param name="holidays"></param>
    /// <param name="options"></param>
    public LookupModel(
        IEnumerable<Entities.Action> actions,
        IEnumerable<Entities.Topic> topics,
        IEnumerable<Entities.TopicScoreRule> rules,
        IEnumerable<Entities.MediaType> mediaTypes,
        IEnumerable<Entities.Source> sources,
        IEnumerable<Entities.License> license,
        IEnumerable<Entities.IngestType> ingestTypes,
        IEnumerable<string> roles,
        IEnumerable<Entities.Series> series,
        IEnumerable<Entities.Contributor> contributors,
        IEnumerable<Entities.Metric> metrics,
        IEnumerable<Entities.Tag> tagServices,
        IEnumerable<Entities.TonePool> tonePools,
        IEnumerable<Entities.User> users,
        IEnumerable<Entities.DataLocation> dataLocations,
        IEnumerable<Entities.Setting> settings,
        IEnumerable<HolidayModel> holidays,
        JsonSerializerOptions options,
        IEnumerable<Entities.Organization> organizations)
    {
        this.Actions = actions.Select(a => new Action.ActionModel(a));
        this.Topics = topics.Select(a => new Topic.TopicModel(a));
        this.Rules = rules.Select(a => new TopicScoreRule.TopicScoreRuleModel(a));
        this.MediaTypes = mediaTypes.Select(a => new MediaType.MediaTypeModel(a));
        this.Sources = sources.Select(a => new Source.SourceModel(a, options));
        this.Licenses = license.Select(a => new License.LicenseModel(a));
        this.IngestTypes = ingestTypes.Select(a => new IngestType.IngestTypeModel(a));
        this.Roles = roles.Select(a => new Role.RoleModel(a));
        this.Series = series.Select(a => new Series.SeriesModel(a));
        this.Contributors = contributors.Select(a => new Contributor.ContributorModel(a));
        this.Metrics = metrics.Select(a => new Metric.MetricModel(a));
        this.Tags = tagServices.Select(a => new Tag.TagModel(a));
        this.TonePools = tonePools.Select(a => new TonePool.TonePoolModel(a));
        this.Users = users.Select(a => new User.UserModel(a));
        this.DataLocations = dataLocations.Select(a => new DataLocation.DataLocationModel(a));
        this.Settings = settings.Select(a => new Setting.SettingModel(a));
        this.Organizations = organizations.Select(a => new Organization.OrganizationModel(a));
        this.Holidays = holidays;
    }
    #endregion
}
