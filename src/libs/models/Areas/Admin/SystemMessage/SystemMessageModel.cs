using TNO.API.Models;

namespace TNO.API.Areas.Admin.Models.SystemMessage;

/// <summary>
/// SystemMessageModel class, provides a model that represents an system message.
/// </summary>
public class SystemMessageModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Message { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an SystemMessageModel.
    /// </summary>
    public SystemMessageModel() { }

    public SystemMessageModel(Entities.SystemMessage entity) : base(entity)
    {
        this.Id = entity.Id;
        this.Name = entity.Name;
        this.Description = entity.Description;
        this.Message = entity.Message;
        this.SortOrder = entity.SortOrder;
        this.IsEnabled = entity.IsEnabled;
    }
    #endregion

    #region Methods

    /// <summary>
    /// Explicit conversion to entity.
    /// </summary>
    /// <param name="model"></param>
    public static explicit operator Entities.SystemMessage(SystemMessageModel model)
    {
        var entity = new Entities.SystemMessage(model.Message, model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}
