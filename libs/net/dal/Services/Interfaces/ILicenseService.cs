
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ILicenseService : IBaseService<License, int>
{
    IEnumerable<License> FindAll();
}
