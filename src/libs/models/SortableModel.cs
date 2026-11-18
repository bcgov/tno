using TNO.Entities;

namespace TNO.API.Models;

/// <summary>
/// SortableModel class, provides a model that represents a sortable model.
/// </summary>
public class SortableModel<TKey> : BaseTypeModel<TKey>
    where TKey : notnull
{
    #region Properties
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SortableModel.
    /// </summary>
    public SortableModel() { }

    /// <summary>
    /// Creates a new instance of an SortableModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public SortableModel(BaseType<TKey>? entity) : base(entity)
    {
    }
    #endregion
}
