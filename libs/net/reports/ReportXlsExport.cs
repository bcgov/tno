using System.Text.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using TNO.API.Areas.Services.Models.ReportInstance;
using TNO.DAL.Services;

namespace TNO.Reports;

class GroupResult
{
    public GroupResult (string name, string date, int count, double tone) {
        this.Name = name;
        this.Date = date;
        this.Count = count;
        this.Tone = tone;
    }
    public string Name {get; set;}
    public string Date {get; set;}
    public int Count {get; set;}
    public double Tone {get; set;}
}

class HeadlineResult
{
    public HeadlineResult (double tone, string date, string headline, string type, string? source, string? series) {
        this.Tone = tone;
        this.Date = date;
        this.Headline = headline;
        this.Type = type;
        this.Source = source;
        this.Series = series;
    }
    public double Tone {get; set;}
    public string Date {get; set;}
    public string Headline {get; set;}
    public string Type {get; set;}
    public string? Source {get; set;}
    public string? Series {get; set;}
}

class TonesCount
{    
    public TonesCount()
    {
        this.Neutral = 0;
        this.Positive = 0;
        this.Negative = 0;
    }
    public int Neutral {get; set;}
    public int Positive {get; set;}
    public int Negative {get; set;}
}

/// <summary>
/// ReportXlsExport class, provides a way to export a report to XLS.
/// </summary>
public class ReportXlsExport
{
  #region Variables

  private readonly IReportInstanceService _reportInstanceService;
  private readonly XSSFWorkbook workbook;
  private readonly JsonSerializerOptions _serializerOptions;
  private readonly XSSFSheet sheet;
  private readonly XSSFColor green = new();
  private ICellStyle regularStyle;
  private ICellStyle boldStyle;
  #endregion

  #region Constructors
  /// <summary>
  /// Creates a new instance of a Report object.
  /// </summary>
  public ReportXlsExport(IReportInstanceService reportInstanceService, JsonSerializerOptions serializerOptions)
  {
    this._reportInstanceService = reportInstanceService;
    this._serializerOptions = serializerOptions;
    workbook = new XSSFWorkbook();
    green.ARGBHex = "008000";
    sheet = (XSSFSheet)workbook.CreateSheet("CBRA");
    this.regularStyle = CreateStyle(10, false, false, HorizontalAlignment.Right);
    this.boldStyle = CreateStyle(10, true, false, HorizontalAlignment.Right);
  }
  #endregion

  #region Methods
        /// <summary>
        /// Creates a new instance of a Report object.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>XSSFWorkbook</returns>
        public XSSFWorkbook GenerateReport(int id)
        {
            int totalColumns = 0;
            var instance = _reportInstanceService.FindById(id);
            var serialized = instance != null ? new ReportInstanceModel(instance, _serializerOptions) : null;

             if (serialized == null) {
                return workbook;
            }

            var content =  serialized.Content.ToList().OrderBy(o => o.Content?.PublishedOn);

            var tonesCount = new TonesCount();

            content.
            Select(s => new {
                s.Content?.MediaType?.Name,
                s.Content?.PublishedOn,
                s.Content?.TonePools
            }).ToList().ForEach((i) => {
                int tone = i.TonePools != null ? i.TonePools.Sum(y => y.Value) : 0;
                if(tone > 0) {
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

            var productTable = content.
            Select(s => new {
                s.Content?.MediaType?.Name,
                s.Content?.PublishedOn,
                s.Content?.TonePools
            }).
            GroupBy(t => new {t.Name, t.PublishedOn}).Select(g => {
              if (g.Key.PublishedOn != null)
              {
                return new GroupResult(
                  g.Key.Name == null ? "" : g.Key.Name,
                  g.Key.PublishedOn.Value.ToString("yyyy-MM"),
                  g.Count(),
                  g.Sum(x => (x != null && x.TonePools != null) ? x.TonePools.Sum(y => y.Value) : 0)
                );
              } else {
                return new GroupResult("", "", 0, 0);
              }
            });

            var groupedByProductDate = productTable.GroupBy(t => new { t.Date, t.Name }).Select(g => new GroupResult (
                g.Key.Name,
                g.Key.Date,
                g.Sum(c => c == null ? 0 : c.Count),
                g.Sum(c => c == null ? 0 : c.Tone)
            )).ToList();

            var productHeaders = productTable.GroupBy(t => t.Name).Select(g => g.Key).ToList();
            var productDateRows = productTable.GroupBy(t => t.Date).Select(g => g.Key).ToList();
            int index = 0;

            AddTitle(index, serialized?.Report?.Name);
            index+=2;

            index = BuildTable(index, productHeaders, productDateRows, groupedByProductDate, tonesCount);
            
            index+=2;

            var sectionTable = content.
            Select(s => new {
                s.Content?.Section,
                s.Content?.PublishedOn,
                s.Content?.TonePools
            }).
            GroupBy(t => new {t.Section, t.PublishedOn}).Select(g => {
              if (g.Key.PublishedOn != null)
              {
                return new GroupResult(
                  g.Key.Section == null ? "No Section" : g.Key.Section,
                  g.Key.PublishedOn.Value.ToString("yyyy-MM"),
                  g.Count(),
                  g.Sum(x => (x != null && x.TonePools != null) ? x.TonePools.Sum(y => y.Value) : 0)
                );
              } else {
                return new GroupResult("", "", 0, 0);
              }
            });

            var groupedBySectiontDate = sectionTable.GroupBy(t => new {t.Date, t.Name}).Select(g => new GroupResult (
                g.Key.Name,
                g.Key.Date,
                g.Sum(c => c == null ? 0 : c.Count),
                g.Sum(c => c == null ? 0 : c.Tone)
            )).ToList();

            var sectionHeaders = sectionTable.GroupBy(t => t.Name).Select(g => g.Key).ToList();
            var sectionDateRows = sectionTable.GroupBy(t => t.Date).Select(g => g.Key).ToList();

            totalColumns = sectionHeaders.Count() >= productHeaders.Count() ? sectionHeaders.Count() : productHeaders.Count();

            index = BuildTable(index, sectionHeaders, sectionDateRows, groupedBySectiontDate, tonesCount);

            var headlines = content.Select(h => {
              if (h.Content != null && h.Content.PublishedOn != null)
              {
                return new HeadlineResult(
                  h.Content.TonePools.Average(p => p.Value),
                  h.Content.PublishedOn.Value.ToString("yyyy-MM-dd"),
                  h.Content.Headline,
                  h.Content.ContentType.ToString(),
                  h.Content.Source?.Name,
                  h.Content.Series?.Name
                );
              } else {
                return new HeadlineResult(0, "", "", "", "", "");
              }
            }).ToList();

            index+=2;

            BuildHeadLines(index, headlines);

            //Set Columns width
            for(int i = 0; i <= (totalColumns * 2 + 10); i++)
            {
                sheet.SetColumnWidth(i, 13 * 256 + 200);
            }
            
            return workbook;
        }

        private void BuildHeadLines(int rowIndex, List<HeadlineResult> headlines)
        {
            if (headlines == null)
            {
              return;
            }

            string[] headlineHeaders = {"Tone", "Date", "Headline", "Type", "Source", "Show/Program"};
            IRow rowHeader = sheet.CreateRow(rowIndex);
            int cellIndex = 0;
            foreach(string h in headlineHeaders)
            {
                ICell cellA = rowHeader.CreateCell(cellIndex);
                cellA.CellStyle = boldStyle;
                cellA.SetCellValue(h);
                cellIndex++;
            }
            rowIndex++;
            foreach (HeadlineResult r in headlines)
            {
                cellIndex = 0;
                IRow row1 = sheet.CreateRow(rowIndex);
            AddNumberCell(row1, cellIndex, regularStyle, r.Tone);
                cellIndex++;
            AddTextCell(row1, cellIndex, regularStyle, r.Date);
                cellIndex++;
            AddTextCell(row1, cellIndex, regularStyle, r.Headline);
                cellIndex++;
            AddTextCell(row1, cellIndex, regularStyle, r.Type);
                cellIndex++;
            AddTextCell(row1, cellIndex, regularStyle, r.Source);
                cellIndex++;
            AddTextCell(row1, cellIndex, regularStyle, r.Series);
                rowIndex++;
            }
        }

        private IFont CreateFont(double size, bool bold, XSSFColor color)
        {
            IFont font = workbook.CreateFont();
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
            ICellStyle style = workbook.CreateCellStyle();
            style.SetFont(font);
            style.WrapText = wrap;
            style.Alignment = horizontalAlignment;
            return style;
        }

        private void AddTitle(int index, string? text)
        {
            ICellStyle titleStyle = CreateStyle(14, true, false, HorizontalAlignment.Center);

            IRow titleRow = sheet.CreateRow(index);
            _ = sheet.AddMergedRegion(CellRangeAddress.ValueOf("A1:E1"));
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
            IRow row1 = sheet.CreateRow(rowIndex);

        AddTextCell(row1, cellIndex, regularStyle, "Date");
            cellIndex++;
            foreach(string h in headers)
            {
            AddTextCell(row1, cellIndex, boldStyle, h);
                cellIndex++;
            }
        AddTextCell(row1, cellIndex, boldStyle, "Total");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Average");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Mean");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Date");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Combined");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Date");
            cellIndex++;
            foreach(string h in headers)
            {
            AddTextCell(row1, cellIndex, boldStyle, h);
                cellIndex++;
            }
        AddTextCell(row1, cellIndex, boldStyle, "Total");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Average");
            cellIndex++;
        AddTextCell(row1, cellIndex, boldStyle, "Mean");
        }

        private int BuildTable(int rowIndex, List<string> headers, List<string> dates, List<GroupResult> groups, TonesCount tonesCount)
        {
            AddHeader(rowIndex, 0, headers);
            rowIndex++;
            var firstDataRow = rowIndex;
            int combinedCellIndex = 0;
            CellAddress firstCellCount = new(0,0);
            CellAddress lastCellCount = new(0,0);
            CellAddress firstCellTone = new(0,0);
            CellAddress lastCellTone = new(0,0);
            foreach(var d in dates)
            {
                int index = 0;
                IRow row1 = sheet.CreateRow(rowIndex);
            AddTextCell(row1, index, regularStyle, d);
                bool isFirstCell = true;
                foreach(var h in headers)
                {
                    int count = 0;
                    foreach(var g in groups)
                    {
                        if (g.Name == h && g.Date == d)
                        {
                            count += g.Count;
                        }
                    }
                    index++;
                    ICell cellA = AddNumberCell(row1, index, regularStyle, count);
                    if (isFirstCell)
                    {
                        firstCellCount = cellA.Address;
                        isFirstCell = false;
                    }
                    lastCellCount = cellA.Address;
                }
                // Set the cells to apply formulas
                CellAddress formulaTopAdress = new(lastCellCount.Row, firstCellCount.Column);
                CellAddress formulaBottomAdress = new(lastCellCount.Row, lastCellCount.Column);
                // Add the Sum column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("SUM({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Add the Average column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("AVERAGE({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Add the Median column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("MEDIAN({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Repeat Date Column
                index++;
            AddTextCell(row1, index, regularStyle, d);
                // Repeat the Sum for the combined Column
                index++;
                combinedCellIndex = index;
            AddFormulaCell(row1, index, regularStyle, string.Format("SUM({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Repeat Date Column Again
                index++;
            AddTextCell(row1, index, regularStyle, d);
                rowIndex++;
                // Roll again but calculating the tones
                isFirstCell = true;
                foreach(var h in headers)
                {
                    double tone = 0;
                    foreach(var g in groups)
                    {
                        if (g.Name == h && g.Date == d)
                        {
                            tone += g.Tone;
                        }
                    }
                    index++;
                    ICell cellA = AddNumberCell(row1, index, regularStyle, tone);
                    if (isFirstCell)
                    {
                        firstCellTone = cellA.Address;
                        isFirstCell = false;
                    }
                    lastCellTone = cellA.Address;
                }
                // Set the cells to apply formulas
                formulaTopAdress = new (lastCellTone.Row, firstCellTone.Column);
                formulaBottomAdress = new (lastCellTone.Row, lastCellTone.Column);
                // Add the Sum column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("SUM({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Add the Average column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("AVERAGE({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                // Add the Median column
                index++;
            AddFormulaCell(row1, index, regularStyle, string.Format("MEDIAN({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
            }
            int cellIndex = 0;
            IRow rowTotal = sheet.CreateRow(rowIndex);
            rowIndex++;
            IRow rowAverage = sheet.CreateRow(rowIndex);
            rowIndex++;
            IRow rowMedian = sheet.CreateRow(rowIndex);
            rowIndex++;
        AddTextCell(rowTotal, cellIndex, boldStyle, "Total");
        AddTextCell(rowAverage, cellIndex, boldStyle, "Average");
        AddTextCell(rowMedian, cellIndex, boldStyle, "Mean");
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellCount.Column + (cellIndex - 1));
                CellAddress formulaBottomAdress = new (lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowTotal, cellIndex, boldStyle, string.Format("SUM({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }
            cellIndex = 0;
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellCount.Column + (cellIndex - 1));
                CellAddress formulaBottomAdress = new (lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowAverage, cellIndex, boldStyle, string.Format("AVERAGE({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }
            cellIndex = 0;
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellCount.Column + (cellIndex - 1));
                CellAddress formulaBottomAdress = new (lastCellCount.Row, firstCellCount.Column + (cellIndex - 1));
            AddFormulaCell(rowMedian, cellIndex, boldStyle, string.Format("MEDIAN({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }

            cellIndex+= 5;
            int cellStartIndex = cellIndex;
        AddTextCell(rowTotal, cellIndex, boldStyle, "Total");
        AddTextCell(rowAverage, cellIndex, boldStyle, "Average");
        AddTextCell(rowMedian, cellIndex, boldStyle, "Mean");

            // Add Row Tone total
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
                CellAddress formulaBottomAdress = new (lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowTotal, cellIndex, regularStyle, string.Format("SUM({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }

            // Add Row Tone average
            cellIndex = cellStartIndex;
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
                CellAddress formulaBottomAdress = new (lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowAverage, cellIndex, regularStyle, string.Format("AVERAGE({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }

            // Add Row Tone median
            cellIndex = cellStartIndex;
            cellIndex++;
            foreach(var h in headers)
            {
                CellAddress formulaTopAdress = new (firstDataRow, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
                CellAddress formulaBottomAdress = new (lastCellTone.Row, firstCellTone.Column + (cellIndex - (headers.Count + 7)));
            AddFormulaCell(rowMedian, cellIndex, regularStyle, string.Format("MEDIAN({0}:{1})",formulaTopAdress.ToString(), formulaBottomAdress.ToString()));
                cellIndex++;
            }

            IRow toneCountLabels = sheet.CreateRow(rowIndex);
        AddTextCell(toneCountLabels, combinedCellIndex, boldStyle, "Positive");
        AddTextCell(toneCountLabels, combinedCellIndex+1, boldStyle, "Neutral");
        AddTextCell(toneCountLabels, combinedCellIndex+2, boldStyle, "Negative");
            rowIndex++;
            IRow toneCount = sheet.CreateRow(rowIndex);
        AddNumberCell(toneCount, combinedCellIndex, regularStyle, tonesCount.Positive);
        AddNumberCell(toneCount, combinedCellIndex+1, regularStyle, tonesCount.Neutral);
        AddNumberCell(toneCount, combinedCellIndex+2, regularStyle, tonesCount.Negative);

            return rowIndex;
        }
        
        #endregion
}