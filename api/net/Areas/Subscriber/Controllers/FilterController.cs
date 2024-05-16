using System.Net;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Areas.Subscriber.Models.Filter;
using TNO.API.Helpers;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Subscriber.Controllers;

/// <summary>
/// FilterController class, provides filter endpoints for the api.
/// </summary>
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
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
    private readonly IUserService _userService;
    private readonly IImpersonationHelper _impersonate;

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a FilterController object, initializes with specified parameters.
    /// </summary>
    /// <param name="filterService"></param>
    /// <param name="serializerOptions"></param>
    /// <param name="userService"></param>
    /// <param name="impersonateHelper"></param>
    public FilterController(
        IFilterService filterService,
        IUserService userService,
        IImpersonationHelper impersonateHelper,
        IOptions<JsonSerializerOptions> serializerOptions)
    {
        _filterService = filterService;
        _serializerOptions = serializerOptions.Value;
        _userService = userService;
        _impersonate = impersonateHelper;
    }
    #endregion

    #region Endpoints
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
    /// Find all "my" filters.
    /// </summary>
    /// <returns></returns>
    [HttpGet("my-filters")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<FilterModel>), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Filter" })]
    public IActionResult FindMyFilters()
    {
        var user = _impersonate.GetCurrentUser();
        return new JsonResult(_filterService.FindMyFilters(user.Id).Select(ds => new FilterModel(ds, _serializerOptions)));
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
        var user = _impersonate.GetCurrentUser();
        model.OwnerId = user.Id;
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
        var user = _impersonate.GetCurrentUser();
        var filter = _filterService.FindById(model.Id) ?? throw new NoContentException("Filter does not exist");
        if (filter.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to delete filter");
        var result = _filterService.UpdateAndSave(model.ToEntity(_serializerOptions));
        filter = _filterService.FindById(result.Id) ?? throw new NoContentException("Filter does not exist");
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
        var user = _impersonate.GetCurrentUser();
        var filter = _filterService.FindById(model.Id) ?? throw new NoContentException("Filter does not exist");
        if (filter.OwnerId != user?.Id) throw new NotAuthorizedException("Not authorized to delete filter");
        _filterService.DeleteAndSave(filter);
        return new JsonResult(model);
    }
    #endregion
}
