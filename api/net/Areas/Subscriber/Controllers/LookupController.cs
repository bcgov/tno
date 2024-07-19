using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Lookup;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

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
    private readonly IActionService _actionService;
    private readonly ITopicService _topicService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISourceService _sourceService;
    private readonly IMinisterService _ministerService;
    private readonly ILicenseService _licenseService;
    private readonly ISeriesService _seriesService;
    private readonly IContributorService _contributorService;
    private readonly ITagService _tagService;
    private readonly ISettingService _settingService;
    private readonly ITonePoolService _tonePoolService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupController object, initializes with specified parameters.
    /// </summary>
    /// <param name="actionService"></param>
    /// <param name="topicService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="sourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="seriesService"></param>
    /// <param name="tagService"></param>
    /// <param name="settingService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="ministerService"></param>
    /// <param name="contributorService"></param>
    /// <param name="serializerOptions"></param>
    public LookupController(
        IActionService actionService,
        ITopicService topicService,
        IMinisterService ministerService,
        IMediaTypeService mediaTypeService,
        ISourceService sourceService,
        ILicenseService licenseService,
        IContributorService contributorService,
        ISeriesService seriesService,
        ITagService tagService,
        ISettingService settingService,
        ITonePoolService tonePoolService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _actionService = actionService;
        _topicService = topicService;
        _ministerService = ministerService;
        _mediaTypeService = mediaTypeService;
        _sourceService = sourceService;
        _licenseService = licenseService;
        _seriesService = seriesService;
        _tagService = tagService;
        _settingService = settingService;
        _tonePoolService = tonePoolService;
        _contributorService = contributorService;
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
    public IActionResult FindAllAsync()
    {
        var actions = _actionService.FindAll();
        var contributors = _contributorService.FindAll();
        var topics = _topicService.FindAll();
        var mediaTypes = _mediaTypeService.FindAll();
        var sources = _sourceService.FindAll();
        var license = _licenseService.FindAll();
        var series = _seriesService.FindAll();
        var ministers = _ministerService.FindAll();
        var tagServices = _tagService.FindAll();
        var settings = _settingService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            topics,
            mediaTypes,
            sources,
            license,
            series,
            tagServices,
            settings,
            tonePools,
            ministers,
            contributors,
            _serializerOptions
            ));
    }
    #endregion
}
