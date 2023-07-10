using TNO.Entities;
namespace TNO.DAL.Services;

public interface ISystemMessageService : IBaseService<SystemMessage, int>
{
    SystemMessage? FindSystemMessage();
}
