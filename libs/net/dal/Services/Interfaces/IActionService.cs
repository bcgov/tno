
namespace TNO.DAL.Services;

public interface IActionService : IBaseService<Action, int>
{
    IEnumerable<Entities.Action> FindAll();
}
