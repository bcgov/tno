
namespace TNO.DAL.Services;

public interface IActionService : IBaseService<Entities.Action, int>
{
    IEnumerable<Entities.Action> FindAll();
    Entities.Action? FindByName(string name);
}
