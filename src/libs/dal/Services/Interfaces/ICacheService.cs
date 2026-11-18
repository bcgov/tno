
namespace TNO.DAL.Services;

public interface ICacheService : IBaseService<Entities.Cache, string>
{
    IEnumerable<Entities.Cache> FindAll();
}
