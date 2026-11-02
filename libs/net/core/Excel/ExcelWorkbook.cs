using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TNO.Core.Excel;

/// <summary>
/// ExcelWorkbook class, builds an .xlsx workbook in memory and writes it with the Open XML SDK.
/// It supports the subset of spreadsheet features the reports need: string, number and formula cells,
/// fonts, fills, alignment, wrapping, number formats, merged regions, column widths and row heights.
/// </summary>
public sealed class ExcelWorkbook
{
    #region Variables
    /// <summary>
    /// Number formats every spreadsheet application knows by id; anything else is written as a custom format.
    /// </summary>
    private static readonly Dictionary<string, uint> BuiltInNumberFormats = new()
    {
        ["General"] = 0,
        ["0"] = 1,
        ["0.00"] = 2,
        ["#,##0"] = 3,
        ["#,##0.00"] = 4,
        ["0%"] = 9,
        ["0.00%"] = 10,
    };

    /// <summary>
    /// The first id available for custom number formats.
    /// </summary>
    private const uint FirstCustomNumberFormatId = 164;

    private static readonly char[] InvalidSheetNameCharacters = ['/', '\\', '?', '*', ':', '[', ']'];

    private readonly List<ExcelSheet> _sheets = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The sheets in this workbook, in tab order.
    /// </summary>
    public IReadOnlyList<ExcelSheet> Sheets => _sheets;
    #endregion

    #region Methods
    /// <summary>
    /// Add a new sheet to the workbook.
    /// </summary>
    /// <param name="name">The sheet name (1-31 characters, without / \ ? * : [ ]).</param>
    /// <returns></returns>
    public ExcelSheet CreateSheet(string name)
    {
        if (String.IsNullOrWhiteSpace(name) || name.Length > 31 || name.IndexOfAny(InvalidSheetNameCharacters) >= 0)
            throw new ArgumentException($"Invalid sheet name '{name}'.", nameof(name));
        if (_sheets.Any(s => String.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"The workbook already contains a sheet named '{name}'.", nameof(name));

        var sheet = new ExcelSheet(name);
        _sheets.Add(sheet);
        return sheet;
    }

    /// <summary>
    /// Write the workbook to the stream as an .xlsx file.
    /// </summary>
    /// <param name="stream"></param>
    public void Write(Stream stream)
    {
        if (_sheets.Count == 0) throw new InvalidOperationException("A workbook requires at least one sheet.");

        var styles = new StyleSheetBuilder();
        var sharedStrings = new SharedStringBuilder();

        using var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook);
        var workbookPart = document.AddWorkbookPart();
        var sheets = new Sheets();

        uint sheetId = 1;
        foreach (var sheet in _sheets)
        {
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = BuildWorksheet(sheet, sheetId == 1, styles, sharedStrings);
            sheets.Append(new Sheet()
            {
                Id = workbookPart.GetIdOfPart(worksheetPart),
                SheetId = sheetId++,
                Name = sheet.Name,
            });
        }

        // Formula cells are written without cached values, so ask the application to calculate on open.
        workbookPart.Workbook = new Workbook(sheets, new CalculationProperties() { FullCalculationOnLoad = true });

        var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
        stylesPart.Stylesheet = styles.Build();

        var sharedStringPart = workbookPart.AddNewPart<SharedStringTablePart>();
        sharedStringPart.SharedStringTable = sharedStrings.Build();
    }

    /// <summary>
    /// Convert a zero-based row and column into an A1 cell reference (e.g. (3, 1) => "B4").
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public static string GetCellReference(int rowIndex, int columnIndex)
    {
        return $"{GetColumnName(columnIndex)}{rowIndex + 1}";
    }

    /// <summary>
    /// Convert a zero-based column index into its column name (e.g. 0 => "A", 27 => "AB").
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <returns></returns>
    public static string GetColumnName(int columnIndex)
    {
        if (columnIndex < 0) throw new ArgumentOutOfRangeException(nameof(columnIndex));
        var name = "";
        var dividend = columnIndex + 1;
        while (dividend > 0)
        {
            var modulo = (dividend - 1) % 26;
            name = (char)('A' + modulo) + name;
            dividend = (dividend - modulo) / 26;
        }
        return name;
    }

    private static Worksheet BuildWorksheet(ExcelSheet sheet, bool isFirst, StyleSheetBuilder styles, SharedStringBuilder sharedStrings)
    {
        // Child elements must follow the order the SpreadsheetML schema defines.
        var worksheet = new Worksheet(
            new SheetViews(new SheetView() { TabSelected = isFirst, WorkbookViewId = 0U }),
            new SheetFormatProperties() { DefaultRowHeight = 15D });

        if (sheet.ColumnWidths.Count > 0)
        {
            var columns = new Columns();
            foreach (var (columnIndex, width) in sheet.ColumnWidths)
            {
                var number = (uint)columnIndex + 1;
                columns.Append(new Column() { Min = number, Max = number, Width = width, CustomWidth = true });
            }
            worksheet.Append(columns);
        }

        var sheetData = new SheetData();
        foreach (var row in sheet.Rows)
        {
            var element = new Row() { RowIndex = (uint)row.Index + 1 };
            if (row.Height.HasValue)
            {
                element.Height = row.Height.Value;
                element.CustomHeight = true;
            }
            if (row.Style != null)
            {
                element.StyleIndex = styles.GetIndex(row.Style);
                element.CustomFormat = true;
            }

            foreach (var cell in row.Cells)
            {
                var c = new Cell() { CellReference = cell.Address };
                if (cell.Style != null) c.StyleIndex = styles.GetIndex(cell.Style);

                if (cell.Formula != null)
                {
                    c.CellFormula = new CellFormula(cell.Formula);
                }
                else if (cell.Number.HasValue)
                {
                    c.DataType = CellValues.Number;
                    c.CellValue = new CellValue(cell.Number.Value);
                }
                else if (cell.Text != null)
                {
                    c.DataType = CellValues.SharedString;
                    c.CellValue = new CellValue(sharedStrings.GetIndex(cell.Text));
                }
                element.Append(c);
            }
            sheetData.Append(element);
        }
        worksheet.Append(sheetData);

        if (sheet.MergedRegions.Count > 0)
        {
            var mergeCells = new MergeCells();
            foreach (var range in sheet.MergedRegions)
                mergeCells.Append(new MergeCell() { Reference = range });
            worksheet.Append(mergeCells);
        }

        return worksheet;
    }
    #endregion

    #region Builders
    /// <summary>
    /// Collects the distinct strings in the workbook into the shared string table.
    /// </summary>
    private sealed class SharedStringBuilder
    {
        private readonly Dictionary<string, int> _indexes = new(StringComparer.Ordinal);
        private readonly List<string> _values = new();

        public int GetIndex(string value)
        {
            if (!_indexes.TryGetValue(value, out var index))
            {
                index = _values.Count;
                _values.Add(value);
                _indexes.Add(value, index);
            }
            return index;
        }

        public SharedStringTable Build()
        {
            var table = new SharedStringTable() { Count = (uint)_values.Count, UniqueCount = (uint)_values.Count };
            foreach (var value in _values)
            {
                // Preserve leading/trailing whitespace and line breaks exactly as given.
                table.Append(new SharedStringItem(new Text(value) { Space = SpaceProcessingModeValues.Preserve }));
            }
            return table;
        }
    }

    /// <summary>
    /// Collects the distinct fonts, fills, number formats and cell formats used in the workbook.
    /// </summary>
    private sealed class StyleSheetBuilder
    {
        private readonly Dictionary<ExcelCellStyle, uint> _cellFormats = new();
        private readonly List<ExcelCellStyle> _cellFormatList = new();
        private readonly Dictionary<(string Name, double Size, bool Bold, string Color), uint> _fonts = new();
        private readonly Dictionary<string, uint> _fills = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, uint> _numberFormats = new(StringComparer.Ordinal);

        public StyleSheetBuilder()
        {
            // Index 0 of fonts and cell formats is the workbook default.
            _fonts.Add(("Calibri", 11D, false, "000000"), 0);
        }

        public uint GetIndex(ExcelCellStyle style)
        {
            if (!_cellFormats.TryGetValue(style, out var index))
            {
                // +1 because cell format 0 is the default format.
                index = (uint)_cellFormatList.Count + 1;
                _cellFormatList.Add(style);
                _cellFormats.Add(style, index);
            }
            return index;
        }

        public Stylesheet Build()
        {
            var cellFormats = new CellFormats(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U, FormatId = 0U });
            foreach (var style in _cellFormatList)
            {
                var numberFormatId = GetNumberFormatId(style.NumberFormat);
                var fillId = GetFillId(style.FillColor);
                var format = new CellFormat()
                {
                    NumberFormatId = numberFormatId,
                    FontId = GetFontId(style),
                    FillId = fillId,
                    BorderId = 0U,
                    FormatId = 0U,
                    ApplyFont = true,
                    ApplyFill = fillId != 0 ? true : null,
                    ApplyNumberFormat = numberFormatId != 0 ? true : null,
                    ApplyAlignment = true,
                };
                format.Append(new Alignment()
                {
                    Horizontal = style.HorizontalAlignment switch
                    {
                        ExcelHorizontalAlignment.Left => HorizontalAlignmentValues.Left,
                        ExcelHorizontalAlignment.Center => HorizontalAlignmentValues.Center,
                        ExcelHorizontalAlignment.Right => HorizontalAlignmentValues.Right,
                        _ => HorizontalAlignmentValues.General,
                    },
                    Vertical = style.VerticalAlignment switch
                    {
                        ExcelVerticalAlignment.Top => VerticalAlignmentValues.Top,
                        ExcelVerticalAlignment.Center => VerticalAlignmentValues.Center,
                        _ => VerticalAlignmentValues.Bottom,
                    },
                    WrapText = style.WrapText ? true : null,
                });
                cellFormats.Append(format);
            }
            cellFormats.Count = (uint)cellFormats.ChildElements.Count;

            var stylesheet = new Stylesheet();
            if (_numberFormats.Count > 0)
            {
                var numberingFormats = new NumberingFormats() { Count = (uint)_numberFormats.Count };
                foreach (var (code, id) in _numberFormats)
                    numberingFormats.Append(new NumberingFormat() { NumberFormatId = id, FormatCode = code });
                stylesheet.Append(numberingFormats);
            }

            var fonts = new Fonts() { Count = (uint)_fonts.Count };
            foreach (var font in _fonts.OrderBy(f => f.Value).Select(f => f.Key))
            {
                var element = new Font();
                if (font.Bold) element.Append(new Bold());
                element.Append(new FontSize() { Val = font.Size });
                element.Append(new Color() { Rgb = $"FF{font.Color}" });
                element.Append(new FontName() { Val = font.Name });
                element.Append(new FontFamilyNumbering() { Val = 2 });
                fonts.Append(element);
            }
            stylesheet.Append(fonts);

            // Fill 0 (none) and fill 1 (gray125) are reserved by the specification.
            var fills = new Fills(
                new Fill(new PatternFill() { PatternType = PatternValues.None }),
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }));
            foreach (var color in _fills.OrderBy(f => f.Value).Select(f => f.Key))
            {
                fills.Append(new Fill(new PatternFill(
                    new ForegroundColor() { Rgb = $"FF{color}" },
                    new BackgroundColor() { Indexed = 64U })
                { PatternType = PatternValues.Solid }));
            }
            fills.Count = (uint)fills.ChildElements.Count;
            stylesheet.Append(fills);

            stylesheet.Append(new Borders(new Border(new LeftBorder(), new RightBorder(), new TopBorder(), new BottomBorder(), new DiagonalBorder())) { Count = 1U });
            stylesheet.Append(new CellStyleFormats(new CellFormat() { NumberFormatId = 0U, FontId = 0U, FillId = 0U, BorderId = 0U }) { Count = 1U });
            stylesheet.Append(cellFormats);
            stylesheet.Append(new CellStyles(new CellStyle() { Name = "Normal", FormatId = 0U, BuiltinId = 0U }) { Count = 1U });
            return stylesheet;
        }

        private uint GetFontId(ExcelCellStyle style)
        {
            var key = (style.FontName, style.FontSize, style.Bold, NormalizeColor(style.FontColor));
            if (!_fonts.TryGetValue(key, out var id))
            {
                id = (uint)_fonts.Count;
                _fonts.Add(key, id);
            }
            return id;
        }

        private uint GetFillId(string? color)
        {
            if (String.IsNullOrWhiteSpace(color)) return 0;
            color = NormalizeColor(color);
            if (!_fills.TryGetValue(color, out var id))
            {
                // +2 because fills 0 and 1 are reserved.
                id = (uint)_fills.Count + 2;
                _fills.Add(color, id);
            }
            return id;
        }

        private uint GetNumberFormatId(string? code)
        {
            if (String.IsNullOrEmpty(code)) return 0;
            if (BuiltInNumberFormats.TryGetValue(code, out var builtIn)) return builtIn;
            if (!_numberFormats.TryGetValue(code, out var id))
            {
                id = FirstCustomNumberFormatId + (uint)_numberFormats.Count;
                _numberFormats.Add(code, id);
            }
            return id;
        }

        private static string NormalizeColor(string color)
        {
            var hex = color.Trim().TrimStart('#').ToUpperInvariant();
            if (hex.Length != 6 || !hex.All(Uri.IsHexDigit))
                throw new ArgumentException($"Colour '{color}' must be a six digit RGB hex value.", nameof(color));
            return hex;
        }
    }
    #endregion
}
