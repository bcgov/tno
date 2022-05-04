using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using TNO.DAL.Services;

namespace TNO.API.Filters;

/// <summary>
/// ETagCacheTableFilterAttribute class, provides a way to validate an ETag with the cache table.
/// </summary>
public class ETagCacheTableFilterAttribute : ActionFilterAttribute
{
    #region Properties
    /// <summary>
    /// get/set - The cache key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// get/set - The cache value for the etag.
    /// </summary>
    public string? Value { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of an ETagCacheTableFilterAttribute object, initializes with specified parameter.
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ETagCacheTableFilterAttribute(string key)
    {
        this.Key = key ?? throw new ArgumentNullException(nameof(key));
    }
    #endregion

    #region Methods
    /// <summary>
    /// Determines if the ETag matches cache.
    /// If it does then it will return a 304:NotModified Http Status Code.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var cacheService = context?.HttpContext?.RequestServices?.GetService<ICacheService>();
        if (cacheService != null && context != null && (context?.HttpContext?.Request?.Method == HttpMethod.Head.Method || context?.HttpContext?.Request?.Method == HttpMethod.Get.Method))
        {
            var etag = context.HttpContext.Request.Headers.IfNoneMatch;
            var cache = cacheService.FindById(this.Key);
            this.Value = cache?.Value;
            if (cache != null && etag != StringValues.Empty && cache.Value == etag)
            {
                context.HttpContext.Response.Headers.ETag = this.Value;
                context.Result = new StatusCodeResult((int)HttpStatusCode.NotModified);
            }
            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Include the cache key as an ETag in the response.
    /// </summary>
    /// <param name="context"></param>
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var cacheService = context?.HttpContext?.RequestServices?.GetService<ICacheService>();
        if (cacheService != null && context != null)
        {
            if (this.Value != null)
                context.HttpContext.Response.Headers.ETag = this.Value;
            base.OnActionExecuted(context);
        }
    }
    #endregion
}
