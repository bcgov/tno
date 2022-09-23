using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Editor.Models.Lookup;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Editor.Controllers;

/// <summary>
/// LookupController class, provides Lookup endpoints for the api.
/// The purpose is to reduce the number of AJAX requests to fetch separate lookup values.
/// </summary>
[Authorize]
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
    private readonly ICategoryService _categoryService;
    private readonly IClaimService _claimService;
    private readonly IProductService _productService;
    private readonly ISourceService _sourceService;
    private readonly ILicenseService _licenseService;
    private readonly IIngestTypeService _ingestTypeService;
    private readonly IRoleService _roleService;
    private readonly ISeriesService _seriesService;
    private readonly ISourceActionService _sourceActionService;
    private readonly IMetricService _metricService;
    private readonly ITagService _tagService;
    private readonly ITonePoolService _tonePoolService;
    private readonly IUserService _userService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupController object, initializes with specified parameters.
    /// </summary>
    /// <param name="actionService"></param>
    /// <param name="categoryService"></param>
    /// <param name="claimService"></param>
    /// <param name="productService"></param>
    /// <param name="sourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="ingestTypeService"></param>
    /// <param name="roleService"></param>
    /// <param name="seriesService"></param>
    /// <param name="sourceActionService"></param>
    /// <param name="metricService"></param>
    /// <param name="tagService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="userService"></param>
    /// <param name="serializerOptions"></param>
    public LookupController(
        IActionService actionService,
        ICategoryService categoryService,
        IClaimService claimService,
        IProductService productService,
        ISourceService sourceService,
        ILicenseService licenseService,
        IIngestTypeService ingestTypeService,
        IRoleService roleService,
        ISeriesService seriesService,
        ISourceActionService sourceActionService,
        IMetricService metricService,
        ITagService tagService,
        ITonePoolService tonePoolService,
        IUserService userService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _actionService = actionService;
        _categoryService = categoryService;
        _claimService = claimService;
        _productService = productService;
        _sourceService = sourceService;
        _licenseService = licenseService;
        _ingestTypeService = ingestTypeService;
        _roleService = roleService;
        _seriesService = seriesService;
        _sourceActionService = sourceActionService;
        _metricService = metricService;
        _tagService = tagService;
        _tonePoolService = tonePoolService;
        _userService = userService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of Lookup.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces("application/json")]
    [ProducesResponseType(typeof(IEnumerable<LookupModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "Lookup" })]
    [ETagCacheTableFilter("lookups")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindAll()
    {
        var actions = _actionService.FindAll();
        var categories = _categoryService.FindAll();
        var claims = _claimService.FindAll();
        var products = _productService.FindAll();
        var sources = _sourceService.FindAll();
        var license = _licenseService.FindAll();
        var ingestTypes = _ingestTypeService.FindAll();
        var roles = _roleService.FindAll();
        var series = _seriesService.FindAll();
        var sourceActions = _sourceActionService.FindAll();
        var metrics = _metricService.FindAll();
        var tagServices = _tagService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        var users = _userService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            categories,
            claims,
            products,
            sources,
            license,
            ingestTypes,
            roles,
            series,
            sourceActions,
            metrics,
            tagServices,
            tonePools,
            users,
            _serializerOptions
            ));
    }
    #endregion
}
