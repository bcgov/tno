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
    IEnumerable<Product> Find(ProductFilter filter);

    /// <summary>
    /// Find the report for the specified 'id'.
    /// Also include the user subscriptions to the linked product types.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="includeSubscriptions"></param>
    /// <returns></returns>
    Product? FindById(int id, bool includeSubscriptions);

    /// <summary>
    /// Add the user product.
    /// </summary>
    /// <param name="subscription"></param>
    void AddAndSave(UserProduct subscription);

    /// <summary>
    /// Update the user product.
    /// </summary>
    /// <param name="subscription"></param>
    void UpdateAndSave(UserProduct subscription);

    /// <summary>
    /// Unsubscribe to the specified user product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    UserProduct Unsubscribe(int userId, int productId);

    /// <summary>
    /// Subscribe to the specified user product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns></returns>
    UserProduct Subscribe(int userId, int productId);

    /// <summary>
    /// Send email to administrator to identify a subscription request or cancellation request.
    /// </summary>
    /// <param name="subscription"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    Task SendSubscriptionRequestEmailAsync(UserProduct subscription);
}
