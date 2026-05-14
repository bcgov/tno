
namespace TNO.DAL.Services;

public interface ILLMService : IBaseService<Entities.LLM, int>
{
    IEnumerable<Entities.LLM> FindAll();
    Entities.LLM? FindByName(string name);
}
