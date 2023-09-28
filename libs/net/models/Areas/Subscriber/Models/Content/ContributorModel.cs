using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.Content;

/// <summary>
/// ContributorModel class, provides a model that represents an contributor.
/// </summary>
public class ContributorModel : BaseTypeModel<int>
{
    #region Properties

    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - The source.
    /// </summary>
    public SourceModel? Source { get; set; }

    /// <summary>
    /// get/set - Whether content should be automatically transcribed.
    /// </summary>
    public bool AutoTranscribe { get; set; }

    /// <summary>
    /// get/set - Contributor's aliases.
    /// </summary>
    public string Aliases { get; set; } = "";

    /// <summary>
    /// get/set - Whether the contributor is associated with the press gallery.
    /// </summary>
    public bool IsPress { get; set; }

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ContributorModel.
    /// </summary>
    public ContributorModel() { }

    /// <summary>
    /// Creates a new instance of an ContributorModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public ContributorModel(Entities.Contributor entity) : base(entity)
    {
        this.Id = entity.Id;
        this.SourceId = entity.SourceId;
        this.Source = entity.Source != null ? new SourceModel(entity.Source) : null;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
        this.IsPress = entity.IsPress;
        this.Aliases = entity.Aliases;
        this.AutoTranscribe = entity.AutoTranscribe;
    }
    #endregion
}
