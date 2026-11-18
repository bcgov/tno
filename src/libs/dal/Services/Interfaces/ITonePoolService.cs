
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ITonePoolService : IBaseService<TonePool, int>
{
    IEnumerable<TonePool> FindAll();
    TonePool? FindByUserId(int userId);
}
