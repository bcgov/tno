using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TNO.Entities;

/// <summary>
/// User update history
/// </summary>
[Table("user_update_history")]
public class UserUpdateHistory : AuditColumns
{
    #region Properties
    /// <summary>
    /// get/set - Primary key identity.
    /// </summary>
    [Key]
    [Column("id")]
    public long Id { get; set; }

    /// <summary>
    /// get/set - Foreign key to the user definition.
    /// </summary>
    [Column("user_id")]
    public int UserId { get; set; }

    /// <summary>
    /// get/set - The user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// get/set - The date and time of the change
    /// </summary>
    [Column("date_of_change")]
    public DateTime DateOfChange { get; set; }

    /// <summary>
    /// get/set - the change type
    /// </summary>
    [Column("change_type")]
    public UserChangeType ChangeType { get; set; }

    /// <summary>
    /// get/set - the changed new value
    /// </summary>
    [Column("value")]
    public string Value { get; set; } = "";
    #endregion

    #region Constructors

    /// <summary>
    /// Creates a new instance of a UserUpdateHistory object.
    /// </summary>
    protected UserUpdateHistory() { }

    /// <summary>
    /// Creates a new instance of a UserUpdateHistory object, initializes with specified parameters.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="changeType"></param>
    /// <param name="dateOfChange"></param>
    /// <param name="value"></param>
    public UserUpdateHistory(long id, int userId, UserChangeType changeType, DateTime dateOfChange, string value)
    {
        this.Id = id;
        this.UserId = userId;
        this.ChangeType = changeType;
        this.DateOfChange = dateOfChange;
        this.Value = value;
    }
    #endregion
}
