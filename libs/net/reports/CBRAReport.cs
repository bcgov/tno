using TNO.Core.Excel;
using TNO.DAL.Services;

namespace TNO.Reports;

/// <summary>
/// CBRAReport class, provides a way to generate the CBRA report.
/// </summary>
public class CBRAReport
{
    #region Variables
    private readonly ITimeTrackingService timeTrackingService;
    private readonly ExcelWorkbook workbook;
    private readonly ExcelSheet sheet;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAReport object.
    /// </summary>
    /// <param name="timeTrackingService"></param>
    public CBRAReport(ITimeTrackingService timeTrackingService)
    {
        this.timeTrackingService = timeTrackingService;
        workbook = new ExcelWorkbook();
        sheet = workbook.CreateSheet("CBRA");
        // Widths are in characters (the former NPOI widths were in 1/256ths of a character).
        sheet.SetColumnWidth(0, 8000 / 256d);
        sheet.SetColumnWidth(1, 4000 / 256d);
        sheet.SetColumnWidth(2, 6000 / 256d);
        sheet.SetColumnWidth(3, 4000 / 256d);
        sheet.SetColumnWidth(4, 4000 / 256d);
        sheet.SetColumnWidth(5, 4000 / 256d);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generates CBRA Excel Report.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public ExcelWorkbook GenerateReport(DateTime from, DateTime? to)
    {
        DateTime utcFrom = from.ToUniversalTime();
        DateTime utcTo = to?.ToUniversalTime() ?? DateTime.Now.ToUniversalTime();

        AddTitle(utcFrom, utcTo);
        var lastRow = AddSpacerRow(3);
        lastRow = AddTotalExcerpts(lastRow + 2, utcFrom, utcTo);
        lastRow = AddSpacerRow(lastRow + 2);
        lastRow = AddStaffSummary(lastRow + 2, utcFrom, utcTo);
        lastRow = AddSpacerRow(lastRow + 2);
        lastRow = AddTotalsByProgram(lastRow + 2, utcFrom, utcTo);
        lastRow = AddSpacerRow(lastRow + 2);
        lastRow = AddTotalsByBroadcaster(lastRow + 2, utcFrom, utcTo);
        lastRow = AddSpacerRow(lastRow + 2);
        AddDatabaseEntries(lastRow + 2, utcFrom, utcTo);

        // sheet.AutoSizeColumn(0);
        // sheet.AutoSizeColumn(1);
        // sheet.AutoSizeColumn(2);
        // sheet.AutoSizeColumn(3);
        // sheet.AutoSizeColumn(4);
        // sheet.AutoSizeColumn(5);

        return workbook;
    }

    private static ExcelCellStyle CreateStyle(double size, bool bold, bool wrap, ExcelHorizontalAlignment horizontalAlignment, string? numberFormat = null)
    {
        return new ExcelCellStyle()
        {
            FontName = "Calibri",
            FontSize = size,
            Bold = bold,
            WrapText = wrap,
            HorizontalAlignment = horizontalAlignment,
            NumberFormat = numberFormat,
        };
    }

    private int AddSpacerRow(int startRowIndex)
    {
        var row = sheet.CreateRow(startRowIndex);
        // Grey 25% fill; 2.5pt tall.
        row.Style = new ExcelCellStyle() { FillColor = "C0C0C0" };
        row.Height = 2.5;

        return startRowIndex;
    }

    private void AddTitle(DateTime from, DateTime to)
    {
        var localFrom = from.ToLocalTime();
        var localTo = to.ToLocalTime();

        var titleStyle = CreateStyle(14, true, false, ExcelHorizontalAlignment.Center);

        var titleRow = sheet.CreateRow(1);
        sheet.AddMergedRegion("A2:E2");
        titleRow.CreateCell(0, titleStyle).SetValue($"CBRA Summary for {localFrom:MM-dd-yyyy} to {localTo:MM-dd-yyyy}");
    }

    private int AddTotalExcerpts(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle(12, true, true, ExcelHorizontalAlignment.Left);
        var labelStyle = CreateStyle(12, false, true, ExcelHorizontalAlignment.Left);
        var valueStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "#,##0");

        var titleRow = sheet.CreateRow(++startRowIndex);
        titleRow.CreateCell(0, titleStyle).SetValue("% of Excerpts which Exceed 10 minutes and/or which do not meet the definition of Qualified Subject Matter");
        sheet.CreateRow(++startRowIndex);

        var totalExcerpts = timeTrackingService.GetTotalExcerpts(from, to);

        foreach (var totalExcerpt in totalExcerpts)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var userKey = totalExcerpt.Category;

            row.CreateCell(0, labelStyle).SetValue(userKey);

            row.CreateCell(1, valueStyle).SetValue((int)totalExcerpt.Totals);
        }
        return startRowIndex;
    }

    private int AddStaffSummary(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle(14, true, false, ExcelHorizontalAlignment.Left);
        var headingStyle = CreateStyle(12, true, false, ExcelHorizontalAlignment.Center);
        var labelStyle = CreateStyle(12, false, true, ExcelHorizontalAlignment.Center);
        var nameStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Left);
        var valueStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "0.00");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion($"A{startRowIndex + 1}:E{startRowIndex + 1}");
        titleRow.CreateCell(0, titleStyle).SetValue("Staff Summary");

        var row1 = sheet.CreateRow(++startRowIndex);
        row1.CreateCell(0, headingStyle).SetValue("Staff");
        row1.CreateCell(1, headingStyle).SetValue("CBRA Hours");

        var row2 = sheet.CreateRow(++startRowIndex);
        row2.CreateCell(0, labelStyle).SetValue("[Username]");
        row2.CreateCell(1, labelStyle).SetValue("[hours]");

        var timeTracking = timeTrackingService.GetStaffSummary(from, to);

        foreach (var timeTrack in timeTracking)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var userKey = timeTrack.Staff;

            row.CreateCell(0, nameStyle).SetValue(userKey);

            row.CreateCell(1, valueStyle).SetValue(timeTrack.CbraHours);
        }
        return startRowIndex;
    }

    private int AddTotalsByProgram(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle(14, true, false, ExcelHorizontalAlignment.Left);
        var headerStyle = CreateStyle(12, true, true, ExcelHorizontalAlignment.Center);
        var nameStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Left);
        var valueStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "0.00");
        var percentageStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "0.00%");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion($"A{startRowIndex + 1}:E{startRowIndex + 1}");
        titleRow.CreateCell(0, titleStyle).SetValue("Total Excerpts and Running Time by Program");

        var row1 = sheet.CreateRow(++startRowIndex);
        row1.CreateCell(0, headerStyle).SetValue("Type");
        row1.CreateCell(1, headerStyle).SetValue("Source");
        row1.CreateCell(2, headerStyle).SetValue("Series");
        row1.CreateCell(3, headerStyle).SetValue("Count");
        row1.CreateCell(4, headerStyle).SetValue("Running Time");
        row1.CreateCell(5, headerStyle).SetValue("% of Total Running Time");

        var totalsByProgram = timeTrackingService.GetTotalsByProgram(from, to);

        foreach (var totalCount in totalsByProgram)
        {
            var row = sheet.CreateRow(++startRowIndex);

            row.CreateCell(0, nameStyle).SetValue(totalCount.MediaType);

            row.CreateCell(1, nameStyle).SetValue(totalCount.SourceType.Trim());

            row.CreateCell(2, nameStyle).SetValue(totalCount.Series.Trim());

            row.CreateCell(3, valueStyle).SetValue((double)totalCount.TotalCount);

            row.CreateCell(4, valueStyle).SetValue(totalCount.TotalRunningTime);

            row.CreateCell(5, percentageStyle).SetValue((double)totalCount.PercentageOfTotalRunningTime);
        }
        return startRowIndex;
    }

    private int AddTotalsByBroadcaster(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle(14, true, false, ExcelHorizontalAlignment.Left);
        var headerStyle = CreateStyle(12, true, true, ExcelHorizontalAlignment.Center);
        var nameStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Left);
        var valueStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "0.00");
        var percentageStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "0.00%");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion($"A{startRowIndex + 1}:E{startRowIndex + 1}");
        titleRow.CreateCell(0, titleStyle).SetValue("% of Total Running Time by Broadcaster");

        var row1 = sheet.CreateRow(++startRowIndex);
        row1.CreateCell(0, headerStyle).SetValue("Source");
        row1.CreateCell(1, headerStyle).SetValue("Running Time");
        row1.CreateCell(2, headerStyle).SetValue("% of Total Running Time");

        var totalsByProgram = timeTrackingService.GetTotalsByBroadcaster(from, to);

        foreach (var totalCount in totalsByProgram)
        {
            var row = sheet.CreateRow(++startRowIndex);

            row.CreateCell(0, nameStyle).SetValue(totalCount.SourceType);

            row.CreateCell(1, valueStyle).SetValue(totalCount.TotalRunningTime);

            row.CreateCell(2, percentageStyle).SetValue((double)totalCount.PercentageOfTotalRunningTime);
        }
        return startRowIndex;
    }

    private int AddDatabaseEntries(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle(14, true, false, ExcelHorizontalAlignment.Left);
        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion($"A{startRowIndex + 1}:E{startRowIndex + 1}");
        titleRow.CreateCell(0, titleStyle).SetValue("Database Entries");

        var headerStyle = CreateStyle(12, true, true, ExcelHorizontalAlignment.Center);
        var nameStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Left);
        var valueStyle = CreateStyle(12, false, false, ExcelHorizontalAlignment.Right, "#,##0");
        var row1 = sheet.CreateRow(++startRowIndex);
        row1.CreateCell(0, headerStyle).SetValue("Day Of Week");
        row1.CreateCell(1, headerStyle).SetValue("Total");
        row1.CreateCell(2, headerStyle).SetValue("CBRA");

        var totalEntries = timeTrackingService.GetTotalEntries(from, to);

        foreach (var totalCount in totalEntries)
        {
            var row = sheet.CreateRow(++startRowIndex);

            row.CreateCell(0, nameStyle).SetValue(totalCount.DayOfWeek);

            row.CreateCell(1, valueStyle).SetValue((double)totalCount.TotalCount);

            row.CreateCell(2, valueStyle).SetValue((double)totalCount.TotalCbra);
        }
        return startRowIndex;
    }
    #endregion
}
