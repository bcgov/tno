using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Admin.Models.Filter;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// FilterController class, provides filter endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("admin")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/filters")]
[Route("api/[area]/filters")]
[Route("v{version:apiVersion}/[area]/filters")]
[Route("[area]/filters")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class FilterController : ControllerBase
{
    #region Variables
    private readonly IFilterService _filterService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterController object, initializes with specified parameters.
    /// </summary>
    /// <param name="filterService"></param>
    /// <param name="serializerOptions"></param>
    public FilterController(
        IFilterService filterService,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _filterService = filterService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Find all filters.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<FilterModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult Find()
    {
        var uri = new Uri(this.Request.GetDisplayUrl());
        var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
        var result = _filterService.Find(new TNO.Models.Filters.FilterFilter(query));
        return new JsonResult(result.Select(ds => new FilterModel(ds, _serializerOptions)));
    }

    /// <summary>
    /// Find filter for the specified 'id'.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FilterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult FindById(int id)
    {
        var result = _filterService.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new FilterModel(result, _serializerOptions));
    }

    /// <summary>
    /// Add filter for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FilterModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult Add(FilterModel model)
    {
        var result = _filterService.AddAndSave(model.ToEntity(_serializerOptions));
        var filter = _filterService.FindById(result.Id) ?? throw new NoContentException("Filter does not exist");
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new FilterModel(filter, _serializerOptions));
    }

    /// <summary>
    /// Update filter for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FilterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult Update(FilterModel model)
    {
        var result = _filterService.UpdateAndSave(model.ToEntity(_serializerOptions));
        var filter = _filterService.FindById(result.Id) ?? throw new NoContentException("Filter does not exist");
        return new JsonResult(new FilterModel(filter, _serializerOptions));
    }

    /// <summary>
    /// Delete filter for the specified 'id'.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(FilterModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult Delete(FilterModel model)
    {
        _filterService.DeleteAndSave(model.ToEntity(_serializerOptions));
        return new JsonResult(model);
    }
    #endregion
}
