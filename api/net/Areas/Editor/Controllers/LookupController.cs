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
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataLocationService _dataLocationService;
    private readonly IDataSourceService _dataSourceService;
    private readonly ILicenseService _licenseService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IRoleService _roleService;
    private readonly ISeriesService _seriesService;
    private readonly ISourceActionService _sourceActionService;
    private readonly ISourceMetricService _sourceMetricService;
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
    /// <param name="contentTypeService"></param>
    /// <param name="dataLocationService"></param>
    /// <param name="dataSourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="roleService"></param>
    /// <param name="seriesService"></param>
    /// <param name="sourceActionService"></param>
    /// <param name="sourceMetricService"></param>
    /// <param name="tagService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="userService"></param>
    /// <param name="serializerOptions"></param>
    public LookupController(
        IActionService actionService,
        ICategoryService categoryService,
        IClaimService claimService,
        IContentTypeService contentTypeService,
        IDataLocationService dataLocationService,
        IDataSourceService dataSourceService,
        ILicenseService licenseService,
        IMediaTypeService mediaTypeService,
        IRoleService roleService,
        ISeriesService seriesService,
        ISourceActionService sourceActionService,
        ISourceMetricService sourceMetricService,
        ITagService tagService,
        ITonePoolService tonePoolService,
        IUserService userService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _actionService = actionService;
        _categoryService = categoryService;
        _claimService = claimService;
        _contentTypeService = contentTypeService;
        _dataLocationService = dataLocationService;
        _dataSourceService = dataSourceService;
        _licenseService = licenseService;
        _mediaTypeService = mediaTypeService;
        _roleService = roleService;
        _seriesService = seriesService;
        _sourceActionService = sourceActionService;
        _sourceMetricService = sourceMetricService;
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
        var contentTypes = _contentTypeService.FindAll();
        var dataLocations = _dataLocationService.FindAll();
        var dataSources = _dataSourceService.FindAll();
        var license = _licenseService.FindAll();
        var mediaTypes = _mediaTypeService.FindAll();
        var roles = _roleService.FindAll();
        var series = _seriesService.FindAll();
        var sourceActions = _sourceActionService.FindAll();
        var sourceMetrics = _sourceMetricService.FindAll();
        var tagServices = _tagService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        var users = _userService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            categories,
            claims,
            contentTypes,
            dataLocations,
            dataSources,
            license,
            mediaTypes,
            roles,
            series,
            sourceActions,
            sourceMetrics,
            tagServices,
            tonePools,
            users,
            _serializerOptions
            ));
    }
    #endregion
}
