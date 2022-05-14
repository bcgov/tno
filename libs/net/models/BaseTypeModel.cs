using TNO.Entities;

namespace TNO.API.Models;

/// <summary>
/// BaseTypeModel abstract class, provides a common model for type objects.
/// </summary>
public abstract class BaseTypeModel<TKey>
    where TKey : notnull
{
    #region Properties
    /// <summary>
    /// get/set - The primary key of the type model.
    /// </summary>
    public TKey Id { get; set; } = default!;

    /// <summary>
    /// get/set - The unique name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// get/set - A description of the type model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// get/set - Whether this model is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The sort order of the models.
    /// </summary>
    public int SortOrder { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an BaseTypeModel.
    /// </summary>
    public BaseTypeModel() { }


    /// <summary>
    /// Creates a new instance of an BaseTypeModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public BaseTypeModel(BaseType<TKey>? entity)
    {
        if (entity != null)
        {
            this.Id = entity.Id;
            this.Name = entity.Name;
            this.Description = entity.Description;
            this.IsEnabled = entity.IsEnabled;
            this.SortOrder = entity.SortOrder;
        }
    }
    #endregion
}
