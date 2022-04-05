
using TNO.Entities;

namespace TNO.DAL.Services;

public interface ICategoryService : IBaseService<Category, int>
{
    IEnumerable<Category> FindAll();
}
