using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.Report;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/reports")]
[Route("api/[area]/reports")]
[Route("v{version:apiVersion}/[area]/reports")]
[Route("[area]/reports")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportController : ControllerBase
{
    #region Variables
    private readonly IReportService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public ReportController(IReportService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all of the reports for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new TNO.Models.Filters.ReportFilter(query), false);
        return new JsonResult(result.Select(ds => new ReportModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find report for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id);
        if (result == null) return NoContent();
        return new JsonResult(new ReportModel(result, _serializerOptions));
    }

    /// <summary>
    /// Make a request to Elasticsearch for content that matches the specified report 'id' filter.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="instanceId"></param>
    /// <param name="requestorId"></param>
    /// <returns></returns>
    [HttpGet("{id}/content")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Dictionary<string, Elastic.Models.SearchResultModel<API.Areas.Services.Models.Content.ContentModel>>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public async Task<IActionResult> FindContentForReportIdAsync(int id, long? instanceId, int? requestorId)
    {
        var report = _service.FindById(id);
        if (report == null) return NoContent();
        var results = await _service.FindContentWithElasticsearchAsync(report, instanceId, requestorId);
        return new JsonResult(results);
    }

    /// <summary>
    /// Get the current instance for the specified report 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="ownerId"></param>
    /// <returns></returns>
    [HttpGet("{id}/instance")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportInstanceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult GetCurrentInstance(int id, int? ownerId)
    {
        var instance = _service.GetCurrentReportInstance(id, ownerId, true);
        if (instance == null) return NoContent();
        var report = _service.FindById(id);
        instance.Report = report;
        return new JsonResult(new ReportInstanceModel(instance, _serializerOptions));
    }

    /// <summary>
    /// Clears all content from folders within this report.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NoContentException"></exception>
    [HttpPost("{id}/clear/folders")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult ClearFoldersInReportId(int id)
    {
        var report = _service.FindById(id);
        if (report == null) return NoContent();
        var result = _service.ClearFoldersInReport(report) ?? throw new NoContentException();
        return new JsonResult(new ReportModel(result, _serializerOptions));
    }

    /// <summary>
    /// Get an array of any report instances that CHES has not yet emailed.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="cutOff"></param>
    /// <returns></returns>
    [HttpGet("sent/{status}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<Models.Report.ChesReportMessagesModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult GetChesMessages(Entities.ReportStatus status, [FromQuery] DateTime cutOff)
    {
        var messages = _service.GetChesMessageIds(status, cutOff);
        return new JsonResult(messages);
    }
    #endregion
}
