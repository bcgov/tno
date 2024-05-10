using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ChartTemplate;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.Core.Extensions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;

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
    private readonly IChartTemplateService _chartTemplateService;
    private readonly IReportHelper _reportHelper;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ChartTemplateController object, initializes with specified parameters.
    /// </summary>
    /// <param name="chartTemplateService"></param>
    /// <param name="reportHelper"></param>
    public ChartTemplateController(
        IChartTemplateService chartTemplateService,
        IReportHelper reportHelper
    )
    {
        _chartTemplateService = chartTemplateService;
        _reportHelper = reportHelper;
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
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public IActionResult FindById(int id)
    {
        var result = _chartTemplateService.FindById(id) ?? throw new NoContentException();
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
        var chart = _chartTemplateService.FindById(result.Id) ?? throw new NoContentException("Chart template does not exist");
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
        var chart = _chartTemplateService.FindById(result.Id) ?? throw new NoContentException("Chart template does not exist");
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
        if (_chartTemplateService.IsInUse(model.Id)) throw new InvalidOperationException("Cannot delete a template in use by a report.");
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
    [ProducesResponseType(typeof(TemplateEngine.Models.Charts.ChartResultModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Chart" })]
    public async Task<IActionResult> PreviewJsonAsync(ChartPreviewRequestModel model)
    {
        var chart = new API.Areas.Admin.Models.Report.ChartTemplateModel()
        {
            Template = model.Template,
            SectionSettings = model.Settings
        };

        // Merge content passed to request and also get content from Elasticsearch if a filter is provided.
        var content = model.Content?.Select(c => new TemplateEngine.Models.ContentModel(c)) ?? Array.Empty<TemplateEngine.Models.ContentModel>();
        if (model.Filter != null && model.Filter.ToJson() != "{}")
            content = content.AppendRange(await _reportHelper.FindContentAsync(model.Filter, model.Index));

        // If this chart pulls data from a linked report add this content.
        var sections = model.LinkedReportId.HasValue ? _reportHelper.GetLinkedReportContent(model.LinkedReportId.Value, null).Result : new();

        var chartTemplate = new TemplateEngine.Models.Reports.ChartEngineContentModel("test", chart, sections, content);
        var preview = await _reportHelper.GenerateJsonAsync(new TemplateEngine.Models.Charts.ChartRequestModel(chartTemplate, model.ChartData), true);
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
        // pie charts should never have scales
        if (model.Settings.ChartType.Equals("pie"))
        {
            var json = JsonNode.Parse(model.Settings.Options.ToJson())?.AsObject();
            if (json != null)
            {
                json.Remove("scales");
                model.Settings.Options = JsonDocument.Parse(json.ToJsonString());
            }
        }

        var chart = new API.Areas.Admin.Models.Report.ChartTemplateModel()
        {
            Template = model.Template,
            SectionSettings = model.Settings
        };

        // Merge content passed to request and also get content from Elasticsearch if a filter is provided.
        var content = model.Content?.Select(c => new TemplateEngine.Models.ContentModel(c)) ?? Array.Empty<TemplateEngine.Models.ContentModel>();
        if (model.Filter != null && model.Filter.ToJson() != "{}")
            content = content.AppendRange(await _reportHelper.FindContentAsync(model.Filter, model.Index));

        // If this chart pulls data from a linked report add this content.
        var sections = model.LinkedReportId.HasValue ? _reportHelper.GetLinkedReportContent(model.LinkedReportId.Value, null).Result : new();

        var chartTemplate = new TemplateEngine.Models.Reports.ChartEngineContentModel("test", chart, sections, content);
        var base64Image = await _reportHelper.GenerateBase64ImageAsync(new TemplateEngine.Models.Charts.ChartRequestModel(chartTemplate, model.ChartData), true);
        return Ok(base64Image);
    }
    #endregion
}
