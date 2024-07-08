using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.DAL.Services;

public interface IProductService : IBaseService<Product, int>
{
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
    IEnumerable<Product> Find(ProductFilter? filter = null);

    /// <summary>
    /// Subscribe the specified user to the product.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> Subscribe(int userId, int productId);

    /// <summary>
    /// Creates a request for an admin to subscribe product for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> RequestSubscribe(int userId, int productId);

    /// <summary>
    /// Unsubscribe product for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> Unsubscribe(int userId, int productId);

    /// <summary>
    /// Creates a request for an admin to unsubscribe product for the specified user.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> RequestUnsubscribe(int userId, int productId);

    /// <summary>
    /// Cancels a request to change subscription status.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<int> CancelSubscriptionStatusChangeRequest(int userId, int productId);
}
