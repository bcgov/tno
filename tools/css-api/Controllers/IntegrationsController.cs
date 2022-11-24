using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.CSS.Models;

namespace TNO.CSS.API.Controllers;

/// <summary>
///
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("v{version:apiVersion}/[controller]")]
public class IntegrationsController : ControllerBase
{
    #region Variables
    #endregion

    #region Constructors
    /// <summary>
    ///
    /// </summary>
    public IntegrationsController()
    {
    }
    #endregion

    #region Endpoints
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IEnumerable<IntegrationModel>), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "integrations" })]
    public IActionResult Get()
    {
        return new JsonResult(new[] { new IntegrationModel() });
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id}")]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IntegrationModel), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.UnprocessableEntity)]
    [SwaggerOperation(Tags = new[] { "integrations" })]
    public IActionResult Get(int id)
    {
        return new JsonResult(new IntegrationModel() { Id = id });
    }
    #endregion
}
