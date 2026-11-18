using TNO.Entities;

namespace TNO.DAL.Services;

public interface IOrganizationService : IBaseService<Organization, int>
{
    IEnumerable<Organization> FindAll();
}
