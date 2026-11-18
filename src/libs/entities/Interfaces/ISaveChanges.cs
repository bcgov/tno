using System.Security.Claims;

namespace TNO.Entities;

public interface ISaveChanges
{
    void OnAdded(User user);
    void OnAdded(ClaimsPrincipal? user);
    void OnModified(User user);
    void OnModified(ClaimsPrincipal? user);
}
