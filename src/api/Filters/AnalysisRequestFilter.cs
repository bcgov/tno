using Microsoft.AspNetCore.Mvc.Filters;
using TNO.API.Helpers;
using TNO.DAL;

namespace TNO.API.Filters;

/// <summary>
/// AnalysisRequestFilter class, after a request whose saves (or direct requests) asked for content
/// to be analyzed, sends the requests to the Kafka analysis topic before the API responds. When
/// Kafka does not accept them the request fails, since the system is down.
/// </summary>
public class AnalysisRequestFilter : IAsyncActionFilter
{
    #region Variables
    private readonly TNOContext _context;
    private readonly IAnalysisRequestSender _sender;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an AnalysisRequestFilter object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sender"></param>
    public AnalysisRequestFilter(TNOContext context, IAnalysisRequestSender sender)
    {
        _context = context;
        _sender = sender;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the action, then send the analysis requests it made.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception != null && !executed.ExceptionHandled)
        {
            // Nothing the failed action requested is sent.
            _context.TakeAnalysisRequests();
            return;
        }
        await _sender.SendAsync(_context);
    }
    #endregion
}
