
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IClaimService : IBaseService<Claim, int>
{
    IEnumerable<Claim> FindAll();
}
