using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.CSS;
using TNO.CSS.Config;
using TNO.Keycloak;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// LookupController class, provides Lookup endpoints for the api.
/// The purpose is to reduce the number of AJAX requests to fetch separate lookup values.
/// </summary>
[ClientRoleAuthorize(ClientRole.Editor, ClientRole.Administrator)]
[ApiController]
[Area("editor")]
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
    private readonly IActionService _actionService;
    private readonly ITopicService _topicService;
    private readonly ITopicScoreRuleService _topicScoreRuleService;
    private readonly IProductService _productService;
    private readonly ISourceService _sourceService;
    private readonly ILicenseService _licenseService;
    private readonly IIngestTypeService _ingestTypeService;
    private readonly ISeriesService _seriesService;
    private readonly IMetricService _metricService;
    private readonly ITagService _tagService;
    private readonly ITonePoolService _tonePoolService;
    private readonly IUserService _userService;
    private readonly IDataLocationService _dataLocationService;
    private readonly ICssEnvironmentService _CssService;
    private readonly CssEnvironmentOptions _CssOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupController object, initializes with specified parameters.
    /// </summary>
    /// <param name="actionService"></param>
    /// <param name="topicService"></param>
    /// <param name="topicScoreRuleService"></param>
    /// <param name="productService"></param>
    /// <param name="sourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="ingestTypeService"></param>
    /// <param name="seriesService"></param>
    /// <param name="metricService"></param>
    /// <param name="tagService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="userService"></param>
    /// <param name="dataLocationService"></param>
    /// <param name="cssService"></param>
    /// <param name="cssOptions"></param>
    /// <param name="serializerOptions"></param>
    public LookupController(
        IActionService actionService,
        ITopicService topicService,
        ITopicScoreRuleService topicScoreRuleService,
        IProductService productService,
        ISourceService sourceService,
        ILicenseService licenseService,
        IIngestTypeService ingestTypeService,
        ISeriesService seriesService,
        IMetricService metricService,
        ITagService tagService,
        ITonePoolService tonePoolService,
        IUserService userService,
        IDataLocationService dataLocationService,
        ICssEnvironmentService cssService,
        IOptions<CssEnvironmentOptions> cssOptions,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _actionService = actionService;
        _topicService = topicService;
        _topicScoreRuleService = topicScoreRuleService;
        _productService = productService;
        _sourceService = sourceService;
        _licenseService = licenseService;
        _ingestTypeService = ingestTypeService;
        _seriesService = seriesService;
        _metricService = metricService;
        _tagService = tagService;
        _tonePoolService = tonePoolService;
        _userService = userService;
        _dataLocationService = dataLocationService;
        _CssService = cssService;
        _CssOptions = cssOptions.Value;
        _serializerOptions = serializerOptions.Value;
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
        var rules = _topicScoreRuleService.FindAll();
        var products = _productService.FindAll();
        var sources = _sourceService.FindAll();
        var license = _licenseService.FindAll();
        var ingestTypes = _ingestTypeService.FindAll();
        var roles = (await _CssService.GetRolesAsync()).Select(r => r.Name!);
        var series = _seriesService.FindAll();
        var metrics = _metricService.FindAll();
        var tagServices = _tagService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        var users = _userService.FindByRoles(roles.Where(x => x.ToLower() != ClientRole.Subscriber.ToString().ToLower()));
        var dataLocations = _dataLocationService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            topics,
            rules,
            products,
            sources,
            license,
            ingestTypes,
            roles,
            series,
            metrics,
            tagServices,
            tonePools,
            users,
            dataLocations,
            _serializerOptions
            ));
    }
    #endregion
}
