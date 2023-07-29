using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ChartTemplate;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Entities;
using TNO.TemplateEngine;
using Microsoft.Extensions.Options;
using TNO.Elastic;
using TNO.Core.Extensions;
using TNO.Core.Http;
using TNO.API.Config;
using System.Text.Json;
using System.Text;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ChartController class, provides Chart endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/chart/templates")]
[Route("api/[area]/chart/templates")]
[Route("v{version:apiVersion}/[area]/chart/templates")]
[Route("[area]/chart/templates")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ChartTemplateController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IChartTemplateService _chartTemplateService;
    private readonly ITemplateEngine<TNO.TemplateEngine.Models.Reports.ChartTemplateModel> _templateEngine;
    private readonly IHttpRequestClient _httpClient;
    private readonly ElasticOptions _elasticOptions;
    private readonly ChartsOptions _chartsOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartTemplateController object, initializes with specified parameters.
    /// </summary>
    /// <param name="chartTemplateService"></param>
    /// <param name="contentService"></param>
    /// <param name="templateEngine"></param>
    /// <param name="httpClient"></param>
    /// <param name="elasticOptions"></param>
    /// <param name="chartsOptions"></param>
    public ChartTemplateController(
        IChartTemplateService chartTemplateService,
        IContentService contentService,
        ITemplateEngine<TNO.TemplateEngine.Models.Reports.ChartTemplateModel> templateEngine,
        IHttpRequestClient httpClient,
        IOptions<ElasticOptions> elasticOptions,
        IOptions<ChartsOptions> chartsOptions
        )
    {
        _chartTemplateService = chartTemplateService;
        _contentService = contentService;
        _templateEngine = templateEngine;
        _httpClient = httpClient;
        _elasticOptions = elasticOptions.Value;
        _chartsOptions = chartsOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ChartTemplateModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_chartTemplateService.FindAll().Select(ds => new ChartTemplateModel(ds)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ChartTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult FindById(int id)
    {
        var result = _chartTemplateService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ChartTemplateModel(result));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ChartTemplateModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult Add(ChartTemplateModel model)
    {
        var result = _chartTemplateService.AddAndSave((ChartTemplate)model);
        var chart = _chartTemplateService.FindById(result.Id) ?? throw new InvalidOperationException("Chart template does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ChartTemplateModel(chart));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ChartTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult Update(ChartTemplateModel model)
    {
        var result = _chartTemplateService.UpdateAndSave((ChartTemplate)model);
        var chart = _chartTemplateService.FindById(result.Id) ?? throw new InvalidOperationException("Chart template does not exist");
        return new JsonResult(new ChartTemplateModel(chart));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ChartTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult Delete(ChartTemplateModel model)
    {
        // Do not allow deleting a chart template that is used by a chart.
        if (_chartTemplateService.IsInUse(model.Id)) throw new InvalidOperationException("Cannot delete a template in use by a chart.");
        _chartTemplateService.DeleteAndSave((ChartTemplate)model);
        return new JsonResult(model);
    }

    /// <summary>
    /// Generate the chart JSON based on the specified template and filter.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("preview/json")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ChartPreviewResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public async Task<IActionResult> PreviewJsonAsync(ChartPreviewRequestModel model)
    {
        var preview = await GenerateJsonAsync(model);
        return new JsonResult(preview);
    }

    /// <summary>
    /// Generate the chart image based on the specified template and filter.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("preview/base64")]
    // [Produces(MediaTypeNames.Text.Plain)] // TODO: Figure out how to use a Produces with text/plain.
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.OK, MediaTypeNames.Text.Plain)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public async Task<IActionResult> GenerateBase64Async(ChartPreviewRequestModel model)
    {
        // Get the Chart JSON data.
        var data = model.ChartData ?? (await GenerateJsonAsync(model)).Json;
        var dataJson = data.ToJson();

        var optionsJson = model.Settings.Options != null ? JsonSerializer.Serialize(model.Settings.Options) : "{}";
        var optionsBytes = System.Text.Encoding.UTF8.GetBytes(optionsJson);
        var optionsBase64 = Convert.ToBase64String(optionsBytes);

        // Send request to Charts API to generate base64
        var body = new StringContent(dataJson, Encoding.UTF8, MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync(
            _chartsOptions.Url.Append(_chartsOptions.Base64Path, model.Settings.ChartType ?? "bar", $"?width={model.Width ?? 250}&height={model.Height ?? 250}&options={optionsBase64}"),
            body);
        var result = await response.Content.ReadAsStringAsync();
        return Ok(result);
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Generate the Chart JSON for the specified 'model' containing a template and content.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<ChartPreviewResultModel> GenerateJsonAsync(ChartPreviewRequestModel model)
    {
        if (model.Template == null) throw new InvalidOperationException("Chart template is missing from model");
        var template = _templateEngine.AddOrUpdateTemplateInMemory($"chart-template", model.Template);

        Elastic.Models.SearchResultModel<Services.Models.Content.ContentModel>? results = null;
        IEnumerable<TNO.TemplateEngine.Models.Reports.ContentModel> content;
        if (model.Filter != null)
        {
            results = await _contentService.FindWithElasticsearchAsync(model.Index ?? _elasticOptions.PublishedIndex, model.Filter);
            content = results.Hits.Hits.Select(h => new TNO.TemplateEngine.Models.Reports.ContentModel(h.Source)).ToArray();
        }
        else if (model.Content?.Any() == true)
        {
            content = model.Content.Select(c => new TNO.TemplateEngine.Models.Reports.ContentModel(c)).ToArray();
        }
        else
        {
            content = Array.Empty<TNO.TemplateEngine.Models.Reports.ContentModel>();
        }

        var templateModel = new TNO.TemplateEngine.Models.Reports.ChartTemplateModel(content, model.Settings);

        var json = await template.RunAsync(instance =>
        {
            instance.Model = templateModel;
            instance.Content = templateModel.Content;
            instance.Sections = templateModel.Sections;
            instance.Settings = templateModel.Settings;
        });

        return new ChartPreviewResultModel(json, results);
    }
    #endregion
}
