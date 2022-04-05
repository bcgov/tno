using System.Security.Claims;
using TNO.Core.Extensions;
using TNO.Entities;

namespace TNO.API.Models.Auth;

/// <summary>
/// EnvModel class, provides a model to represent a user.
/// </summary>
public class PrincipalModel
{
    #region Properties
    /// <summary>
    /// get/set - Primary key to identify user.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public Guid? Key { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public DateTime? LastLoginOn { get; set; }

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set -
    /// </summary>
    public IEnumerable<string> Claims { get; set; } = Array.Empty<string>();
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
        this.Claims = principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        this.Roles = principal.Claims.Where(c => c.Type == "group").Select(c => c.Value);
    }
    #endregion
}
