using System.Text.Json;

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
    /// get/set - An array of all topics.
    /// </summary>
    public IEnumerable<Topic.TopicModel> Topics { get; set; } = Array.Empty<Topic.TopicModel>();

    /// <summary>
    /// get/set - An array of all products.
    /// </summary>
    public IEnumerable<Product.ProductModel> Products { get; set; } = Array.Empty<Product.ProductModel>();

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
    /// get/set - An array of all tone pools.
    /// </summary>
    public IEnumerable<TonePool.TonePoolModel> TonePools { get; set; } = Array.Empty<TonePool.TonePoolModel>();

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
    /// <param name="products"></param>
    /// <param name="sources"></param>
    /// <param name="license"></param>
    /// <param name="series"></param>
    /// <param name="tagServices"></param>
    /// <param name="tonePools"></param>
    /// <param name="ministers"></param>
    /// <param name="options"></param>
    public LookupModel(
        IEnumerable<Entities.Action> actions,
        IEnumerable<Entities.Topic> topics,
        IEnumerable<Entities.Product> products,
        IEnumerable<Entities.Source> sources,
        IEnumerable<Entities.License> license,
        IEnumerable<Entities.Series> series,
        IEnumerable<Entities.Tag> tagServices,
        IEnumerable<Entities.TonePool> tonePools,
        IEnumerable<Entities.Minister> ministers,
        JsonSerializerOptions options)
    {
        this.Actions = actions.Select(a => new Action.ActionModel(a));
        this.Topics = topics.Select(a => new Topic.TopicModel(a));
        this.Products = products.Select(a => new Product.ProductModel(a));
        this.Sources = sources.Select(a => new Source.SourceModel(a, options));
        this.Licenses = license.Select(a => new License.LicenseModel(a));
        this.Series = series.Select(a => new Series.SeriesModel(a));
        this.Tags = tagServices.Select(a => new Tag.TagModel(a));
        this.TonePools = tonePools.Select(a => new TonePool.TonePoolModel(a));
        this.Ministers = ministers.Select(a => new Minister.MinisterModel(a));
    }
    #endregion
}
