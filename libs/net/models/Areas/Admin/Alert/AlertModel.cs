using System.Text.Json;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.Alert;

/// <summary>
/// AlertModel class, provides a model that represents an product.
/// </summary>
public class AlertModel : BaseTypeWithAuditColumnsModel<int>
{
    #region Properties
    /// <summary>
    /// get/set -
    /// </summary>
    public string Message { get; set; } = "";
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AlertModel.
    /// </summary>
    public AlertModel() { }

    public AlertModel(Entities.Alert entity) : base(entity)
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
    public static explicit operator Entities.Alert(AlertModel model)
    {
        var entity = new Entities.Alert(model.Message, model.Name)
        {
            Id = model.Id,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            Version = model.Version ?? 0
        };
        return entity;
    }
    #endregion
}