using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Lookup;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.CSS;
using TNO.CSS.Config;
using TNO.Keycloak;
using TNO.Core.Http;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// LookupController class, provides Lookup endpoints for the api.
/// The purpose is to reduce the number of AJAX requests to fetch separate lookup values.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber, ClientRole.Administrator)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/lookups")]
[Route("api/[area]/lookups")]
[Route("v{version:apiVersion}/[area]/lookups")]
[Route("[area]/lookups")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class LookupController : ControllerBase
{
    #region Variables
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IHttpRequestClient _httpClient;
    private readonly IActionService _actionService;
    private readonly ITopicService _topicService;
    private readonly ITopicScoreRuleService _topicScoreRuleService;
    private readonly IProductService _productService;
    private readonly ISourceService _sourceService;
    private readonly IMinisterService _ministerService;
    private readonly ILicenseService _licenseService;
    private readonly ISeriesService _seriesService;
    private readonly ITagService _tagService;
    private readonly ITonePoolService _tonePoolService;
    private readonly ICssEnvironmentService _CssService;
    private readonly CssEnvironmentOptions _CssOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupController object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="actionService"></param>
    /// <param name="topicService"></param>
    /// <param name="productService"></param>
    /// <param name="sourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="seriesService"></param>
    /// <param name="tagService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="ministerService"></param>
    /// <param name="cssService"></param>
    /// <param name="cssOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public LookupController(
        IHttpRequestClient httpClient,
        IActionService actionService,
        ITopicService topicService,
        IMinisterService ministerService,
        IProductService productService,
        ISourceService sourceService,
        ILicenseService licenseService,
        ISeriesService seriesService,
        ITagService tagService,
        ITonePoolService tonePoolService,
        ICssEnvironmentService cssService,
        IOptions<CssEnvironmentOptions> cssOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<LookupController> logger)
    {
        _httpClient = httpClient;
        _actionService = actionService;
        _topicService = topicService;
        _ministerService = ministerService;
        _productService = productService;
        _sourceService = sourceService;
        _licenseService = licenseService;
        _seriesService = seriesService;
        _tagService = tagService;
        _tonePoolService = tonePoolService;
        _CssService = cssService;
        _CssOptions = cssOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Lookup.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<LookupModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "Lookup" })]
    [ETagCacheTableFilter("lookups")]
    [ResponseCache(Duration = 5 * 60)]
    public async Task<IActionResult> FindAllAsync()
    {
        if (String.IsNullOrWhiteSpace(_CssOptions.ClientId)) throw new ConfigurationException("CSS clientId has not been configured");
        if (String.IsNullOrWhiteSpace(_CssOptions.Secret)) throw new ConfigurationException("CSS secret has not been configured");

        var actions = _actionService.FindAll();
        var topics = _topicService.FindAll();
        var products = _productService.FindAll();
        var sources = _sourceService.FindAll();
        var license = _licenseService.FindAll();
        var series = _seriesService.FindAll();
        var ministers = _ministerService.FindAll();
        var tagServices = _tagService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            topics,
            products,
            sources,
            license,
            series,
            tagServices,
            tonePools,
            ministers,
            _serializerOptions
            ));
    }
    #endregion
}
