using System.Text.Json.Serialization;
using TNO.API.Models;
using TNO.Entities;

namespace TNO.API.Areas.Admin.Models.User;

/// <summary>
/// UserUpdateHistoryModel class
/// </summary>
public class UserUpdateHistoryModel : AuditColumnsModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user definition.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// get/set - the user
    /// </summary>
    [JsonIgnore]
    public UserModel? User { get; set; }

    /// <summary>
    /// get/set - The date and time the user is changed.
    /// </summary>
    public DateTime DateOfChange { get; set; }

    /// <summary>
    /// get/set - The user change type.
    /// </summary>
    public UserChangeType ChangeType { get; set; }

    /// <summary>
    /// get/set - The changed new value.
    /// </summary>
    public string Value { get; set; } = "";

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an UserUpdateHistoryModel.
    /// </summary>
    public UserUpdateHistoryModel() { }

    /// <summary>
    /// Creates a new instance of an UserUpdateHistoryModel, initializes with specified parameter.
    /// </summary>
    /// <param name="entity"></param>
    public UserUpdateHistoryModel(UserUpdateHistory entity) : base(entity)
    {
        this.Id = entity.Id;
        this.UserId = entity.UserId;
        this.User = entity.User != null ? new UserModel(entity.User) : null;
        this.DateOfChange = entity.DateOfChange;
        this.ChangeType = entity.ChangeType;
        this.Value = entity.Value;
    }
    #endregion
}
