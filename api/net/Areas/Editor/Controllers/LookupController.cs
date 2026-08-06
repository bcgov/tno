using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
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
    private const string HolidaysUrl = "https://canada-holidays.ca/api/v1/provinces/BC";
    private const string HolidaysCacheKey = "lookup:bc-holidays";
    private const string HolidaysFallbackCacheKey = "lookup:bc-holidays:last-known-good";
    private static readonly TimeSpan HolidaysCacheDuration = TimeSpan.FromHours(12);
    private static readonly TimeSpan HolidaysRequestTimeout = TimeSpan.FromSeconds(5);

    private readonly JsonSerializerOptions _serializerOptions;
    private readonly IHttpRequestClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
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
    /// <param name="httpClientFactory"></param>
    /// <param name="cache"></param>
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
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
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
        _httpClientFactory = httpClientFactory;
        _cache = cache;
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

        var statHolidays = await GetStatHolidaysAsync();

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
            statHolidays,
            _serializerOptions,
            organizations
            ));
    }

    /// <summary>
    /// Fetch the BC statutory holidays from the external holiday service, cached for a long duration.
    /// The external service is a soft dependency: a fresh cached value is served when available;
    /// otherwise a single request is made with a short timeout (so a hung TLS handshake fails fast).
    /// On failure the last successfully fetched value is served (or an empty list on a cold cache),
    /// and the error is logged as a warning rather than an error so it does not pollute alerting.
    /// </summary>
    /// <returns></returns>
    private async Task<IEnumerable<HolidayModel>> GetStatHolidaysAsync()
    {
        if (_cache.TryGetValue(HolidaysCacheKey, out IEnumerable<HolidayModel>? cached) && cached != null)
            return cached;

        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = HolidaysRequestTimeout;
            using var response = await client.GetAsync(HolidaysUrl);
            response.EnsureSuccessStatusCode();
            using var stream = await response.Content.ReadAsStreamAsync();
            var model = await JsonSerializer.DeserializeAsync<CanadaHolidayModel>(stream, _serializerOptions);
            var holidays = model?.Province?.Holidays ?? Array.Empty<HolidayModel>();

            // Cache the fresh value and retain it as the last-known-good fallback for future failures.
            _cache.Set(HolidaysCacheKey, holidays, HolidaysCacheDuration);
            _cache.Set(HolidaysFallbackCacheKey, holidays);
            return holidays;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch BC holidays; serving last-known-good value.");
            if (_cache.TryGetValue(HolidaysFallbackCacheKey, out IEnumerable<HolidayModel>? fallback) && fallback != null)
                return fallback;
            return Array.Empty<HolidayModel>();
        }
    }
    #endregion
}
