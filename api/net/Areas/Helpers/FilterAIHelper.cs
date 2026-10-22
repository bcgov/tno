using System.Text.Json;
using Microsoft.Extensions.Options;
using TNO.AI;
using TNO.API.Config;
using TNO.API.Models.FilterAI;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Elastic;
using TNO.TemplateEngine.Config;
using TNO.TemplateEngine.FilterAI;

namespace TNO.API.Helpers;

/// <summary>
/// FilterAIHelper class, generates filter settings from a prompt and answers questions about how
/// to write that prompt. Requests are answered synchronously, so each request to the model has
/// its own timeout and the whole answer a budget, both under the two minute route limit. An
/// editor's model searches all content; a subscriber's searches published content without the
/// sources and media types they may not see.
/// </summary>
public class FilterAIHelper : IFilterAIHelper
{
    #region Variables
    /// <summary>The most time one request to the model may take.</summary>
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(45);

    private readonly ISettingService _settingService;
    private readonly ILLMService _llmService;
    private readonly ISourceService _sourceService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly ISeriesService _seriesService;
    private readonly IContributorService _contributorService;
    private readonly ITagService _tagService;
    private readonly IActionService _actionService;
    private readonly ITNOElasticClient _elasticClient;
    private readonly IImpersonationHelper _impersonate;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ElasticOptions _elasticOptions;
    private readonly AzureOptions _azureOptions;
    private readonly ApiOptions _apiOptions;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly ILogger<FilterAIHelper> _logger;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterAIHelper object, initializes with specified parameters.
    /// </summary>
    /// <param name="settingService"></param>
    /// <param name="llmService"></param>
    /// <param name="sourceService"></param>
    /// <param name="mediaTypeService"></param>
    /// <param name="seriesService"></param>
    /// <param name="contributorService"></param>
    /// <param name="tagService"></param>
    /// <param name="actionService"></param>
    /// <param name="elasticClient"></param>
    /// <param name="impersonate"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="azureOptions"></param>
    /// <param name="apiOptions"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="logger"></param>
    public FilterAIHelper(
        ISettingService settingService,
        ILLMService llmService,
        ISourceService sourceService,
        IMediaTypeService mediaTypeService,
        ISeriesService seriesService,
        IContributorService contributorService,
        ITagService tagService,
        IActionService actionService,
        ITNOElasticClient elasticClient,
        IImpersonationHelper impersonate,
        IHttpClientFactory httpClientFactory,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<AzureOptions> azureOptions,
        IOptions<ApiOptions> apiOptions,
        IOptions<JsonSerializerOptions> serializerOptions,
        ILogger<FilterAIHelper> logger)
    {
        _settingService = settingService;
        _llmService = llmService;
        _sourceService = sourceService;
        _mediaTypeService = mediaTypeService;
        _seriesService = seriesService;
        _contributorService = contributorService;
        _tagService = tagService;
        _actionService = actionService;
        _elasticClient = elasticClient;
        _impersonate = impersonate;
        _httpClientFactory = httpClientFactory;
        _elasticOptions = elasticOptions.Value;
        _azureOptions = azureOptions.Value;
        _apiOptions = apiOptions.Value;
        _serializerOptions = serializerOptions.Value;
        _logger = logger;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generate filter settings from the prompt.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forSubscriber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<FilterAIGenerateResultModel> GenerateAsync(FilterAIGenerateRequestModel request, bool forSubscriber, CancellationToken cancellationToken = default)
    {
        var (endpoint, options) = GetModel();
        try
        {
            return await CreateGenerator(forSubscriber, request.Settings?.SearchUnpublished == true).GenerateAsync(endpoint, options, request, Today(), cancellationToken);
        }
        catch (FilterAIException ex)
        {
            throw new BadRequestException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Answer a question about how to write the filter prompt.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="forSubscriber"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    public async Task<FilterAIChatResultModel> ChatAsync(FilterAIChatRequestModel request, bool forSubscriber, CancellationToken cancellationToken = default)
    {
        var (endpoint, options) = GetModel();
        try
        {
            return await CreateGenerator(forSubscriber, request.Settings?.SearchUnpublished == true).ChatAsync(endpoint, options, request, Today(), cancellationToken);
        }
        catch (FilterAIException ex)
        {
            throw new BadRequestException(ex.Message, ex);
        }
    }

    /// <summary>
    /// The model an administrator chose for filters, with the same defaults the report engine
    /// applies when a model row leaves the key or deployment empty.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="BadRequestException"></exception>
    private (LlmEndpoint Endpoint, LlmRequestOptions Options) GetModel()
    {
        var setting = _settingService.FindByName(IFilterAIHelper.LLMSettingName);
        if (setting == null || !int.TryParse(setting.Value, out var llmId))
            throw new BadRequestException("An administrator must choose the AI model for filters (Admin > Filters).");
        var llm = _llmService.FindById(llmId) ?? throw new BadRequestException("The AI model chosen for filters no longer exists. An administrator must choose another (Admin > Filters).");
        if (!llm.IsEnabled) throw new BadRequestException($"The AI model '{llm.Name}' chosen for filters is disabled.");
        if (!String.IsNullOrWhiteSpace(llm.AgentName)) throw new BadRequestException($"The AI model '{llm.Name}' is an agent; filters need a deployment-based model.");

        var apiKey = String.IsNullOrWhiteSpace(llm.ApiKey) ? _azureOptions.AI?.ApiKey : llm.ApiKey;
        var deploymentName = String.IsNullOrWhiteSpace(llm.DeploymentName) ? _azureOptions.AI?.DefaultModelDeploymentName : llm.DeploymentName;
        if (llm.ProjectEndpoint == null || String.IsNullOrWhiteSpace(deploymentName)) throw new BadRequestException($"The AI model '{llm.Name}' requires a project endpoint and a deployment name.");
        if (String.IsNullOrWhiteSpace(apiKey)) throw new BadRequestException($"The AI model '{llm.Name}' has no API key configured.");

        return (new LlmEndpoint(llm.ProjectEndpoint, apiKey, deploymentName), new LlmRequestOptions(llm.MaxOutputTokens));
    }

    /// <summary>
    /// A generator whose tools search the content the requestor may see, in the index the filter
    /// itself will search, so what the model checks is what the filter returns.
    /// </summary>
    /// <param name="forSubscriber"></param>
    /// <param name="searchUnpublished">Whether the filter searches unpublished content (drafts) too.</param>
    /// <returns></returns>
    private FilterAIGenerator CreateGenerator(bool forSubscriber, bool searchUnpublished)
    {
        var excludedSourceIds = Array.Empty<int>();
        var excludedMediaTypeIds = Array.Empty<int>();
        if (forSubscriber)
        {
            var user = _impersonate.GetCurrentUser();
            excludedSourceIds = user.SourcesManyToMany.Select(s => s.SourceId).ToArray();
            excludedMediaTypeIds = user.MediaTypesManyToMany.Select(m => m.MediaTypeId).ToArray();
        }
        var tools = new FilterAITools(
            _elasticClient,
            _sourceService,
            _mediaTypeService,
            _seriesService,
            _contributorService,
            _tagService,
            _actionService,
            searchUnpublished ? _elasticOptions.ContentIndex : _elasticOptions.PublishedIndex,
            searchUnpublished ? null : _elasticOptions.ContentIndex,
            excludedSourceIds,
            excludedMediaTypeIds);

        var client = _httpClientFactory.CreateClient();
        client.Timeout = RequestTimeout;
        // Only the editor's filter page applies the body length settings.
        var options = FilterAIOptions.Default with { SupportsBodyLength = !forSubscriber };
        return new FilterAIGenerator(new LlmDirectClient(client, _logger), tools, _serializerOptions, _logger, options);
    }

    /// <summary>
    /// Today in the application's time zone.
    /// </summary>
    /// <returns></returns>
    private DateTime Today() => DateTime.UtcNow.ToTimeZone(_apiOptions.TimeZone).Date;
    #endregion
}
