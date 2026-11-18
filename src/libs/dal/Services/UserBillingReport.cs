using TNO.Core.Excel;
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
    public ExcelWorkbook GenerateReport()
    {
        var workbook = new ExcelWorkbook();
        var sheet = workbook.CreateSheet("AccountBilling");
        // Widths are in characters (the former NPOI widths were in 1/256ths of a character).
        sheet.SetColumnWidth(0, 4000 / 256d); //name
        sheet.SetColumnWidth(1, 7000 / 256d); //email
        sheet.SetColumnWidth(2, 4000 / 256d); //account type
        sheet.SetColumnWidth(3, 6000 / 256d); //ministry
        sheet.SetColumnWidth(4, 8000 / 256d); //reports
        sheet.SetColumnWidth(5, 4000 / 256d); //start date
        sheet.SetColumnWidth(6, 4000 / 256d); //end date
        sheet.SetColumnWidth(7, 11000 / 256d); //history
        sheet.SetColumnWidth(8, 10000 / 256d); //notes
        AddDataSheet(0, sheet);

        return workbook;
    }

    private static ExcelCellStyle CreateStyle(double size, bool bold, bool wrap, ExcelHorizontalAlignment horizontalAlignment,
        ExcelVerticalAlignment verticalAlignment)
    {
        return new ExcelCellStyle()
        {
            FontName = "Calibri",
            FontSize = size,
            Bold = bold,
            WrapText = wrap,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment,
        };
    }

    private int AddDataSheet(int startRowIndex, ExcelSheet sheet)
    {
        var headerStyle = CreateStyle(12, true, true, ExcelHorizontalAlignment.Center, ExcelVerticalAlignment.Top);
        var cellStyle = CreateStyle(12, false, true, ExcelHorizontalAlignment.Left, ExcelVerticalAlignment.Top);
        var rowIndex = startRowIndex;
        var row1 = sheet.CreateRow(rowIndex);
        row1.CreateCell(0, headerStyle).SetValue("Name");
        row1.CreateCell(1, headerStyle).SetValue("Email");
        row1.CreateCell(2, headerStyle).SetValue("Account Type");
        row1.CreateCell(3, headerStyle).SetValue("Ministry");
        row1.CreateCell(4, headerStyle).SetValue("Reports");
        row1.CreateCell(5, headerStyle).SetValue("Billing Start Date");
        row1.CreateCell(6, headerStyle).SetValue("Billing End Date");
        row1.CreateCell(7, headerStyle).SetValue("Billing History");
        row1.CreateCell(8, headerStyle).SetValue("Notes");

        var userList = userService.GetUserUpdateHistory();
        var orgList = organizationService.FindAll();

        foreach (var user in userList)
        {
            rowIndex = ++rowIndex;
            var row = sheet.CreateRow(rowIndex);

            row.CreateCell(0, cellStyle).SetValue(user.Username);

            row.CreateCell(1, cellStyle).SetValue(user.Email);

            row.CreateCell(2, cellStyle).SetValue(user.AccountType.ToString());

            row.CreateCell(3, cellStyle).SetValue(user.Organizations.Select(x => x.Name).ToList().FirstOrDefault());

            row.CreateCell(4, cellStyle).SetValue(String.Join(", ", user.Reports.Select(x => x.Name).ToList()));

            row.CreateCell(5, cellStyle).SetValue(user.CreatedOn.ToString("dd-MMM-yyyy"));

            var col7 = row.CreateCell(6, cellStyle);
            if (user.UserUpdateHistory?.Count() > 0 && user.UserUpdateHistory.Any(x => x.ChangeType == UserChangeType.Disable))
            {
                col7.SetValue(user.UserUpdateHistory.LastOrDefault(x => x.ChangeType == UserChangeType.Disable)?.DateOfChange.ToString("dd-MMM-yyyy") ?? "");
            }
            else
            {
                col7.SetValue("");
            }

            var col8 = row.CreateCell(7, cellStyle);
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
            col8.SetValue(historyString);

            row.CreateCell(8, cellStyle).SetValue(user.Note);
        }
        return startRowIndex;
    }
    #endregion
}
