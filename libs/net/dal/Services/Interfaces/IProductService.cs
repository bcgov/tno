
using TNO.Entities;

namespace TNO.DAL.Services;

public interface IProductService : IBaseService<Product, int>
{
    IEnumerable<Product> FindAll();
}
