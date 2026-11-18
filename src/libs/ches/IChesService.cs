using TNO.Ches.Models;

namespace TNO.Ches;

/// <summary>
/// IChesService interface, provides API endpoints for Common Hosted Email Service (CHES).
/// </summary>
public interface IChesService
{
    /// <summary>
    /// Send an HTTP request to CHES to send the specified 'email'.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<EmailResponseModel> SendEmailAsync(IEmail email);

    /// <summary>
    /// Send an HTTP request to CHES to send the specified 'email'.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<EmailResponseModel> SendEmailAsync(IEmailMerge email);

    /// <summary>
    /// Send an HTTP request to get the current status of the message for the specified 'messageId'.
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<StatusResponseModel> GetStatusAsync(Guid messageId);

    /// <summary>
    /// Send an HTTP request to get the current status of the message(s) for the specified 'filter'.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    Task<IEnumerable<StatusResponseModel>> GetStatusAsync(StatusModel filter);

    /// <summary>
    /// Send a cancel HTTP request to CHES for the specified 'messageId'.
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task<StatusResponseModel> CancelEmailAsync(Guid messageId);

    /// <summary>
    /// Send a cancel HTTP request to CHES for the specified 'filter'.
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    Task<IEnumerable<StatusResponseModel>> CancelEmailAsync(StatusModel filter);

    /// <summary>
    /// Send an HTTP request to promote the message for the specified 'messageId'.
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    Task PromoteAsync(Guid messageId);
}
