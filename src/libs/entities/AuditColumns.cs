using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using TNO.Core.Extensions;

namespace TNO.Entities;

/// <summary>
/// AuditColumns abstract class, provides common audit columns for entities.
/// </summary>
public abstract class AuditColumns : ISaveChanges
{
    /// <summary>
    /// get/set - A way to identify who created the record.
    /// </summary>
    [Column("created_by")]
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// get/set - When the record was created.
    /// </summary>
    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// get/set - A way to identify who last updated the record.
    /// </summary>
    [Column("updated_by")]
    public string UpdatedBy { get; set; } = "";

    /// <summary>
    /// get/set - The last time the record was updated.
    /// </summary>
    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }

    /// <summary>
    /// get/set - The concurrency version value for the row.
    /// </summary>
    [Column("version")]
    public long Version { get; set; }

    #region Methods
    /// <summary>
    /// Apply the user information to the record.
    /// </summary>
    /// <param name="user"></param>
    public void OnAdded(User user)
    {
        var now = DateTime.UtcNow;
        this.CreatedBy = user.Username;
        this.CreatedOn = now;
        this.UpdatedBy = user.Username;
        this.UpdatedOn = now;
        this.Version = 0;
    }

    /// <summary>
    /// Apply the user information to the record.
    /// </summary>
    /// <param name="user"></param>
    public void OnAdded(ClaimsPrincipal? user)
    {
        var now = DateTime.UtcNow;
        this.CreatedBy = user?.GetUsername() ?? "";
        this.CreatedOn = now;
        this.UpdatedBy = user?.GetUsername() ?? "";
        this.UpdatedOn = now;
        this.Version = 0;
    }

    /// <summary>
    /// Apply the user information to the record.
    /// </summary>
    /// <param name="user"></param>
    public void OnModified(User user)
    {
        this.UpdatedBy = user.Username;
        this.UpdatedOn = DateTime.UtcNow;
        this.Version++;
    }

    /// <summary>
    /// Apply the user information to the record.
    /// </summary>
    /// <param name="user"></param>
    public void OnModified(ClaimsPrincipal? user)
    {
        this.UpdatedBy = user?.GetUsername() ?? "";
        this.UpdatedOn = DateTime.UtcNow;
        this.Version++;
    }
    #endregion
}
