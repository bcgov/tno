using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Filters;
using TNO.API.Models;
using TNO.Core.Exceptions;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Keycloak;
using TonePoolModel = TNO.API.Areas.Subscriber.Models.TonePool.TonePoolModel;

namespace TNO.API.Areas.Subscriber.Controllers;

///<summary>
///TonePoolController class, provides tone pool endpoints for the api.
///</summary>
///
[ClientRoleAuthorize(ClientRole.Subscriber)]
[ApiController]
[Area("subscriber")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/tonePool")]
[Route("api/[area]/tonePool")]
[Route("v{version:apiVersion}/[area]/tonePool")]
[Route("[area]/tonePool")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]

public class TonePoolController: ControllerBase
{
    #region variables

    private readonly ITonePoolService _service;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a TonePoolController object, initializes with specified parameters.
    /// </summary>
    /// <param name="service"></param>
    ///
    public TonePoolController(ITonePoolService service)
    {
        _service = service;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Return an array of TonePool.
    /// </summary>
    /// <returns></returns>
    [HttpGet, HttpHead]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<TonePoolModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.NotModified)]
    [SwaggerOperation(Tags = new[] { "TonePool" })]
    [ETagCacheTableFilter("tone_pools")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult FindById(int id)
    {
        var result = _service.FindById(id) ?? throw new NoContentException();
        return new JsonResult(new TonePoolModel(result));
    }

    /// <summary>
    /// Return a TonePool by UserId.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [HttpGet("user/{userId}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TonePoolModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "TonePool" })]
    public IActionResult FindByUserId(int userId)
    {
        var result = _service.FindByUserId(userId);
        
        // If no result, return an empty object.
        if (result == null) return new JsonResult(new TonePoolModel());

        return new JsonResult(new TonePoolModel(result));
    }


    /// <summary>
    /// Add a Tone Pool.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    ///
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(TonePoolModel), (int)HttpStatusCode.Created)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.BadRequest)]
    [SwaggerOperation(Tags = new[] { "TonePool" })]
    [ETagCacheTableFilter("tone_pools")]
    [ResponseCache(Duration = 5 * 60)]
    public IActionResult Add(TonePoolModel model)
    {
        var tonePoolEntity = new TonePool(model.Name, model.OwnerId)
        {
            IsPublic = model.IsPublic,
            Description = model.Description,
            IsEnabled = model.IsEnabled,
            SortOrder = model.SortOrder,
            
        };

        var result = _service.AddAndSave(tonePoolEntity);
        result = _service.FindById(result.Id) ?? throw new NoContentException();
        return CreatedAtAction(nameof(FindById), new { id = result.Id }, new TonePoolModel(result));
    }
    #endregion
    
}

