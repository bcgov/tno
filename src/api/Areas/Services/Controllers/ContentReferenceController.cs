using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Services.Models.ContentReference;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;
using TNO.Models.Filters;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContentReferenceController class, provides ContentReference endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/content/references")]
[Route("api/[area]/content/references")]
[Route("v{version:apiVersion}/[area]/content/references")]
[Route("[area]/content/references")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContentReferenceController : ControllerBase
{
    #region Variables
    private readonly IContentReferenceService _service;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContentReferenceController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    /// <param name="serializerOptions"></param>
    public ContentReferenceController(IContentReferenceService service, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _service = service;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find a page of content references for the specified query filter.
    /// Supports the same query parameters as the admin endpoint (e.g. 'sources', 'publishedStartOn', 'page', 'quantity').
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<ContentReferenceModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _service.Find(new ContentReferenceFilter(query));
        return new JsonResult(result.Items.Select(reference => new ContentReferenceModel(reference, _serializerOptions)).ToArray());
    }

    /// <summary>
    /// Find a page of content for the specified query filter.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="uid"></param>
    /// <returns></returns>
    [HttpGet("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.NoContent)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult FindByKey(string source, [FromQuery] string uid)
    {
        var reference = _service.FindByKey(source, uid);
        if (reference == null) return NoContent();
        return new JsonResult(new ContentReferenceModel(reference, _serializerOptions));
    }

    /// <summary>
    /// Add new content reference to database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Add([FromBody] ContentReferenceModel model)
    {
        var result = _service.AddAndSave(model.ToEntity(_serializerOptions));
        return CreatedAtAction(nameof(FindByKey), new { source = result.Source, uid = result.Uid }, new ContentReferenceModel(result, _serializerOptions));
    }

    /// <summary>
    /// Update content reference in database.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{source}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(ContentReferenceModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "ContentReference" })]
    public IActionResult Update([FromBody] ContentReferenceModel model)
    {
        var result = _service.UpdateAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(new ContentReferenceModel(result, _serializerOptions));
    }
    #endregion
}
