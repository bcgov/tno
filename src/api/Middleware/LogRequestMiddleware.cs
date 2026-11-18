using TNO.Core.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace TNO.API.Middleware
{
    /// <summary>
    /// LogRequestMiddleware class, provides a way to log requests inbound to the API.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LogRequestMiddleware
    {
        #region Variables
        private readonly RequestDelegate _next;
        private readonly ILogger<LogRequestMiddleware> _logger;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an LogRequestMiddleware class, and initializes it with the specified arguments.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public LogRequestMiddleware(RequestDelegate next, ILogger<LogRequestMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a log message for the request.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            _logger.LogDebug("Received HTTP Request {method} user:{user} {scheme}://{host}{path}{query}",
                context.Request.Method,
                context.User.GetUsername(),
                context.Request.Scheme,
                context.Request.Host,
                context.Request.Path,
                context.Request.QueryString);

            await _next(context);
        }
        #endregion
    }
}
