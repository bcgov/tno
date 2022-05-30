using System.Security.Claims;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.API.Models.Auth;

/// <summary>
/// PrincipalModel class, provides a model to represent a user.
/// </summary>
public class PrincipalModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set - Uniqiue key to link to Keycloak.
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    /// get/set - Unique sername to identify user.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// get/set - Friendly name to display.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// get/set - User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// get/set - User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// get/set - The last time the user logged in.
    /// </summary>
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// get/set - The user status.
    /// </summary>
    public UserStatus? Status { get; set; }

    /// <summary>
    /// get/set - A note.
    /// </summary>
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - Keycloak groups, TNO roles.
    /// </summary>
    public IEnumerable<string> Groups { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - Keycloak roles, TNO claims.
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a PrincipalModel object.
    /// </summary>
    public PrincipalModel() { }

    /// <summary>
    /// Creates a new instance of a PrincipalModel object, initializes it with specified arguments.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="user"></param>
    public PrincipalModel(ClaimsPrincipal principal, User? user)
    {
        this.Id = user?.Id ?? 0;
        this.Key = principal.GetUid();
        this.Username = principal.GetUsername();
        this.DisplayName = principal.GetDisplayName();
        this.Email = principal.GetEmail();
        this.FirstName = principal.GetFirstName();
        this.LastName = principal.GetLastName();
        this.Status = user?.Status;
        this.Note = user?.Note ?? "";
        this.Groups = user?.RolesManyToMany.Where(r => r.Role != null).Select(r => r.Role!.Name) ?? principal.Claims.Where(c => c.Type == "group").Select(c => c.Value);
        this.Roles = user?.RolesManyToMany.Where(r => r.Role != null).Select(r => r.Role!).SelectMany(r => r.Claims).Select(c => c.Name).Distinct() ?? principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
    }
    #endregion
}
