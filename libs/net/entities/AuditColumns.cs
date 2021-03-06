using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using TNO.Core.Extensions;

namespace TNO.Entities;

/// <summary>
/// AuditColumns abstract class, provides common audit columns for entities.
/// </summary>
public abstract class AuditColumns : ISaveChanges
{
    [Column("created_by_id")]
    public Guid CreatedById { get; set; }

    [Column("created_by")]
    public string CreatedBy { get; set; } = "";

    [Column("created_on")]
    public DateTime CreatedOn { get; set; }

    [Column("updated_by_id")]
    public Guid UpdatedById { get; set; }

    [Column("updated_by")]
    public string UpdatedBy { get; set; } = "";

    [Column("updated_on")]
    public DateTime UpdatedOn { get; set; }

    /// <summary>
    /// get/set - The concurrency version value for the row.
    /// </summary>
    [Column("version")]
    public long Version { get; set; }

    #region Methods
    public void OnAdded(User user)
    {
        var now = DateTime.UtcNow;
        this.CreatedById = user.Key;
        this.CreatedBy = user.Username;
        this.CreatedOn = now;
        this.UpdatedById = user.Key;
        this.UpdatedBy = user.Username;
        this.UpdatedOn = now;
        this.Version = 0;
    }

    public void OnAdded(ClaimsPrincipal? user)
    {
        var now = DateTime.UtcNow;
        this.CreatedById = user?.GetUid() ?? Guid.Empty;
        this.CreatedBy = user?.GetUsername() ?? "";
        this.CreatedOn = now;
        this.UpdatedById = user?.GetUid() ?? Guid.Empty;
        this.UpdatedBy = user?.GetUsername() ?? "";
        this.UpdatedOn = now;
        this.Version = 0;
    }

    public void OnModified(User user)
    {
        this.UpdatedById = user.Key;
        this.UpdatedBy = user.Username;
        this.UpdatedOn = DateTime.UtcNow;
        this.Version++;
    }

    public void OnModified(ClaimsPrincipal? user)
    {
        this.UpdatedById = user?.GetUid() ?? Guid.Empty;
        this.UpdatedBy = user?.GetUsername() ?? "";
        this.UpdatedOn = DateTime.UtcNow;
        this.Version++;
    }
    #endregion
}
