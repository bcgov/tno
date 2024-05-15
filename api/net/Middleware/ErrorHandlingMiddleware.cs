using System.Net;
using System.Net.Mime;
using System.Security.Authentication;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;

namespace TNO.API.Middleware
{
    /// <summary>
    /// ErrorHandlingMiddleware class, provides a way to catch and handle unhandled errors in a generic way.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        #region Variables
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly JsonOptions _options;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an ErrorHandlingMiddleware class, and initializes it with the specified arguments.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger, IOptions<JsonOptions> options)
        {
            _next = next;
            _env = env;
            _logger = logger;
            _options = options.Value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Handle the exception if one occurs.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handle the exception by returning an appropriate error message depending on type and environment.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An unhandled error has occurred.";
            string? details = null;

            if (ex is SecurityTokenException)
            {
                code = HttpStatusCode.Unauthorized;
                message = "The authentication token is invalid.";
            }
            else if (ex is SecurityTokenValidationException)
            {
                code = HttpStatusCode.Unauthorized;
                message = "The authentication token is invalid.";
            }
            else if (ex is SecurityTokenExpiredException)
            {
                code = HttpStatusCode.Unauthorized;
                message = "The authentication token has expired.";
            }
            else if (ex is SecurityTokenNotYetValidException)
            {
                code = HttpStatusCode.Unauthorized;
                message = "The authentication token not yet valid.";
            }
            else if (ex is AuthenticationException)
            {
                code = HttpStatusCode.Unauthorized;
                var exception = ex as AuthenticationException;
                message = exception?.Message;
                details = exception?.InnerException?.Message;

                _logger.LogWarning(ex, "Unable to validate authentication information.  {error}", ex.Message);
            }
            else if (ex is NotAuthorizedException)
            {
                code = HttpStatusCode.Forbidden;
                message = "User is not authorized to perform this action.";

                _logger.LogWarning(ex, "Not authorized error.  {error}", ex.Message);
            }
            else if (ex is DbUpdateConcurrencyException)
            {
                code = HttpStatusCode.BadRequest;
                message = "Data may have been modified or deleted since item was loaded.  Refresh your data and reapply your changes.";

                _logger.LogError(ex,
                    "Optimistic concurrency error (user agent: {userAgent})",
                    context.Request.Headers["User-Agent"].ToString());
            }
            else if (ex is DbUpdateException)
            {
                code = HttpStatusCode.BadRequest;
                if (ex.InnerException?.Message.Contains("23505: duplicate key value violates unique constraint") == true)
                    message = "A record already exists with this key.";
                else
                    message = "A database error occurred while updating.";

                _logger.LogDebug(ex, "Database update error.  {error}", ex.Message);
            }
            else if (ex is KeyNotFoundException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;

                _logger.LogDebug(ex, "Key not found.  {error}", ex.Message);
            }
            else if (ex is NoContentException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;

                _logger.LogDebug(ex, "Content not found.  {error}", ex.Message);
            }
            else if (ex is RowVersionMissingException)
            {
                code = HttpStatusCode.BadRequest;
                message = "Item cannot be updated without a row version.";

                _logger.LogDebug(ex, "Row version missing error.  {error}", ex.Message);
            }
            else if (ex is ArgumentException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;

                _logger.LogWarning(ex, "Middleware caught unhandled exception. {error}", ex.Message);
            }
            else if (ex is BadRequestException || ex is InvalidOperationException)
            {
                code = HttpStatusCode.BadRequest;
                message = ex.Message;

                _logger.LogError(ex, "Invalid operation or bad request details. {error}", ex.Message);
            }
            else if (ex is ConfigurationException)
            {
                code = HttpStatusCode.InternalServerError;
                message = "Application configuration details invalid or missing.";

                _logger.LogError(ex, "Configuration error.  {error}", ex.Message);
            }
            else
            {
                _logger.LogError(ex, "Middleware caught unhandled exception. {error}", ex.GetAllMessages());
            }

            if (!context.Response.HasStarted)
            {
                var result = JsonSerializer.Serialize(new Models.ErrorResponseModel(ex, message, details, !_env.IsProduction()), _options.JsonSerializerOptions);
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
            }
            else
            {
                // Had to do this because odd errors were occurring when bearer tokens were failing.
                await context.Response.WriteAsync(string.Empty);
            }
        }
        #endregion
    }
}
