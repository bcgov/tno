using TNO.API.Models;

namespace TNO.API.Areas.Subscriber.Models.TonePool;

/// <summary>
/// TonePoolModel class, provides a model that represents an tone pool.
/// </summary>
public class TonePoolModel : BaseTypeModel<int>
{
    #region Properties
    /// <summary>
    /// get/set - The user Id who owns this tone pool.
    /// </summary>
    public int OwnerId { get; set; }

    /// <summary>
    /// get/set - Whether this tone pool is publicly shared.
    /// </summary>
    public bool IsPublic { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an TonePoolModel.
    /// </summary>
    public TonePoolModel() { }

    /// <summary>
    /// Creates a new instance of an TonePoolModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public TonePoolModel(Entities.TonePool entity) : base(entity)
    {
        this.OwnerId = entity.OwnerId;
        this.IsPublic = entity.IsPublic;
    }
    #endregion
}
