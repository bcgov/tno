using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.ReportTemplate;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Entities;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/report/templates")]
[Route("api/[area]/report/templates")]
[Route("v{version:apiVersion}/[area]/report/templates")]
[Route("[area]/report/templates")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ReportTemplateController : ControllerBase
{
    #region Variables
    private readonly IReportTemplateService _reportTemplateService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ReportTemplateController object, initializes with specified parameters.
    /// </summary>
    /// <param name="reportTemplateService"></param>
    /// <param name="serializerOptions"></param>
    public ReportTemplateController(
        IReportTemplateService reportTemplateService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _reportTemplateService = reportTemplateService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportTemplateModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_reportTemplateService.FindAll().Select(ds => new ReportTemplateModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find content for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id)
    {
        var result = _reportTemplateService.FindById(id);
        if (result == null) return new NoContentResult();
        return new JsonResult(new ReportTemplateModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportTemplateModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Add(ReportTemplateModel model)
    {
        var result = _reportTemplateService.AddAndSave((ReportTemplate)model);
        var report = _reportTemplateService.FindById(result.Id) ?? throw new InvalidOperationException("Report template does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new ReportTemplateModel(report, _serializerOptions));
    }

    /// <summary>
    /// Update content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Update(ReportTemplateModel model)
    {
        var result = _reportTemplateService.UpdateAndSave((ReportTemplate)model);
        var report = _reportTemplateService.FindById(result.Id) ?? throw new InvalidOperationException("Report template does not exist");
        return new JsonResult(new ReportTemplateModel(report, _serializerOptions));
    }

    /// <summary>
    /// Delete content for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult Delete(ReportTemplateModel model)
    {
        // Do not allow deleting a report template that is used by a report.
        if (_reportTemplateService.IsInUse(model.Id)) throw new InvalidOperationException("Cannot delete a template in use by a report.");
        _reportTemplateService.DeleteAndSave((ReportTemplate)model);
        return new JsonResult(model);
    }
    #endregion
}
