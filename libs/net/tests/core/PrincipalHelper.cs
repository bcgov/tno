namespace TNO.Test.Core;

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Principal static class, provides helper functions for principal identities.
/// </summary>
[ExcludeFromCodeCoverage]
public static class PrincipalHelper
{
    /// <summary>
    /// Create a ClaimsPrincipal for the specified role.
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public static ClaimsPrincipal CreateForRole(params string[] role)
    {
        role ??= Array.Empty<string>();

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
            };

        foreach (var claim in role)
        {
            claims.Add(new Claim(ClaimTypes.Role, claim ?? "none"));
        }
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

        return user;
    }

    /// <summary>
    /// Add a claim to the specified 'user'.
    /// This will create a new user that has the claim.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public static ClaimsPrincipal AddClaim(this ClaimsPrincipal user, string type, string value)
    {
        var identity = new ClaimsIdentity(user.Identity);
        identity.AddClaim(new Claim(type, value));

        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Create a ClaimsPrincipal for the specified 'roles' and add it as a singleton to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    public static IServiceCollection AddPrincipalForRole(this IServiceCollection services, params string[] roles)
    {
        return services.AddSingleton(CreateForRole(roles));
    }
}
