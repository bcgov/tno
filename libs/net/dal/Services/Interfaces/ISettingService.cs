
namespace TNO.DAL.Services;

public interface ISettingService : IBaseService<Entities.Setting, int>
{
    IEnumerable<Entities.Setting> FindAll();
    Entities.Setting? FindByName(string name);
}
