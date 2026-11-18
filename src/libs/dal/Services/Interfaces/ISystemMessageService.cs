using TNO.Entities;
namespace TNO.DAL.Services;

public interface ISystemMessageService : IBaseService<SystemMessage, int>
{
    IEnumerable<SystemMessage> FindAll();
}
