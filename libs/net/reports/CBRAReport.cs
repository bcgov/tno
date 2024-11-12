using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using TNO.DAL.Services;
using TNO.Entities;
using TNO.Models.Filters;

namespace TNO.Reports;

/// <summary>
/// CBRAReport class, provides a way to generate the CBRA report.
/// </summary>
public class CBRAReport
{
    #region Variables
    private readonly IContentService contentService;
    private readonly ITimeTrackingService timeTrackingService;
    private readonly IActionService actionService;
    private readonly XSSFWorkbook workbook;
    private readonly XSSFSheet sheet;

    private readonly XSSFColor green = new();

    private List<Content> content = new();
    private List<Content> talkRadio = new();
    private Dictionary<string, List<Content>> talkRadioSeries = new();
    private string[] talkRadioKeys = Array.Empty<string>();
    private List<Content> newsRadio = new();
    private string[] newsRadioKeys = Array.Empty<string>();
    private List<Content> television = new();
    private Dictionary<string, List<Content>> newsRadioSources = new();
    private string[] televisionKeys = Array.Empty<string>();
    private Dictionary<string, List<Content>> televisionSeries = new();
    private int maxRows;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a CBRAReport object.
    /// </summary>
    /// <param name="contentService"></param>
    /// <param name="timeTrackingService"></param>
    /// <param name="actionService"></param>
    public CBRAReport(IContentService contentService, ITimeTrackingService timeTrackingService, IActionService actionService)
    {
        this.contentService = contentService;
        this.timeTrackingService = timeTrackingService;
        this.actionService = actionService;
        workbook = new XSSFWorkbook();
        green.ARGBHex = "008000";
        sheet = (XSSFSheet)workbook.CreateSheet("CBRA");
        sheet.SetColumnWidth(0, 6000);
        sheet.SetColumnWidth(1, 4000);
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
        var filter = new ContentFilter
        {
            Page = 1,
            Quantity = 1000,
            UpdatedStartOn = utcFrom,
            UpdatedEndOn = utcTo
        };
        var page = contentService.FindWithDatabase(filter); // TODO: Asking for a page isn't ideal.

        // TODO: This is horrible, but hibernate is a mess. Need to make this more
        // performant.
        content = page.Items;

        // TODO: Hardcoding isn't good.
        talkRadio = content.Where(c => c.MediaType?.Name == "Talk Radio").ToList();
        talkRadioSeries = talkRadio.GroupBy(c => c.Series?.Name).ToDictionary(g => g.Key ?? "NOT SET", g => g.ToList());
        talkRadioKeys = talkRadioSeries.Keys.ToArray();

        // TODO: Hardcoding isn't good.
        newsRadio = content.Where(c => c.MediaType?.Name == "News Radio").ToList();
        newsRadioSources = newsRadio.GroupBy(c => c.OtherSource).ToDictionary(g => g.Key ?? "NOT SET", g => g.ToList());
        newsRadioKeys = newsRadioSources.Keys.ToArray();

        // TODO: Hardcoding isn't good.
        television = content.Where(c => c.MediaType?.Name == "Television").ToList();
        televisionSeries = television.GroupBy(c => c.Series?.Name).ToDictionary(g => g.Key ?? "NOT SET", g => g.ToList());
        televisionKeys = televisionSeries.Keys.ToArray();

        int[] sizes = { talkRadioSeries.Count, newsRadioSources.Count, televisionSeries.Count };
        maxRows = sizes.Max();

        AddTitle(utcFrom, utcTo);
        var lastRow = AddTotalExcerptsByProgram(3);
        lastRow = AddTotalRunningTimeByProgram(lastRow + 3);
        lastRow = AddPercentOfTotalRunningTimeByProgram(lastRow + 3);
        lastRow = AddPercentOfTotalRunningTime(lastRow + 3);
        lastRow = AddSpacerRow(lastRow + 2);
        lastRow = AddTotals(lastRow + 2);
        lastRow = AddSpacerRow(lastRow + 2);
        lastRow = AddStaffSummary(lastRow + 2, utcFrom, utcTo);
        lastRow = AddSpacerRow(lastRow + 2);
        AddDatabaseEntries(lastRow + 2);

        sheet.AutoSizeColumn(0);
        sheet.AutoSizeColumn(1);
        sheet.AutoSizeColumn(2);
        sheet.AutoSizeColumn(3);
        sheet.AutoSizeColumn(4);
        sheet.AutoSizeColumn(5);

        return workbook;
    }

    private static void CreateCell(IRow currentRow, int cellIndex, string value, XSSFCellStyle style)
    {
        var cell = currentRow.CreateCell(cellIndex);
        cell.SetCellValue(value);
        cell.CellStyle = style;
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
        if (color == null)
            color = new XSSFColor
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


    private int AddTotalExcerptsByProgram(int startRowIndex)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Center);

        var totalExcerptsRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf("A4:E4"));
        var totalExcerptCell = totalExcerptsRow.CreateCell(0);
        totalExcerptCell.CellStyle = titleStyle;
        totalExcerptCell.SetCellValue("Total Excerpts by Program");

        var style = CreateStyle((short)12, false, true, HorizontalAlignment.Center);

        var row1 = sheet.CreateRow(++startRowIndex);
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("Talk Radio");
        var cellC = row1.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("News Radio");
        var cellE = row1.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("Television");

        var row2 = sheet.CreateRow(++startRowIndex);
        cellA = row2.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("(Series)");
        var cellB = row2.CreateCell(1);
        cellB.CellStyle = style;
        cellB.SetCellValue("(#)");
        cellC = row2.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("(Source)");
        var cellD = row2.CreateCell(3);
        cellD.CellStyle = style;
        cellD.SetCellValue("(#)");
        cellE = row2.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("(Series)");
        var cellF = row2.CreateCell(5);
        cellF.CellStyle = style;
        cellF.SetCellValue("(#)");

        var nameStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);

        for (var i = 0; i < maxRows; i++)
        {

            var row = sheet.CreateRow(++startRowIndex);

            if (i < talkRadioSeries.Count)
            {
                var talkRadioKey = talkRadioKeys[i];
                var talkRadioRows = talkRadioSeries[talkRadioKey];

                cellA = row.CreateCell(0);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(talkRadioKey);

                cellB = row.CreateCell(1);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(talkRadioRows.Count);
            }

            if (i < newsRadioSources.Count)
            {
                var newsRadioKey = newsRadioKeys[i];
                var newsRadioRows = newsRadioSources[newsRadioKey];

                cellA = row.CreateCell(2);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(newsRadioKey);

                cellB = row.CreateCell(3);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(newsRadioRows.Count);
            }

            if (i < televisionSeries.Count)
            {
                var televisionKey = televisionKeys[i];
                var televisionRows = televisionSeries[televisionKey];

                cellA = row.CreateCell(4);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(televisionKey);

                cellB = row.CreateCell(5);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(televisionRows.Count);
            }

        }

        return startRowIndex;
    }

    private int AddTotalRunningTimeByProgram(int startRowIndex)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Center);

        var totalExcerptsRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var totalExcerptCell = totalExcerptsRow.CreateCell(0);
        totalExcerptCell.CellStyle = titleStyle;
        totalExcerptCell.SetCellValue("Total Running Time by Program");

        var style = CreateStyle((short)12, false, true, HorizontalAlignment.Center);

        var row1 = sheet.CreateRow(++startRowIndex);
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("Talk Radio");
        var cellC = row1.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("News Radio");
        var cellE = row1.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("Television");

        var row2 = sheet.CreateRow(++startRowIndex);
        cellA = row2.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("(Series)");
        var cellB = row2.CreateCell(1);
        cellB.CellStyle = style;
        cellB.SetCellValue("(#)");
        cellC = row2.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("(Source)");
        var cellD = row2.CreateCell(3);
        cellD.CellStyle = style;
        cellD.SetCellValue("(#)");
        cellE = row2.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("(Series)");
        var cellF = row2.CreateCell(5);
        cellF.CellStyle = style;
        cellF.SetCellValue("(#)");

        var nameStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00");

        for (var i = 0; i < maxRows; i++)
        {

            var row = sheet.CreateRow(++startRowIndex);

            if (i < talkRadioSeries.Count)
            {
                var talkRadioKey = talkRadioKeys[i];
                var talkRadioRows = talkRadioSeries[talkRadioKey];

                cellA = row.CreateCell(0);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(talkRadioKey);

                var total = talkRadioRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTime(total);
                cellB = row.CreateCell(1);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

            if (i < newsRadioSources.Count)
            {
                var newsRadioKey = newsRadioKeys[i];
                var newsRadioRows = newsRadioSources[newsRadioKey];

                cellA = row.CreateCell(2);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(newsRadioKey);

                var total = newsRadioRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTime(total);
                cellB = row.CreateCell(3);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

            if (i < televisionSeries.Count)
            {
                var televisionKey = televisionKeys[i];
                var televisionRows = televisionSeries[televisionKey];

                cellA = row.CreateCell(4);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(televisionKey);

                var total = televisionRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTime(total);
                cellB = row.CreateCell(5);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

        }

        return startRowIndex;
    }

    private int AddPercentOfTotalRunningTimeByProgram(int startRowIndex)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Center);

        var totalExcerptsRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var totalExcerptCell = totalExcerptsRow.CreateCell(0);
        totalExcerptCell.CellStyle = titleStyle;
        totalExcerptCell.SetCellValue("% of Total Running Time by Program");

        var style = CreateStyle((short)12, false, true, HorizontalAlignment.Center);

        var row1 = sheet.CreateRow(++startRowIndex);
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("Talk Radio");
        var cellC = row1.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("News Radio");
        var cellE = row1.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("Television");

        var row2 = sheet.CreateRow(++startRowIndex);
        cellA = row2.CreateCell(0);
        cellA.CellStyle = style;
        cellA.SetCellValue("(Series)");
        var cellB = row2.CreateCell(1);
        cellB.CellStyle = style;
        cellB.SetCellValue("(%)");
        cellC = row2.CreateCell(2);
        cellC.CellStyle = style;
        cellC.SetCellValue("(Source)");
        var cellD = row2.CreateCell(3);
        cellD.CellStyle = style;
        cellD.SetCellValue("(%)");
        cellE = row2.CreateCell(4);
        cellE.CellStyle = style;
        cellE.SetCellValue("(Series)");
        var cellF = row2.CreateCell(5);
        cellF.CellStyle = style;
        cellF.SetCellValue("(%)");

        var totalTalkRadio = talkRadio.Where((c) => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
        var totalNewsRadio = newsRadio.Where((c) => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
        var totalTelevision = television.Where((c) => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);

        var nameStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Left, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        for (var i = 0; i < maxRows; i++)
        {
            var row = sheet.CreateRow(++startRowIndex);

            if (i < talkRadioSeries.Count)
            {
                var talkRadioKey = talkRadioKeys[i];
                var talkRadioRows = talkRadioSeries[talkRadioKey];

                cellA = row.CreateCell(0);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(talkRadioKey);

                var totalSeries = talkRadioRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTimePercent(totalSeries, totalTalkRadio);
                cellB = row.CreateCell(1);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

            if (i < newsRadioSources.Count)
            {
                var newsRadioKey = newsRadioKeys[i];
                var newsRadioRows = newsRadioSources[newsRadioKey];

                cellA = row.CreateCell(2);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(newsRadioKey);

                var totalSource = newsRadioRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTimePercent(totalSource, totalNewsRadio);
                cellB = row.CreateCell(3);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

            if (i < televisionSeries.Count)
            {
                var televisionKey = televisionKeys[i];
                var televisionRows = televisionSeries[televisionKey];

                cellA = row.CreateCell(4);
                cellA.CellStyle = nameStyle;
                cellA.SetCellValue(televisionKey);

                var totalSeries = televisionRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
                var time = CalculateRunningTimePercent(totalSeries, totalTelevision);
                cellB = row.CreateCell(5);
                cellB.CellStyle = valueStyle;
                cellB.SetCellValue(time);
            }

        }

        return startRowIndex;
    }

    private int AddPercentOfTotalRunningTime(int startRowIndex)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Center);

        var totalExcerptsRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var totalExcerptCell = totalExcerptsRow.CreateCell(0);
        totalExcerptCell.CellStyle = titleStyle;
        totalExcerptCell.SetCellValue("% of Total Running Time");

        var style = CreateStyle((short)12, false, true, HorizontalAlignment.Center);

        var row1 = sheet.CreateRow(++startRowIndex);
        var cellA = row1.CreateCell(1);
        cellA.CellStyle = style;
        cellA.SetCellValue("(Source)");
        var cellB = row1.CreateCell(2);
        cellB.CellStyle = style;
        cellB.SetCellValue("(%)");

        var totalRunningTime = content.Where((c) => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
        var sources = newsRadio.GroupBy(c => c.OtherSource).ToDictionary(g => g.Key, g => g.ToList());
        var sourceKeys = sources.Keys.ToArray();

        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Center, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        for (var i = 0; i < sources.Count; i++)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var sourceKey = sourceKeys[i];
            var sourceRows = sources[sourceKey];

            var name = sourceKey ?? "NOT SET";
            cellA = row.CreateCell(0);
            cellA.CellStyle = valueStyle;
            cellA.SetCellValue(name);

            var totalSource = sourceRows.Where(c => c.FileReferences.Count > 0).Sum(c => c.FileReferences.First().RunningTime);
            var time = CalculateRunningTimePercent(totalSource, totalRunningTime);
            cellB = row.CreateCell(1);
            cellB.CellStyle = valueStyle;
            cellB.SetCellValue(time);
        }

        return startRowIndex;
    }

    private int AddTotals(int startRowIndex)
    {
        var labelStyle = CreateStyle((short)12, true, true, HorizontalAlignment.Left);
        var typeStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Center);
        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Right, green);
        var valuePercentStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Right, green);
        valuePercentStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        // Total number of Excerpts
        var row1 = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:C{startRowIndex + 1}"));
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("Total number of Excerpts");

        var cellB = row1.CreateCell(3);
        cellB.CellStyle = typeStyle;
        cellB.SetCellValue("(#)");

        var cellC = row1.CreateCell(4);
        cellC.CellStyle = valueStyle;
        cellC.SetCellValue(content.Count);

        // Total number of Excerpts which do not meet the definition of Qualified
        // Subject Matter
        row1 = sheet.CreateRow(startRowIndex += 2);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:C{startRowIndex + 1}"));
        row1.Height = 580;
        cellA = row1.CreateCell(0);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("Total Number of Excerpts which do not meet the definition of Qualified Subject Matter");

        cellB = row1.CreateCell(3);
        cellB.CellStyle = typeStyle;
        cellB.SetCellValue("(#)");

        var action = actionService.FindByName("Non Qualified Subject");
        var totalNonQualified = content.Where((c) => c.ActionsManyToMany.Any((a) => a.ActionId == action?.Id)).Count();
        cellC = row1.CreateCell(4);
        cellC.CellStyle = valueStyle;
        cellC.SetCellValue(totalNonQualified);

        // Total number of Excerpts over 10 minutes
        row1 = sheet.CreateRow(startRowIndex += 2);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:C{startRowIndex + 1}"));
        cellA = row1.CreateCell(0);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("Total number of Excerpts over 10 minutes");

        cellB = row1.CreateCell(3);
        cellB.CellStyle = typeStyle;
        cellB.SetCellValue("(#)");

        var tenMinutes = 1000 * 60 * 10;
        var totalOver10Minutes = content.Where(c => c.FileReferences.Any(fr => fr.RunningTime > tenMinutes)).Count();
        cellC = row1.CreateCell(4);
        cellC.CellStyle = valueStyle;
        cellC.SetCellValue(totalOver10Minutes);

        // Percentage over 10 minutes
        row1 = sheet.CreateRow(startRowIndex += 2);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:C{startRowIndex + 1}"));
        cellA = row1.CreateCell(0);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("Percentage over 10 minutes");

        cellB = row1.CreateCell(3);
        cellB.CellStyle = typeStyle;
        cellB.SetCellValue("(%)");

        var percentage = totalOver10Minutes > 0 ? totalOver10Minutes / content.Count : 0;
        cellC = row1.CreateCell(4);
        cellC.CellStyle = valuePercentStyle;
        cellC.SetCellValue(percentage);

        return startRowIndex;
    }

    private int AddStaffSummary(int startRowIndex, DateTime from, DateTime to)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var headingStyle = CreateStyle((short)12, true, false, HorizontalAlignment.Center);
        var labelStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Center);
        var nameStyle = CreateStyle((short)12, true, false, HorizontalAlignment.Left, green);
        var valueStyle = CreateStyle((short)12, true, false, HorizontalAlignment.Right, green);
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

        var timeTracking = timeTrackingService.Find(from, to).GroupBy(tt => tt.User?.Username).ToDictionary(g => g.Key ?? "NOT SET", g => g.ToList());
        var timeKeys = timeTracking.Keys.ToArray();

        for (var i = 0; i < timeTracking.Count; i++)
        {
            var row = sheet.CreateRow(++startRowIndex);
            var userKey = timeKeys[i];
            var userRows = timeTracking[userKey];

            cellA = row.CreateCell(0);
            cellA.CellStyle = nameStyle;
            cellA.SetCellValue(userKey);

            var totalEffort = userRows.Sum(u => u.Effort);
            cellB = row.CreateCell(1);
            cellB.CellStyle = valueStyle;
            cellB.SetCellValue(totalEffort);
        }

        return startRowIndex;
    }

    private int AddDatabaseEntries(int startRowIndex)
    {
        var titleStyle = CreateStyle((short)14, true, false, HorizontalAlignment.Left);
        var headingStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Left);
        var labelStyle = CreateStyle((short)12, false, false, HorizontalAlignment.Center);
        var valueStyle = CreateStyle((short)12, false, true, HorizontalAlignment.Center, green);
        valueStyle.DataFormat = workbook.CreateDataFormat().GetFormat("0.00%");

        var titleRow = sheet.CreateRow(startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue("Database Entries");

        var row1 = sheet.CreateRow(++startRowIndex);
        sheet.AddMergedRegion(CellRangeAddress.ValueOf($"A{startRowIndex + 1}:E{startRowIndex + 1}"));
        var cellA = row1.CreateCell(0);
        cellA.CellStyle = headingStyle;
        cellA.SetCellValue("Percentage of CBRA Database Entries");

        var row2 = sheet.CreateRow(++startRowIndex);
        cellA = row2.CreateCell(1);
        cellA.CellStyle = labelStyle;
        cellA.SetCellValue("(%)");

        var total = talkRadioSeries.Count + newsRadioSources.Count + televisionSeries.Count;
        var percentage = total <= 0 ? 0 : total / content.Count;
        var row3 = sheet.CreateRow(++startRowIndex);
        cellA = row3.CreateCell(1);
        cellA.CellStyle = valueStyle;
        cellA.SetCellValue(percentage);

        return startRowIndex;
    }

    private static double CalculateRunningTimePercent(long runningTime, long totalRunningTime)
    {
        if (runningTime <= 0)
            return runningTime;

        return CalculateRunningTime(runningTime) / CalculateRunningTime(totalRunningTime);
    }

    private static double CalculateRunningTime(long runningTime)
    {
        return ToFloat(ToDuration(runningTime));
    }

    private static TimeSpan ToDuration(long milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    private static double ToFloat(TimeSpan duration)
    {
        var totalMinutes = duration.TotalMinutes;
        if (totalMinutes <= 0)
            return 0f;
        var hours = totalMinutes / 60;
        var minutes = totalMinutes % 60;
        return hours + ((double)minutes / 60);
    }
    #endregion
}
