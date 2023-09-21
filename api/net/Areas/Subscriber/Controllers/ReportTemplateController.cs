using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.ReportTemplate;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// ReportController class, provides Report endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    /// Find all public report templates.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ReportTemplateModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindPublic()
    {
        return new JsonResult(_reportTemplateService.FindPublic().Select(ds => new ReportTemplateModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find report template for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ReportTemplateModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
    [SwaggerOperation(Tags = new[] { "Report" })]
    public IActionResult FindById(int id)
    {
        var result = _reportTemplateService.FindById(id) ?? throw new NoContentException();
        if (!result.IsPublic) throw new NotAuthorizedException("Not authorized to access report template");
        return new JsonResult(new ReportTemplateModel(result, _serializerOptions));
    }
    #endregion
}
