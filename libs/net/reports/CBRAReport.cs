using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using TNO.DAL.Services;

namespace TNO.Reports;

/// <summary>
/// CBRAReport class, provides a way to generate the CBRA report.
/// </summary>
public class CBRAReport
{
    #region Variables
    private readonly ITimeTrackingService timeTrackingService;
    private readonly XSSFWorkbook workbook;
    private readonly XSSFSheet sheet;

    private readonly XSSFColor green = new();

    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAReport object.
    /// </summary>
    /// <param name="timeTrackingService"></param>
    public CBRAReport(ITimeTrackingService timeTrackingService)
    {
        this.timeTrackingService = timeTrackingService;
        workbook = new XSSFWorkbook();
        green.ARGBHex = "008000";
        sheet = (XSSFSheet)workbook.CreateSheet("CBRA");
        sheet.SetColumnWidth(0, 8000);
        sheet.SetColumnWidth(1, 4000);
        sheet.SetColumnWidth(2, 6000);
        sheet.SetColumnWidth(3, 4000);
        sheet.SetColumnWidth(4, 4000);
        sheet.SetColumnWidth(5, 4000);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Generates CBRA Excel Report.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public XSSFWorkbook GenerateReport(DateTime from, DateTime? to)
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

    private IFont CreateFont(double size, bool bold, XSSFColor color)
    {
        var font = workbook.CreateFont();
        font.FontName = "Calibri";
        font.FontHeightInPoints = size;
        font.IsBold = bold;
        font.Color = color.Index;
        return font;
    }

    private ICellStyle CreateStyle(double size, bool bold, bool wrap, HorizontalAlignment horizontalAlignment, XSSFColor? color = null)
    {
        color ??= new XSSFColor
        {
            ARGBHex = "000000"
        };

        var font = CreateFont(size, bold, color);
        var style = workbook.CreateCellStyle();
        style.SetFont(font);
        style.WrapText = wrap;
        style.Alignment = horizontalAlignment;
        return style;
    }

    private int AddSpacerRow(int startRowIndex)
    {
        var style = workbook.CreateCellStyle();
        style.FillForegroundColor = IndexedColors.Grey25Percent.Index;
        style.FillPattern = FillPattern.SolidForeground;

        var row = sheet.CreateRow(startRowIndex);
        row.RowStyle = style;
        row.Height = 50;

        return startRowIndex;
    }

    private void AddTitle(DateTime from, DateTime to)
    {
        var localFrom = from.ToLocalTime();
        var localTo = to.ToLocalTime();

        var titleStyle = CreateStyle(14, true, false, HorizontalAlignment.Center);

        var titleRow = sheet.CreateRow(1);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf("A2:E2"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue($"CBRA Summary for {localFrom:MM-dd-yyyy} to {localTo:MM-dd-yyyy}");
    }

    private int AddTotalExcerpts(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Left);
        var labelStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left);
        var valueStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

        var titleRow = sheet.CreateRow(++startRowIndex);
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("% of Excerpts which Exceed 10 minutes and/or which do not meet the definition of Qualified Subject Matter");
        sheet.CreateRow(++startRowIndex);

        var totalExcerpts = timeTrackingService.GetTotalExcerpts(from, to);

        foreach (var totalExcerpt in totalExcerpts)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var userKey = totalExcerpt.Category;

            var cellA = row.CreateCell(0);
            cellA.CellStyle = labelStyle;
            cellA.SetCellValue(userKey);

            var cellB = row.CreateCell(1);
            cellB.CellStyle = valueStyle;
            cellB.SetCellValue((int)totalExcerpt.Totals);
        }
        return startRowIndex;
    }

    private int AddStaffSummary(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var headingStyle = CreateStyle((short)12, true, false, HorizontalAlignment.Center);
        var labelStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Center);
        var nameStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("Staff Summary");

        var row1 = sheet.CreateRow(++startRowIndex);
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = headingStyle;
        cellA.SetCellValue("Staff");
        var cellB = row1.CreateCell(1);
        cellB.CellStyle = headingStyle;
        cellB.SetCellValue("CBRA Hours");

        var row2 = sheet.CreateRow(++startRowIndex);
        cellA = row2.CreateCell(0);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("[Username]");
        cellB = row2.CreateCell(1);
        cellB.CellStyle = labelStyle;
        cellB.SetCellValue("[hours]");

        var timeTracking = timeTrackingService.GetStaffSummary(from, to);

        foreach (var timeTrack in timeTracking)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var userKey = timeTrack.Staff;

            cellA = row.CreateCell(0);
            cellA.CellStyle = nameStyle;
            cellA.SetCellValue(userKey);

            cellB = row.CreateCell(1);
            cellB.CellStyle = valueStyle;
            cellB.SetCellValue(timeTrack.CbraHours);
        }
        return startRowIndex;
    }

    private int AddTotalsByProgram(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var headerStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Center);
        var nameStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");
        var percentageStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        percentageStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("Total Excerpts and Running Time by Program");

        var row1 = sheet.CreateRow(++startRowIndex);
        var col1 = row1.CreateCell(0);
        col1.CellStyle = headerStyle;
        col1.SetCellValue("Type");
        var col2 = row1.CreateCell(1);
        col2.CellStyle = headerStyle;
        col2.SetCellValue("Source");
        var col3 = row1.CreateCell(2);
        col3.CellStyle = headerStyle;
        col3.SetCellValue("Series");
        var col4 = row1.CreateCell(3);
        col4.CellStyle = headerStyle;
        col4.SetCellValue("Count");
        var col5 = row1.CreateCell(4);
        col5.CellStyle = headerStyle;
        col5.SetCellValue("Running Time");
        var col6 = row1.CreateCell(5);
        col6.CellStyle = headerStyle;
        col6.SetCellValue("% of Total Running Time");

        var totalsByProgram = timeTrackingService.GetTotalsByProgram(from, to);

        foreach (var totalCount in totalsByProgram)
        {
            var row = sheet.CreateRow(++startRowIndex);

            var colCell1 = row.CreateCell(0);
            colCell1.CellStyle = nameStyle;
            colCell1.SetCellValue(totalCount.MediaType);

            var colCell2 = row.CreateCell(1);
            colCell2.CellStyle = nameStyle;
            colCell2.SetCellValue(totalCount.SourceType.Trim());

            var colCell3 = row.CreateCell(2);
            colCell3.CellStyle = nameStyle;
            colCell3.SetCellValue(totalCount.Series.Trim());

            var colCel4 = row.CreateCell(3);
            colCel4.CellStyle = valueStyle;
            colCel4.SetCellValue((double)totalCount.TotalCount);

            var colCell5 = row.CreateCell(4);
            colCell5.CellStyle = valueStyle;
            colCell5.SetCellValue(totalCount.TotalRunningTime);

            var colCel6 = row.CreateCell(5);
            colCel6.CellStyle = percentageStyle;
            colCel6.SetCellValue((double)totalCount.PercentageOfTotalRunningTime);
        }
        return startRowIndex;
    }

    private int AddTotalsByBroadcaster(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var headerStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Center);
        var nameStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");
        var percentageStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        percentageStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("% of Total Running Time by Broadcaster");

        var row1 = sheet.CreateRow(++startRowIndex);
        var col1 = row1.CreateCell(0);
        col1.CellStyle = headerStyle;
        col1.SetCellValue("Source");
        var col2 = row1.CreateCell(1);
        col2.CellStyle = headerStyle;
        col2.SetCellValue("Running Time");
        var col3 = row1.CreateCell(2);
        col3.CellStyle = headerStyle;
        col3.SetCellValue("% of Total Running Time");

        var totalsByProgram = timeTrackingService.GetTotalsByBroadcaster(from, to);

        foreach (var totalCount in totalsByProgram)
        {
            var row = sheet.CreateRow(++startRowIndex);

            col1 = row.CreateCell(0);
            col1.CellStyle = nameStyle;
            col1.SetCellValue(totalCount.SourceType);

            col2 = row.CreateCell(1);
            col2.CellStyle = valueStyle;
            col2.SetCellValue(totalCount.TotalRunningTime);

            col3 = row.CreateCell(2);
            col3.CellStyle = percentageStyle;
            col3.SetCellValue((double)totalCount.PercentageOfTotalRunningTime);
        }
        return startRowIndex;
    }

    private int AddDatabaseEntries(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("Database Entries");

        var headerStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Center);
        var nameStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Right, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");
        var row1 = sheet.CreateRow(++startRowIndex);
        var col1 = row1.CreateCell(0);
        col1.CellStyle = headerStyle;
        col1.SetCellValue("Day Of Week");
        var col2 = row1.CreateCell(1);
        col2.CellStyle = headerStyle;
        col2.SetCellValue("Total");
        var col3 = row1.CreateCell(2);
        col3.CellStyle = headerStyle;
        col3.SetCellValue("CBRA");

        var totalEntries = timeTrackingService.GetTotalEntries(from, to);

        foreach (var totalCount in totalEntries)
        {
            var row = sheet.CreateRow(++startRowIndex);

            col1 = row.CreateCell(0);
            col1.CellStyle = nameStyle;
            col1.SetCellValue(totalCount.DayOfWeek);

            col2 = row.CreateCell(1);
            col2.CellStyle = valueStyle;
            col2.SetCellValue((double)totalCount.TotalCount);

            col3 = row.CreateCell(2);
            col3.CellStyle = valueStyle;
            col3.SetCellValue((double)totalCount.TotalCbra);
        }
        return startRowIndex;
    }
    #endregion
}
