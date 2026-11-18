
using TNO.Entities;
namespace TNO.DAL.Services;

public interface IMinisterService : IBaseService<Minister, int>
{
    IEnumerable<Minister> FindAll();
}
