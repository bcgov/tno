using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IProductService : IBaseService<Product, int>
{
    /// <summary>
    /// Find all products.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Product> FindAll();

    /// <summary>
    /// Find all my products.
    /// </summary>
    /// <returns></returns>
    IEnumerable<Product> FindProductsByUser(int userId);

    /// <summary>
    /// Find all products that match the filter.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IEnumerable<Product> Find(ProductFilter filter);

    /// <summary>
    /// Unsubscribe all products for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> Unsubscribe(int userId, int productId);
}
