using System.Text.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using TNO.API.Areas.Services.Models.ReportInstance;

namespace TNO.Reports;

/// <summary>
/// ReportXlsExport class, provides a way to export a report to XLS.
/// </summary>
public class ReportXlsExport
{
    #region Variables
    private readonly XSSFWorkbook _workbook;
    private readonly JsonSerializerOptions _serializerOptions;
    private readonly XSSFSheet _sheet;
    private readonly XSSFColor _green = new();
    private readonly ICellStyle _regularStyle;
    private readonly ICellStyle _boldStyle;
    #endregion

    #region Constructors
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    /// <param name="sheetName"></param>
    /// <param name="serializerOptions"></param>
    public ReportXlsExport(string sheetName, JsonSerializerOptions serializerOptions)
    {
        _serializerOptions = serializerOptions;
        _workbook = new XSSFWorkbook();
        _green.ARGBHex = "008000";
        _sheet = (XSSFSheet)_workbook.CreateSheet(sheetName);
        _regularStyle = CreateStyle(10, false, false, HorizontalAlignment.Right);
        _boldStyle = CreateStyle(10, true, false, HorizontalAlignment.Right);
    }
    #endregion

    #region Methods
    /// <summary>
    /// Creates a new instance of a Report object.
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="groupBy">Whether to group by month, otherwise it will be grouped by day.</param>
    /// <returns>XSSFWorkbook</returns>
    public XSSFWorkbook GenerateExcel(Entities.ReportInstance instance, string? groupBy = "yyyy-MM")
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        int totalColumns = 0; ;
        var serialized = new ReportInstanceModel(instance, _serializerOptions);
        var content = serialized.Content.ToList().OrderBy(o => o.Content?.PublishedOn);
        var tonesCount = new TonesCount();

        content.Where(c => c.Content != null).ToList().ForEach((i) =>
            {
                // TODO: Handle user tone pools.
                var tonePool = i.Content!.TonePools?.FirstOrDefault(t => t.Name == "Default" && t.IsPublic);
                int tone = tonePool?.Value ?? 0;
                if (tone > 0)
                {
                    tonesCount.Positive++;
                }
                else if (tone < 0)
                {
                    tonesCount.Negative++;
                }
                else
                {
                    tonesCount.Neutral++;
                }
            });

        var productTable = content
            .Where(c => c.Content != null)
            .Select(s => new
            {
                Name = s.Content!.MediaType?.Name ?? "",
                s.Content!.PublishedOn,
                s.Content!.TonePools
            })
            .GroupBy(t => new { t.Name, PublishedOn = t.PublishedOn?.ToString(groupBy) }).Select(g =>
            {
                if (g.Key.PublishedOn != null)
                {
                    return new GroupResult(
                        g.Key.Name,
                        g.Key.PublishedOn,
                        g.Count(),
                        Math.Round(g.Average(x => x.TonePools.FirstOrDefault(t => t.Name == "Default" && t.IsPublic)?.Value ?? 0), 2)
                    );
                }
                else
                {
                    return new GroupResult("", "", 0, 0);
                }
            })
            .ToArray();

        var groupedByProductDate = productTable.GroupBy(t => new { t.Date, t.Name }).Select(g => new GroupResult(
            g.Key.Name,
            g.Key.Date,
            g.Sum(c => c == null ? 0 : c.Count),
            g.Sum(c => c == null ? 0 : c.Tone)
        )).ToList();

        var productHeaders = productTable.GroupBy(t => t.Name).Select(g => g.Key).ToList();
        var productDateRows = productTable.GroupBy(t => t.Date).Select(g => g.Key).ToList();
        int index = 0;

        AddTitle(index, serialized?.Report?.Name);
        index += 2;

        index = BuildTable(index, productHeaders, productDateRows, groupedByProductDate, tonesCount);

        index += 2;

        var sectionTable = content
            .Where(c => c.Content != null)
            .Select(s => new
            {
                Section = s.Content!.Section ?? "",
                s.Content!.PublishedOn,
                s.Content!.TonePools
            }).
            GroupBy(t => new { t.Section, PublishedOn = t.PublishedOn?.ToString(groupBy) }).Select(g =>
            {
                if (g.Key.PublishedOn != null)
                {
                    return new GroupResult(
                        String.IsNullOrWhiteSpace(g.Key.Section) ? "No Section" : g.Key.Section,
                        g.Key.PublishedOn,
                        g.Count(),
                        Math.Round(g.Average(x => x.TonePools.FirstOrDefault(t => t.Name == "Default" && t.IsPublic)?.Value ?? 0), 2)
                        );
                }
                else
                {
                    return new GroupResult("", "", 0, 0);
                }
            });

        var groupedBySectionDate = sectionTable.GroupBy(t => new { t.Date, t.Name }).Select(g => new GroupResult(
            g.Key.Name,
            g.Key.Date,
            g.Sum(c => c == null ? 0 : c.Count),
            g.Sum(c => c == null ? 0 : c.Tone)
        )).ToList();

        var sectionHeaders = sectionTable.GroupBy(t => t.Name).Select(g => g.Key).ToList();
        var sectionDateRows = sectionTable.GroupBy(t => t.Date).Select(g => g.Key).ToList();

        totalColumns = sectionHeaders.Count >= productHeaders.Count ? sectionHeaders.Count : productHeaders.Count;

        index = BuildTable(index, sectionHeaders, sectionDateRows, groupedBySectionDate, tonesCount);

        var headlines = content.Select(h =>
        {
            if (h.Content != null && h.Content.PublishedOn != null)
            {
                // TODO: Handle user tone pools.
                var tone = h.Content.TonePools.FirstOrDefault(t => t.Name == "Default" && t.IsPublic);
                return new HeadlineResult(
                  tone?.Value,
                  h.Content.PublishedOn.Value.ToString("yyyy-MM-dd"),
                  h.Content.Headline,
                  h.Content.ContentType.ToString(),
                  h.Content.Source?.Name,
                  h.Content.Series?.Name
                );
            }
            else
            {
                return new HeadlineResult(0, "", "", "", "", "");
            }
        }).ToList();

        index += 2;

        BuildHeadLines(index, headlines);

        //Set Columns width
        for (int i = 0; i <= (totalColumns * 2 + 10); i++)
        {
            _sheet.SetColumnWidth(i, 13 * 256 + 200);
        }

        return _workbook;
    }

    private void BuildHeadLines(int rowIndex, List<HeadlineResult> headlines)
    {
        if (headlines == null)
        {
            return;
        }

        string[] headlineHeaders = { "Tone", "Date", "Headline", "Type", "Source", "Show/Program" };
        IRow rowHeader = _sheet.CreateRow(rowIndex);
        int cellIndex = 0;
        foreach (string h in headlineHeaders)
        {
            ICell cellA = rowHeader.CreateCell(cellIndex);
            cellA.CellStyle = _boldStyle;
            cellA.SetCellValue(h);
            cellIndex++;
        }
        rowIndex++;
        foreach (HeadlineResult r in headlines)
        {
            cellIndex = 0;
            IRow row1 = _sheet.CreateRow(rowIndex);
            if (r.Tone.HasValue)
                AddNumberCell(row1, cellIndex, _regularStyle, r.Tone.Value);
            cellIndex++;
            AddTextCell(row1, cellIndex, _regularStyle, r.Date);
            cellIndex++;
            AddTextCell(row1, cellIndex, _regularStyle, r.Headline);
            cellIndex++;
            AddTextCell(row1, cellIndex, _regularStyle, r.Type);
            cellIndex++;
            AddTextCell(row1, cellIndex, _regularStyle, r.Source);
            cellIndex++;
            AddTextCell(row1, cellIndex, _regularStyle, r.Series);
            rowIndex++;
        }
    }

    private IFont CreateFont(double size, bool bold, XSSFColor color)
    {
        IFont font = _workbook.CreateFont();
        font.FontName = "Verdana";
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

        IFont font = CreateFont(size, bold, color);
        ICellStyle style = _workbook.CreateCellStyle();
        style.SetFont(font);
        style.WrapText = wrap;
        style.Alignment = horizontalAlignment;
        return style;
    }

    private void AddTitle(int index, string? text)
    {
        ICellStyle titleStyle = CreateStyle(14, true, false, HorizontalAlignment.Center);

        IRow titleRow = _sheet.CreateRow(index);
        _ = _sheet.AddMergedRegion(CellRangeAddress.ValueOf("A1:E1"));
        ICell titleCell = titleRow.CreateCell(0);
        titleCell.CellStyle = titleStyle;
        titleCell.SetCellValue(text);
    }

    private static ICell AddTextCell(IRow row, int cellIndex, ICellStyle style, string? value)
    {
        ICell cellT = row.CreateCell(cellIndex);
        cellT.CellStyle = style;
        cellT.SetCellValue(value);
        return cellT;
    }

    private static ICell AddNumberCell(IRow row, int cellIndex, ICellStyle style, double value)
    {
        ICell cellT = row.CreateCell(cellIndex);
        cellT.CellStyle = style;
        cellT.SetCellValue(value);
        return cellT;
    }

    private static ICell AddFormulaCell(IRow row, int cellIndex, ICellStyle style, string formula)
    {
        ICell cellT = row.CreateCell(cellIndex);
        cellT.CellStyle = style;
        cellT.SetCellType(CellType.Formula);
        cellT.SetCellFormula(formula);
        return cellT;
    }

    private void AddHeader(int rowIndex, int columnIndex, List<string> headers)
    {
        int cellIndex = columnIndex;
        IRow row1 = _sheet.CreateRow(rowIndex);

        AddTextCell(row1, cellIndex, _regularStyle, "Date");
        cellIndex++;
        foreach (string h in headers)
        {
            AddTextCell(row1, cellIndex, _boldStyle, h);
            cellIndex++;
        }
        AddTextCell(row1, cellIndex, _boldStyle, "Total");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Average");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Mean");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Date");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Combined");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Date");
        cellIndex++;
        foreach (string h in headers)
        {
            AddTextCell(row1, cellIndex, _boldStyle, h);
            cellIndex++;
        }
        AddTextCell(row1, cellIndex, _boldStyle, "Total");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Average");
        cellIndex++;
        AddTextCell(row1, cellIndex, _boldStyle, "Mean");
    }

    private int BuildTable(int rowIndex, List<string> headers, List<string> dates, List<GroupResult> groups, TonesCount tonesCount)
    {
        AddHeader(rowIndex, 0, headers);
        rowIndex++;
        var firstDataRow = rowIndex;
        int combinedCellIndex = 0;
        CellAddress firstCellCount = new(0, 0);
        CellAddress lastCellCount = new(0, 0);
        CellAddress firstCellTone = new(0, 0);
        CellAddress lastCellTone = new(0, 0);
        foreach (var date in dates)
        {
            int index = 0;
            IRow row1 = _sheet.CreateRow(rowIndex);
            AddTextCell(row1, index, _regularStyle, date);
            bool isFirstCell = true;
            foreach (var h in headers)
            {
                int count = 0;
                foreach (var g in groups)
                {
                    if (g.Name == h && g.Date == date)
                    {
                        count += g.Count;
                    }
                }
                index++;
                ICell cellA = AddNumberCell(row1, index, _regularStyle, count);
                if (isFirstCell)
                {
                    firstCellCount = cellA.Address;
                    isFirstCell = false;
                }
                lastCellCount = cellA.Address;
            }
            // Set the cells to apply formulas
            CellAddress formulaTopAddress = new(lastCellCount.Row, firstCellCount.Column);
            CellAddress formulaBottomAddress = new(lastCellCount.Row, lastCellCount.Column);
            // Add the Sum column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("SUM({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Add the Average column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("AVERAGE({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Add the Median column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("MEDIAN({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Repeat Date Column
            index++;
            AddTextCell(row1, index, _regularStyle, date);
            // Repeat the Sum for the combined Column
            index++;
            combinedCellIndex = index;
            AddFormulaCell(row1, index, _regularStyle, string.Format("SUM({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Repeat Date Column Again
            index++;
            AddTextCell(row1, index, _regularStyle, date);
            rowIndex++;
            // Roll again but calculating the tones
            isFirstCell = true;
            foreach (var h in headers)
            {
                double tone = 0;
                foreach (var g in groups)
                {
                    if (g.Name == h && g.Date == date)
                    {
                        tone += g.Tone;
                    }
                }
                index++;
                ICell cellA = AddNumberCell(row1, index, _regularStyle, tone);
                if (isFirstCell)
                {
                    firstCellTone = cellA.Address;
                    isFirstCell = false;
                }
                lastCellTone = cellA.Address;
            }
            // Set the cells to apply formulas
            formulaTopAddress = new(lastCellTone.Row, firstCellTone.Column);
            formulaBottomAddress = new(lastCellTone.Row, lastCellTone.Column);
            // Add the Sum column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("SUM({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Add the Average column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("AVERAGE({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            // Add the Median column
            index++;
            AddFormulaCell(row1, index, _regularStyle, string.Format("MEDIAN({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
        }
        int cellIndex = 0;
        IRow rowTotal = _sheet.CreateRow(rowIndex);
        rowIndex++;
        IRow rowAverage = _sheet.CreateRow(rowIndex);
        rowIndex++;
        IRow rowMedian = _sheet.CreateRow(rowIndex);
        rowIndex++;
        AddTextCell(rowTotal, cellIndex, _boldStyle, "Total");
        AddTextCell(rowAverage, cellIndex, _boldStyle, "Average");
        AddTextCell(rowMedian, cellIndex, _boldStyle, "Mean");
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellCount.Column + (cellIndex - 1));
            CellAddress formulaBottomAddress = new(lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowTotal, cellIndex, _boldStyle, string.Format("SUM({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }
        cellIndex = 0;
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellCount.Column + (cellIndex - 1));
            CellAddress formulaBottomAddress = new(lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowAverage, cellIndex, _boldStyle, string.Format("AVERAGE({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }
        cellIndex = 0;
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellCount.Column + (cellIndex - 1));
            CellAddress formulaBottomAddress = new(lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowMedian, cellIndex, _boldStyle, string.Format("MEDIAN({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }

        cellIndex += 5;
        int cellStartIndex = cellIndex;
        AddTextCell(rowTotal, cellIndex, _boldStyle, "Total");
        AddTextCell(rowAverage, cellIndex, _boldStyle, "Average");
        AddTextCell(rowMedian, cellIndex, _boldStyle, "Mean");

        // Add Row Tone total
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            CellAddress formulaBottomAddress = new(lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowTotal, cellIndex, _regularStyle, string.Format("SUM({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }

        // Add Row Tone average
        cellIndex = cellStartIndex;
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            CellAddress formulaBottomAddress = new(lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowAverage, cellIndex, _regularStyle, string.Format("AVERAGE({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }

        // Add Row Tone median
        cellIndex = cellStartIndex;
        cellIndex++;
        foreach (var h in headers)
        {
            CellAddress formulaTopAddress = new(firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            CellAddress formulaBottomAddress = new(lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowMedian, cellIndex, _regularStyle, string.Format("MEDIAN({0}:{1})", formulaTopAddress.ToString(), formulaBottomAddress.ToString()));
            cellIndex++;
        }

        IRow toneCountLabels = _sheet.CreateRow(rowIndex);
        AddTextCell(toneCountLabels, combinedCellIndex, _boldStyle, "Positive");
        AddTextCell(toneCountLabels, combinedCellIndex + 1, _boldStyle, "Neutral");
        AddTextCell(toneCountLabels, combinedCellIndex + 2, _boldStyle, "Negative");
        rowIndex++;
        IRow toneCount = _sheet.CreateRow(rowIndex);
        AddNumberCell(toneCount, combinedCellIndex, _regularStyle, tonesCount.Positive);
        AddNumberCell(toneCount, combinedCellIndex + 1, _regularStyle, tonesCount.Neutral);
        AddNumberCell(toneCount, combinedCellIndex + 2, _regularStyle, tonesCount.Negative);

        return rowIndex;
    }

    #endregion
}
