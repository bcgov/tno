using TNO.API.Models;

namespace TNO.API.Areas.Services.Models.Content;

/// <summary>
/// SeriesModel class, provides a model that represents an series.
/// </summary>
public class SeriesModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - Foreign key to source.
    /// </summary>
    public int? SourceId { get; set; }

    /// <summary>
    /// get/set - Whether to show the topics on the content form.
    /// </summary>
    public bool UseInTopics { get; set; }

    /// <summary>
    /// get/set - Whether this is an 'other' series.
    /// </summary>
    public bool IsOther { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SeriesModel.
    /// </summary>
    public SeriesModel() { }

    /// <summary>
    /// Creates a new instance of an SeriesModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SeriesModel(Entities.Series entity) : base(entity)
    {
        this.SourceId = entity.SourceId;
        this.UseInTopics = entity.UseInTopics;
        this.IsOther = entity.IsOther;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Explicit cast to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.Series(SeriesModel model)
    {
        return new Entities.Series(model.Name, model.SourceId)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            IsOther = model.IsOther,
            UseInTopics = model.UseInTopics,
        };
    }
    #endregion
}
