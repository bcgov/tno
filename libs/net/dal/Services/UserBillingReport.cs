using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TNO.Entities;

namespace TNO.DAL.Services;

/// <summary>
/// UserBillingReport class, provides a way to generate the user billing report.
/// </summary>
public class UserBillingReport
{
    #region Variables
    private readonly IUserService userService;
    private readonly IOrganizationService organizationService;

    private readonly XSSFColor green = new();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a UserBillingReport object.
    /// </summary>
    /// <param name="userService"></param>
    /// <param name="organizationService"></param>
    public UserBillingReport(IUserService userService, IOrganizationService organizationService)
    {
        this.userService = userService;
        this.organizationService = organizationService;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generates user billing Excel Report.
    /// </summary>
    /// <returns></returns>
    public XSSFWorkbook GenerateReport()
    {
        XSSFWorkbook workbook = new XSSFWorkbook();
        green.ARGBHex = "008000";
        XSSFSheet sheet = (XSSFSheet)workbook.CreateSheet("AccountBilling");
        sheet.SetColumnWidth(0, 4000); //name
        sheet.SetColumnWidth(1, 7000); //email
        sheet.SetColumnWidth(2, 4000); //account type
        sheet.SetColumnWidth(3, 6000); //ministry
        sheet.SetColumnWidth(4, 8000); //reports
        sheet.SetColumnWidth(5, 4000); //start date
        sheet.SetColumnWidth(6, 4000); //end date
        sheet.SetColumnWidth(7, 11000); //history
        sheet.SetColumnWidth(8, 10000); //notes
        AddDataSheet(0, workbook, sheet);

        return workbook;
    }

    private IFont CreateFont(double size, bool bold, XSSFColor color, XSSFWorkbook workbook)
    {
        var font = workbook.CreateFont();
        font.FontName = "Calibri";
        font.FontHeightInPoints = size;
        font.IsBold = bold;
        font.Color = color.Index;
        return font;
    }

    private ICellStyle CreateStyle(double size, bool bold, bool wrap, HorizontalAlignment horizontalAlignment,
        VerticalAlignment verticalAlignment, XSSFWorkbook workbook, XSSFColor? color = null)
    {
        color ??= new XSSFColor
        {
            ARGBHex = "000000"
        };

        var font = CreateFont(size, bold, color, workbook);
        var style = workbook.CreateCellStyle();
        style.SetFont(font);
        style.WrapText = wrap;
        style.Alignment = horizontalAlignment;
        style.VerticalAlignment = verticalAlignment;
        return style;
    }

    private int AddDataSheet(int startRowIndex, XSSFWorkbook workbook, XSSFSheet sheet)
    {
        var headerStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Center, VerticalAlignment.Top, workbook);
        var cellStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, VerticalAlignment.Top, workbook, green);
        cellStyle.WrapText = true;
        var rowIndex = startRowIndex;
        var row1 = sheet.CreateRow(rowIndex);
        var col1 = row1.CreateCell(0);
        col1.CellStyle = headerStyle;
        col1.SetCellValue("Name");
        var col2 = row1.CreateCell(1);
        col2.CellStyle = headerStyle;
        col2.SetCellValue("Email");
        var col3 = row1.CreateCell(2);
        col3.CellStyle = headerStyle;
        col3.SetCellValue("Account Type");
        var col4 = row1.CreateCell(3);
        col4.CellStyle = headerStyle;
        col4.SetCellValue("Ministry");
        var col5 = row1.CreateCell(4);
        col5.CellStyle = headerStyle;
        col5.SetCellValue("Reports");
        var col6 = row1.CreateCell(5);
        col6.CellStyle = headerStyle;
        col6.SetCellValue("Billing Start Date");
        var col7 = row1.CreateCell(6);
        col7.CellStyle = headerStyle;
        col7.SetCellValue("Billing End Date");
        var col8 = row1.CreateCell(7);
        col8.CellStyle = headerStyle;
        col8.SetCellValue("Billing History");
        var col9 = row1.CreateCell(8);
        col9.CellStyle = headerStyle;
        col9.SetCellValue("Notes");

        var userList = userService.GetUserUpdateHistory();
        var orgList = organizationService.FindAll();

        foreach (var user in userList)
        {
            rowIndex = ++rowIndex;
            var row = sheet.CreateRow(rowIndex);

            col1 = row.CreateCell(0);
            col1.CellStyle = cellStyle;
            col1.SetCellValue(user.Username);

            col2 = row.CreateCell(1);
            col2.CellStyle = cellStyle;
            col2.SetCellValue(user.Email);

            col3 = row.CreateCell(2);
            col3.CellStyle = cellStyle;
            col3.SetCellValue(user.AccountType.ToString());

            col4 = row.CreateCell(3);
            col4.CellStyle = cellStyle;
            col4.SetCellValue(user.Organizations.Select(x => x.Name).ToList().FirstOrDefault());

            col5 = row.CreateCell(4);
            col5.CellStyle = cellStyle;
            col5.SetCellValue(String.Join(", ", user.Reports.Select(x => x.Name).ToList()));

            col6 = row.CreateCell(5);
            col6.CellStyle = cellStyle;
            col6.SetCellValue(user.CreatedOn.ToString("dd-MMM-yyyy"));

            col7 = row.CreateCell(6);
            col7.CellStyle = cellStyle;
            if (user.UserUpdateHistory?.Count() > 0 && user.UserUpdateHistory.Any(x => x.ChangeType == UserChangeType.Disable))
            {
                col7.SetCellValue(user.UserUpdateHistory.LastOrDefault(x => x.ChangeType == UserChangeType.Disable)?.DateOfChange.ToString("dd-MMM-yyyy") ?? "");
            }
            else
            {
                col7.SetCellValue("");
            }

            col8 = row.CreateCell(7);
            col8.CellStyle = cellStyle;
            var historyList = user.UserUpdateHistory?.OrderBy(x => x.DateOfChange);

            var historyQuery =
                from history in historyList
                join org in orgList on history.Value equals org.Id.ToString() into ho
                from subgroup in ho.DefaultIfEmpty()
                select new
                {
                    history.DateOfChange,
                    history.Value,
                    history.UpdatedBy,
                    history.ChangeType,
                    Organization = subgroup?.Name ?? history.Value
                };
            var historyString = $"{user.CreatedOn.ToString("dd-MMM-yy")} Account created by {user.CreatedBy}";
            if (historyQuery != null)
            {
                foreach (var history in historyQuery)
                {
                    switch (history.ChangeType)
                    {
                        case Entities.UserChangeType.AccountType:
                            historyString += $"\n{history.DateOfChange.ToString("dd-MMM-yy")} Account type changed to {history.Value}";
                            break;
                        case Entities.UserChangeType.Disable:
                            historyString += $"\n{history.DateOfChange.ToString("dd-MMM-yy")} Account deactivated by {history.UpdatedBy}";
                            break;
                        case Entities.UserChangeType.Enable:
                            historyString += $"\n{history.DateOfChange.ToString("dd-MMM-yy")} Account reactivated by {history.UpdatedBy}";
                            break;
                        case Entities.UserChangeType.Organization:
                            historyString += $"\n{history.DateOfChange.ToString("dd-MMM-yy")} Ministry changed to {history.Organization}";
                            break;
                        default:
                            break;
                    }
                }
            }
            col8.SetCellValue(historyString);

            col9 = row.CreateCell(8);
            col9.CellStyle = cellStyle;
            col9.SetCellValue(user.Note);
        }
        return startRowIndex;
    }
    #endregion
}
