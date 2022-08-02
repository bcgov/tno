
using TNO.DAL.Models;
using TNO.Entities;
using TNO.Entities.Models;

namespace TNO.DAL.Services;

public interface ICategoryService : IBaseService<Category, int>
{
    IEnumerable<Category> FindAll();
    IPaged<Category> Find(CategoryFilter filter);

}
