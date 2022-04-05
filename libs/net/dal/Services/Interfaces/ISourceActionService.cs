
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ISourceActionService : IBaseService<SourceAction, int>
{
    IEnumerable<SourceAction> FindAll();
}
