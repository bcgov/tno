using System.Security.Claims;

namespace TNO.Core.Extensions;

public static class IdentityExtensions
{
    /// <summary>
    /// Get the currently logged in user's ClaimTypes.NameIdentifier.
    /// Return an empty Guid if no user is logged in.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static Guid GetUid(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier);
        return String.IsNullOrWhiteSpace(claim?.Value) ? Guid.Empty : new Guid(claim.Value);
    }


    /// <summary>
    /// Get the user's username.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetUsername(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst("username") ?? user?.FindFirst("preferred_username");
        return claim?.Value;
    }

    /// <summary>
    /// Get the user's display name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetDisplayName(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst("name");
        return claim?.Value;
    }

    /// <summary>
    /// Get the user's first name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetFirstName(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst(ClaimTypes.GivenName) ?? user?.FindFirst("given_name");
        return claim?.Value;
    }

    /// <summary>
    /// Get the user's last name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetLastName(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst(ClaimTypes.Surname) ?? user?.FindFirst("family_name");
        return claim?.Value;
    }

    /// <summary>
    /// Get the user's email.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        var claim = user?.FindFirst(ClaimTypes.Email);
        return claim?.Value;
    }

    /// <summary>
    /// Determine if the user any of the specified roles.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="role"></param>
    /// <returns>True if the user has any of the roles.</returns>
    public static bool HasRole(this ClaimsPrincipal user, params string[] role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (role.Length == 0) throw new ArgumentOutOfRangeException(nameof(role));

        return user.Claims.Any(c => c.Type == ClaimTypes.Role && role.Contains(c.Value));
    }

    /// <summary>
    /// Determine if the user all of the specified roles.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="role"></param>
    /// <returns>True if the user has all of the roles.</returns>
    public static bool HasRoles(this ClaimsPrincipal user, params string[] role)
    {
        if (role == null) throw new ArgumentNullException(nameof(role));
        if (role.Length == 0) throw new ArgumentOutOfRangeException(nameof(role));

        var count = user.Claims.Count(c => c.Type == ClaimTypes.Role && role.Contains(c.Value));

        return count == role.Length;
    }
}
