using System.Security.Claims;

namespace TNO.DAL.Services;

public interface IBaseService
{
    ClaimsPrincipal Principal { get; }

    IServiceProvider Services { get; }
}
