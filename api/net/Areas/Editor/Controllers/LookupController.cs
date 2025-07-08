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
using TNO.Core.Http;
using TNO.DAL.Services;
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
    private readonly IHttpRequestClient _httpClient;
    private readonly IActionService _actionService;
    private readonly ITopicService _topicService;
    private readonly ITopicScoreRuleService _topicScoreRuleService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISourceService _sourceService;
    private readonly ILicenseService _licenseService;
    private readonly IIngestTypeService _ingestTypeService;
    private readonly ISeriesService _seriesService;
    private readonly IContributorService _contributorService;
    private readonly IMetricService _metricService;
    private readonly ITagService _tagService;
    private readonly ITonePoolService _tonePoolService;
    private readonly IUserService _userService;
    private readonly IDataLocationService _dataLocationService;
    private readonly ISettingService _settingService;
    private readonly IKeycloakService _keycloakService;
    private readonly IOrganizationService _organizationService;
    private readonly Config.KeycloakOptions _keycloakOptions;
    private readonly ILogger _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a LookupController object, initializes with specified parameters.
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="actionService"></param>
    /// <param name="topicService"></param>
    /// <param name="topicScoreRuleService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="sourceService"></param>
    /// <param name="licenseService"></param>
    /// <param name="ingestTypeService"></param>
    /// <param name="seriesService"></param>
    /// <param name="contributorService"></param>
    /// <param name="metricService"></param>
    /// <param name="tagService"></param>
    /// <param name="tonePoolService"></param>
    /// <param name="userService"></param>
    /// <param name="dataLocationService"></param>
    /// <param name="settingService"></param>
    /// <param name="keycloakService"></param>
    /// <param name="keycloakOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    /// <param name="organizationService"></param>
    public LookupController(
        IHttpRequestClient httpClient,
        IActionService actionService,
        ITopicService topicService,
        ITopicScoreRuleService topicScoreRuleService,
        IMediaTypeService mediaTypeService,
        ISourceService sourceService,
        ILicenseService licenseService,
        IIngestTypeService ingestTypeService,
        ISeriesService seriesService,
        IContributorService contributorService,
        IMetricService metricService,
        ITagService tagService,
        ITonePoolService tonePoolService,
        IUserService userService,
        IDataLocationService dataLocationService,
        ISettingService settingService,
        IKeycloakService keycloakService,
        IOptions<Config.KeycloakOptions> keycloakOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<LookupController> logger,
        IOrganizationService organizationService)
    {
        _httpClient = httpClient;
        _actionService = actionService;
        _topicService = topicService;
        _topicScoreRuleService = topicScoreRuleService;
        _mediaTypeService = mediaTypeService;
        _sourceService = sourceService;
        _licenseService = licenseService;
        _ingestTypeService = ingestTypeService;
        _seriesService = seriesService;
        _contributorService = contributorService;
        _metricService = metricService;
        _tagService = tagService;
        _tonePoolService = tonePoolService;
        _userService = userService;
        _dataLocationService = dataLocationService;
        _settingService = settingService;
        _keycloakService = keycloakService;
        _keycloakOptions = keycloakOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _organizationService = organizationService;
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
        if (!_keycloakOptions.ClientId.HasValue) throw new ConfigurationException("Keycloak ClientId must be in configuration.");

        CanadaHolidayModel? statHolidays = null;
        try
        {
            statHolidays = await _httpClient.GetAsync<CanadaHolidayModel>("https://canada-holidays.ca/api/v1/provinces/BC");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch BC holidays");
        }

        var actions = _actionService.FindAll();
        var topics = _topicService.FindAll();
        var rules = _topicScoreRuleService.FindAll();
        var mediaTypes = _mediaTypeService.FindAll();
        var sources = _sourceService.FindAll();
        var license = _licenseService.FindAll();
        var ingestTypes = _ingestTypeService.FindAll();
        var roles = (await _keycloakService.GetRolesAsync(_keycloakOptions.ClientId.Value)).Select(r => r.Name!);
        var series = _seriesService.FindAll();
        var contributors = _contributorService.FindAll();
        var metrics = _metricService.FindAll();
        var tagServices = _tagService.FindAll();
        var tonePools = _tonePoolService.FindAll();
        var users = _userService.FindByRoles(roles.Where(x => x == ClientRole.Editor.ToString().ToLower()));
        var dataLocations = _dataLocationService.FindAll();
        var settings = _settingService.FindAll();
        var organizations = _organizationService.FindAll();
        return new JsonResult(new LookupModel(
            actions,
            topics,
            rules,
            mediaTypes,
            sources,
            license,
            ingestTypes,
            roles,
            series,
            contributors,
            metrics,
            tagServices,
            tonePools,
            users,
            dataLocations,
            settings,
            statHolidays?.Province?.Holidays ?? Array.Empty<HolidayModel>(),
            _serializerOptions,
            organizations
            ));
    }
    #endregion
}
