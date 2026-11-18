using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.IngestType;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Entities.Models;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// IngestTypeController class, provides IngestType endpoints for the admin api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/ingest/types")]
[Route("api/[area]/ingest/types")]
[Route("v{version:apiVersion}/[area]/ingest/types")]
[Route("[area]/ingest/types")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class IngestTypeController : ControllerBase
{
    #region Variables
    private readonly IIngestTypeService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a IngestTypeController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    public IngestTypeController(IIngestTypeService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of ingest type for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<IngestTypeModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult FindAll()
    {
        return new JsonResult(_service.FindAll().Select(ds => new IngestTypeModel(ds)));
    }

    /// <summary>
    /// Find a page of ingest type for the specified query filter.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IPaged<IngestTypeModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new IngestTypeFilter(query));
        var page = new Paged<IngestTypeModel>(result.Items.Select(ds => new IngestTypeModel(ds)), result.Page, result.Quantity, result.Total);
        return new JsonResult(page);
    }

    /// <summary>
    /// Find ingest type for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new IngestTypeModel(result));
    }

    /// <summary>
    /// Add ingest type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestTypeModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult Add([FromBody] IngestTypeModel model)
    {
        var result = _service.AddAndSave((IngestType)model);
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new IngestTypeModel(result));
    }

    /// <summary>
    /// Update ingest type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult Update([FromBody] IngestTypeModel model)
    {
        var result = _service.UpdateAndSave((IngestType)model);
        return new JsonResult(new IngestTypeModel(result));
    }

    /// <summary>
    /// Delete ingest type for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IngestTypeModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "IngestType" })]
    public IActionResult Delete([FromBody] IngestTypeModel model)
    {
        _service.DeleteAndSave((IngestType)model);
        return new JsonResult(model);
    }
    #endregion
}
