using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Swashbuckle.AspNetCore.Annotations;
using TNO.API.Models;
using TNO.DAL.Services;

namespace TNO.API.Areas.Admin.Controllers;

/// <summary>
/// CBRAController class, provides CBRA endpoints for the api.
/// </summary>
[Authorize]
[ApiController]
[Area("reports")]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[area]/cbra")]
[Route("api/[area]/cbra")]
[Route("v{version:apiVersion}/[area]/cbra")]
[Route("[area]/cbra")]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Unauthorized)]
[ProducesResponseType(typeof(ErrorResponseModel), (int)HttpStatusCode.Forbidden)]
public class CBRAController : ControllerBase
{
    #region Variables
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private readonly IActionService _actionService;
    private readonly JsonSerializerOptions _serializerOptions;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAController object, initializes with specified parameters.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="userService"></param>
    /// <param name="actionService"></param>
    /// <param name="serializerOptions"></param>
    public CBRAController(IContentService contentService, IUserService userService, IActionService actionService, IOptions<JsonSerializerOptions> serializerOptions)
    {
        _contentService = contentService;
        _userService = userService;
        _actionService = actionService;
        _serializerOptions = serializerOptions.Value;
    }
    #endregion

    #region Endpoints
    /// <summary>
    /// Generate an Excel document CBRA report.
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [SwaggerOperation(Tags = new[] { "CBRA" })]
    public FileResult FindAll()
    {
        var workbook = new XSSFWorkbook();
        var font = (XSSFFont)workbook.CreateFont();
        font.FontHeightInPoints = 11;
        font.FontName = "Tahoma";

        var borderedCellStyle = (XSSFCellStyle)workbook.CreateCellStyle();
        borderedCellStyle.SetFont(font);
        borderedCellStyle.BorderLeft = BorderStyle.Medium;
        borderedCellStyle.BorderTop = BorderStyle.Medium;
        borderedCellStyle.BorderRight = BorderStyle.Medium;
        borderedCellStyle.BorderBottom = BorderStyle.Medium;
        borderedCellStyle.VerticalAlignment = VerticalAlignment.Center;

        var sheet = workbook.CreateSheet("Report");
        var headerRow = sheet.CreateRow(0);

        CreateCell(headerRow, 0, "BatchName", borderedCellStyle);

        using var stream = new MemoryStream();
        workbook.Write(stream);
        var bytes = stream.ToArray();

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cbra.xlsx");
    }
    #endregion

    #region Methods
    private static void CreateCell(IRow currentRow, int cellIndex, string value, XSSFCellStyle style)
    {
        var cell = currentRow.CreateCell(cellIndex);
        cell.SetCellValue(value);
        cell.CellStyle = style;
    }
    #endregion
}
