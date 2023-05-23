
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;
namespace TNO.DAL.Services;

public interface IMinisterService : IBaseService<Minister, int>
{
    IEnumerable<Entities.Minister> FindAll();
}
