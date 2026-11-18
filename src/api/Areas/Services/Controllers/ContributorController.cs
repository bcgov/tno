using System.Net;
using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;
using TNO.Keycloak;

namespace TNO.API.Areas.Services.Controllers;

/// <summary>
/// ContributorController class, provides contributor endpoints for services (e.g. the automation
/// service creating a columnist it extracted).
/// </summary>
[ClientRoleAuthorize(ClientRole.Administrator)]
[ApiController]
[Area("services")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/contributors")]
[Route("api/[area]/contributors")]
[Route("v{version:apiVersion}/[area]/contributors")]
[Route("[area]/contributors")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class ContributorController : ControllerBase
{
    #region Variables
    private readonly IContributorService _contributorService;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a ContributorController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contributorService"></param>
    public ContributorController(IContributorService contributorService)
    {
        _contributorService = contributorService;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Add the contributor when no enabled contributor matches the name (case-insensitive, name
    /// or alias); otherwise return the existing match.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Areas.Services.Models.Contributor.ContributorModel), (int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "Contributor" })]
    public IActionResult AddOrMatch([FromBody] Areas.Services.Models.Contributor.ContributorModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name))
            return new BadRequestObjectResult(new { error = "A name is required." });
        var name = model.Name.Trim();
        var existing = _contributorService.FindAll().FirstOrDefault(c => c.IsEnabled
            && (c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || (c.Aliases ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Any(alias => alias.Equals(name, StringComparison.OrdinalIgnoreCase))));
        var contributor = existing ?? _contributorService.AddAndSave(new Entities.Contributor(name) { IsEnabled = true });
        return new JsonResult(new Areas.Services.Models.Contributor.ContributorModel(contributor));
    }
    #endregion
}
