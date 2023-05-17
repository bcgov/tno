using TNO.Entities;

namespace TNO.DAL.Services;

public interface IContributorService : IBaseService<Contributor, int>
{
    IEnumerable<Contributor> FindAll();
}
