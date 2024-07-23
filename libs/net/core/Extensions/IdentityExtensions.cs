using System.Security.Claims;

namespace TNO.Core.Extensions;

public static class IdentityExtensions
{
    /// <summary>
    /// Get the currently logged in user's ClaimTypes.NameIdentifier.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetIdentifier(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Get the user's username.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst("username")?.Value
            ?? user.FindFirst("idir_username")?.Value
            ?? user.FindFirst("github_username")?.Value
            ?? user.FindFirst("bceid_username")?.Value
            ?? user.FindFirst("bcgov_username")?.Value
            ?? user.FindFirst("user_principal_name")?.Value
            ?? user.FindFirst("preferred_username")?.Value;
    }

    /// <summary>
    /// Get the currently logged in user's 'identity_provider'
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetIdentityProvider(this ClaimsPrincipal user)
    {
        return user.FindFirst("identity_provider")?.Value;
    }

    /// <summary>
    /// Get the currently logged in user's 'uid'
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetUid(this ClaimsPrincipal user)
    {
        return user.FindFirst("idir_user_guid")?.Value
            ?? user.FindFirst("github_id")?.Value
            ?? user.FindFirst("bceid_user_guid")?.Value
            ?? user.FindFirst("bcgov_guid")?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Get the user's display name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetDisplayName(this ClaimsPrincipal user)
    {
        return user.FindFirst("display_name")?.Value
            ?? user.FindFirst("displayName")?.Value
            ?? user.FindFirst("name")?.Value;
    }

    /// <summary>
    /// Get the user's first name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetFirstName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.GivenName)?.Value ?? user.FindFirst("given_name")?.Value;
    }

    /// <summary>
    /// Get the user's last name.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetLastName(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Surname)?.Value ?? user.FindFirst("family_name")?.Value;
    }

    /// <summary>
    /// Get the user's email.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Get whether the email has been verified.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static bool? GetEmailVerified(this ClaimsPrincipal user)
    {
        if (bool.TryParse(user.FindFirst("email_verified")?.Value, out bool result))
            return result;
        return null;
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
