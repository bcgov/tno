using Microsoft.AspNetCore.Mvc.Filters;
using TNO.API.Helpers;
using TNO.DAL;

namespace TNO.API.Filters;

/// <summary>
/// IndexRequestFilter class, after an action that saved index requests, sends them to Kafka before
/// the response. When Kafka does not accept them the request fails, since the system is down.
/// </summary>
public class IndexRequestFilter : IAsyncActionFilter
{
    #region Variables
    private readonly TNOContext _context;
    private readonly IIndexRequestSender _sender;
    private readonly IIndexFailureNotifier _failureNotifier;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an IndexRequestFilter object, initializes with specified parameters.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="sender"></param>
    /// <param name="failureNotifier">Support email alerts for unsent committed requests.</param>
    public IndexRequestFilter(TNOContext context, IIndexRequestSender sender, IIndexFailureNotifier failureNotifier)
    {
        _context = context;
        _sender = sender;
        _failureNotifier = failureNotifier;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Run the action, then send the index requests it saved.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception != null && !executed.ExceptionHandled)
        {
            // The action may have failed after committing. Alert support about any unsent committed changes.
            var requests = _context.TakeIndexRequests();
            if (requests.Count > 0) await _failureNotifier.NotifyAsync(requests, executed.Exception);
            return;
        }
        await _sender.SendAsync(_context);
    }
    #endregion
}
