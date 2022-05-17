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
    /// get/set - An array of all categories.
    /// </summary>
    public IEnumerable<Category.CategoryModel> Categories { get; set; } = Array.Empty<Category.CategoryModel>();

    /// <summary>
    /// get/set - An array of all claims.
    /// </summary>
    public IEnumerable<Claim.ClaimModel> Claims { get; set; } = Array.Empty<Claim.ClaimModel>();

    /// <summary>
    /// get/set - An array of all content types.
    /// </summary>
    public IEnumerable<ContentType.ContentTypeModel> ContentTypes { get; set; } = Array.Empty<ContentType.ContentTypeModel>();

    /// <summary>
    /// get/set - An array of all data locations.
    /// </summary>
    public IEnumerable<DataLocation.DataLocationModel> DataLocations { get; set; } = Array.Empty<DataLocation.DataLocationModel>();

    /// <summary>
    /// get/set - An array of all data sources.
    /// </summary>
    public IEnumerable<DataSource.DataSourceModel> DataSources { get; set; } = Array.Empty<DataSource.DataSourceModel>();

    /// <summary>
    /// get/set - An array of all licenses.
    /// </summary>
    public IEnumerable<License.LicenseModel> Licenses { get; set; } = Array.Empty<License.LicenseModel>();

    /// <summary>
    /// get/set - An array of all media types.
    /// </summary>
    public IEnumerable<MediaType.MediaTypeModel> MediaTypes { get; set; } = Array.Empty<MediaType.MediaTypeModel>();

    /// <summary>
    /// get/set - An array of all roles.
    /// </summary>
    public IEnumerable<Role.RoleModel> Roles { get; set; } = Array.Empty<Role.RoleModel>();

    /// <summary>
    /// get/set - An array of all series.
    /// </summary>
    public IEnumerable<Series.SeriesModel> Series { get; set; } = Array.Empty<Series.SeriesModel>();

    /// <summary>
    /// get/set - An array of all source actions.
    /// </summary>
    public IEnumerable<SourceAction.SourceActionModel> SourceActions { get; set; } = Array.Empty<SourceAction.SourceActionModel>();

    /// <summary>
    /// get/set - An array of all source metrics.
    /// </summary>
    public IEnumerable<SourceMetric.SourceMetricModel> SourceMetrics { get; set; } = Array.Empty<SourceMetric.SourceMetricModel>();

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
    /// <param name="categories"></param>
    /// <param name="claims"></param>
    /// <param name="contentTypes"></param>
    /// <param name="dataLocations"></param>
    /// <param name="dataSources"></param>
    /// <param name="license"></param>
    /// <param name="mediaTypes"></param>
    /// <param name="roles"></param>
    /// <param name="series"></param>
    /// <param name="sourceActions"></param>
    /// <param name="sourceMetrics"></param>
    /// <param name="tagServices"></param>
    /// <param name="tonePools"></param>
    /// <param name="users"></param>
    /// <param name="options"></param>
    public LookupModel(
        IEnumerable<Entities.Action> actions,
        IEnumerable<Entities.Category> categories,
        IEnumerable<Entities.Claim> claims,
        IEnumerable<Entities.ContentType> contentTypes,
        IEnumerable<Entities.DataLocation> dataLocations,
        IEnumerable<Entities.DataSource> dataSources,
        IEnumerable<Entities.License> license,
        IEnumerable<Entities.MediaType> mediaTypes,
        IEnumerable<Entities.Role> roles,
        IEnumerable<Entities.Series> series,
        IEnumerable<Entities.SourceAction> sourceActions,
        IEnumerable<Entities.SourceMetric> sourceMetrics,
        IEnumerable<Entities.Tag> tagServices,
        IEnumerable<Entities.TonePool> tonePools,
        IEnumerable<Entities.User> users,
        JsonSerializerOptions options)
    {
        this.Actions = actions.Select(a => new Action.ActionModel(a));
        this.Categories = categories.Select(a => new Category.CategoryModel(a));
        this.Claims = claims.Select(a => new Claim.ClaimModel(a));
        this.ContentTypes = contentTypes.Select(a => new ContentType.ContentTypeModel(a));
        this.DataLocations = dataLocations.Select(a => new DataLocation.DataLocationModel(a));
        this.DataSources = dataSources.Select(a => new DataSource.DataSourceModel(a, options));
        this.Licenses = license.Select(a => new License.LicenseModel(a));
        this.MediaTypes = mediaTypes.Select(a => new MediaType.MediaTypeModel(a));
        this.Roles = roles.Select(a => new Role.RoleModel(a));
        this.Series = series.Select(a => new Series.SeriesModel(a));
        this.SourceActions = sourceActions.Select(a => new SourceAction.SourceActionModel(a));
        this.SourceMetrics = sourceMetrics.Select(a => new SourceMetric.SourceMetricModel(a));
        this.Tags = tagServices.Select(a => new Tag.TagModel(a));
        this.TonePools = tonePools.Select(a => new TonePool.TonePoolModel(a));
        this.Users = users.Select(a => new User.UserModel(a));
    }
    #endregion
}
