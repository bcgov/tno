
namespace TNO.DAL.Services;

public interface ISettingService : IBaseService<Entities.Setting, int>
{
    IEnumerable<Entities.Setting> FindAll();
    Entities.Setting? FindByName(string name);

    /// <summary>
    /// Set the value of the setting with the specified 'name', creating it when it does not exist.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="description">The description used when the setting is created.</param>
    /// <returns></returns>
    Entities.Setting SetValue(string name, string value, string description = "");
}
