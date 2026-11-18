using System.Security.Claims;
using System.Text.Json;
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
    /// get/set - Unique key to link to Keycloak.
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// get/set - Unique username to identify user.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// get/set - User's email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// get/set - User's preferred email address.
    /// </summary>
    public string? PreferredEmail { get; set; }

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
    /// get/set - Whether the user is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// get/set - The user status.
    /// </summary>
    public UserStatus? Status { get; set; }

    /// <summary>
    /// get/set - A note.
    /// </summary>
    public string Note { get; set; } = "";

    /// <summary>
    /// get/set - Keycloak roles, TNO claims.
    /// </summary>
    public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// get/set - User preferences.
    /// </summary>
    public JsonDocument Preferences { get; set; } = JsonDocument.Parse("{}");

    /// <summary>
    /// get/set - The account authorization state.
    /// </summary>
    public AccountAuthState AuthState { get; set; }

    /// <summary>
    /// get/set - An array of media types not accessible to this user.
    /// </summary>
    public IEnumerable<int> MediaTypes { get; set; } = Array.Empty<int>();

    /// <summary>
    /// get/set - An array of sources not accessible to this user.
    /// </summary>
    public IEnumerable<int> Sources { get; set; } = Array.Empty<int>();

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
    /// <param name="state"></param>

    public PrincipalModel(ClaimsPrincipal principal, User? user, AccountAuthState state)
    {
        this.Id = user?.Id ?? 0;
        this.Key = principal.GetUid();
        this.Username = principal.GetUsername();
        this.DisplayName = principal.GetDisplayName();
        this.Email = principal.GetEmail();
        this.PreferredEmail = user?.PreferredEmail;
        this.FirstName = principal.GetFirstName();
        this.LastName = principal.GetLastName();
        this.Status = user?.Status;
        this.IsEnabled = user?.IsEnabled ?? false;
        this.Note = user?.Note ?? "";
        this.Preferences = user?.Preferences != null ? user.Preferences : JsonDocument.Parse("{}");
        this.LastLoginOn = user?.LastLoginOn;
        this.AuthState = state;
        this.Roles = user?.Roles.Split(",")
            .Where(s => !String.IsNullOrWhiteSpace(s))
            .Select(r => r[1..^1])
            .Distinct() ?? principal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        this.MediaTypes = user?.MediaTypesManyToMany.Select(s => s.MediaTypeId).ToArray() ?? Array.Empty<int>();
        this.Sources = user?.SourcesManyToMany.Select(s => s.SourceId).ToArray() ?? Array.Empty<int>();
    }
    #endregion
}
