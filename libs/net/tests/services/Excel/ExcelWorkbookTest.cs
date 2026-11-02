using AwesomeAssertions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Validation;
using TNO.Core.Excel;

namespace TNO.Test.Services.Excel;

public class ExcelWorkbookTest
{
    private static SpreadsheetDocument Reopen(ExcelWorkbook workbook, MemoryStream stream)
    {
        workbook.Write(stream);
        stream.Position = 0;
        return SpreadsheetDocument.Open(stream, false);
    }

    private static Cell GetCell(SpreadsheetDocument document, string reference)
    {
        var part = document.WorkbookPart!.WorksheetParts.First();
        return part.Worksheet!.Descendants<Cell>().Single(c => c.CellReference == reference);
    }

    private static string GetString(SpreadsheetDocument document, Cell cell)
    {
        var table = document.WorkbookPart!.SharedStringTablePart!.SharedStringTable!;
        return table.Elements<SharedStringItem>().ElementAt(int.Parse(cell.CellValue!.Text)).InnerText;
    }

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(27, "AB")]
    [InlineData(701, "ZZ")]
    [InlineData(702, "AAA")]
    public void GetColumnName_ConvertsZeroBasedIndex(int index, string expected)
    {
        ExcelWorkbook.GetColumnName(index).Should().Be(expected);
    }

    [Fact]
    public void GetCellReference_IsOneBasedRow()
    {
        ExcelWorkbook.GetCellReference(3, 1).Should().Be("B4");
    }

    [Fact]
    public void Write_ProducesValidSpreadsheetWithValuesFormulasAndLayout()
    {
        var workbook = new ExcelWorkbook();
        var sheet = workbook.CreateSheet("Report");
        sheet.SetColumnWidth(0, 31.25);
        sheet.AddMergedRegion("A1:E1");
        var bold = new ExcelCellStyle() { FontName = "Verdana", FontSize = 14, Bold = true, HorizontalAlignment = ExcelHorizontalAlignment.Center };
        var percent = new ExcelCellStyle() { NumberFormat = "0.00%", HorizontalAlignment = ExcelHorizontalAlignment.Right };
        var custom = new ExcelCellStyle() { NumberFormat = "yyyy-mm-dd" };
        sheet.CreateRow(0).CreateCell(0, bold).SetValue("Title");
        var row = sheet.CreateRow(1);
        row.CreateCell(0, percent).SetValue(0.125);
        row.CreateCell(1).SetValue(2);
        row.CreateCell(2).SetFormula("SUM(A2:B2)");
        row.CreateCell(3, bold).SetValue((string?)null);
        row.CreateCell(4).SetValue("  keep spaces\nand lines  ");
        var spacer = sheet.CreateRow(3);
        spacer.Height = 2.5;
        spacer.Style = new ExcelCellStyle() { FillColor = "C0C0C0" };

        using var stream = new MemoryStream();
        using var document = Reopen(workbook, stream);

        new OpenXmlValidator().Validate(document, TestContext.Current.CancellationToken).Should().BeEmpty();
        document.WorkbookPart!.Workbook!.Descendants<Sheet>().Single().Name!.Value.Should().Be("Report");
        GetString(document, GetCell(document, "A1")).Should().Be("Title");
        GetCell(document, "A2").CellValue!.Text.Should().Be("0.125");
        GetCell(document, "C2").CellFormula!.Text.Should().Be("SUM(A2:B2)");
        GetCell(document, "C2").CellValue.Should().BeNull();
        GetCell(document, "D2").CellValue.Should().BeNull();
        GetCell(document, "D2").StyleIndex!.Value.Should().Be(GetCell(document, "A1").StyleIndex!.Value);
        GetString(document, GetCell(document, "E2")).Should().Be("  keep spaces\nand lines  ");
        document.WorkbookPart.Workbook!.CalculationProperties!.FullCalculationOnLoad!.Value.Should().BeTrue();

        var worksheet = document.WorkbookPart.WorksheetParts.First().Worksheet!;
        worksheet.Descendants<MergeCell>().Single().Reference!.Value.Should().Be("A1:E1");
        var column = worksheet.Descendants<Column>().Single();
        column.Width!.Value.Should().Be(31.25);
        var spacerRow = worksheet.Descendants<Row>().Single(r => r.RowIndex! == 4U);
        spacerRow.Height!.Value.Should().Be(2.5);
        spacerRow.CustomFormat!.Value.Should().BeTrue();

        var styles = document.WorkbookPart.WorkbookStylesPart!.Stylesheet!;
        var formats = styles.CellFormats!.Elements<CellFormat>().ToList();
        var titleFormat = formats[(int)GetCell(document, "A1").StyleIndex!.Value];
        var titleFont = styles.Fonts!.Elements<Font>().ElementAt((int)titleFormat.FontId!.Value);
        titleFont.FontName!.Val!.Value.Should().Be("Verdana");
        titleFont.FontSize!.Val!.Value.Should().Be(14);
        titleFont.Bold.Should().NotBeNull();
        titleFormat.Alignment!.Horizontal!.Value.Should().Be(HorizontalAlignmentValues.Center);
        formats[(int)GetCell(document, "A2").StyleIndex!.Value].NumberFormatId!.Value.Should().Be(10U);
        styles.NumberingFormats.Should().BeNull("only built-in number formats are used before the custom one is applied");
    }

    [Fact]
    public void Write_RegistersCustomNumberFormatsAndSolidFills()
    {
        var workbook = new ExcelWorkbook();
        var row = workbook.CreateSheet("S").CreateRow(0);
        row.CreateCell(0, new ExcelCellStyle() { NumberFormat = "yyyy-mm-dd", FillColor = "#c0c0c0" }).SetValue(46000);

        using var stream = new MemoryStream();
        using var document = Reopen(workbook, stream);

        var styles = document.WorkbookPart!.WorkbookStylesPart!.Stylesheet!;
        var format = styles.CellFormats!.Elements<CellFormat>().ElementAt((int)GetCell(document, "A1").StyleIndex!.Value);
        format.NumberFormatId!.Value.Should().Be(164U);
        styles.NumberingFormats!.Elements<NumberingFormat>().Single().FormatCode!.Value.Should().Be("yyyy-mm-dd");
        var fill = styles.Fills!.Elements<Fill>().ElementAt((int)format.FillId!.Value).PatternFill!;
        fill.PatternType!.Value.Should().Be(PatternValues.Solid);
        fill.ForegroundColor!.Rgb!.Value.Should().Be("FFC0C0C0");
    }

    [Fact]
    public void Write_SharesEqualStylesAndStrings()
    {
        var workbook = new ExcelWorkbook();
        var row = workbook.CreateSheet("S").CreateRow(0);
        row.CreateCell(0, new ExcelCellStyle() { Bold = true }).SetValue("same");
        row.CreateCell(1, new ExcelCellStyle() { Bold = true }).SetValue("same");

        using var stream = new MemoryStream();
        using var document = Reopen(workbook, stream);

        GetCell(document, "A1").StyleIndex!.Value.Should().Be(GetCell(document, "B1").StyleIndex!.Value);
        document.WorkbookPart!.SharedStringTablePart!.SharedStringTable!.Elements<SharedStringItem>().Should().HaveCount(1);
    }

    [Fact]
    public void CreateRowAndCreateCell_ReplaceExistingEntries()
    {
        var sheet = new ExcelWorkbook().CreateSheet("S");
        sheet.CreateRow(0).CreateCell(0).SetValue("old");
        var row = sheet.CreateRow(0);
        row.CreateCell(1).SetValue("first");
        row.CreateCell(1).SetValue(5);

        sheet.Rows.Should().ContainSingle();
        row.Cells.Should().ContainSingle().Which.Number.Should().Be(5);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a/b")]
    [InlineData("[x]")]
    [InlineData("this sheet name is longer than 31")]
    public void CreateSheet_RejectsInvalidNames(string name)
    {
        var workbook = new ExcelWorkbook();
        workbook.Invoking(w => w.CreateSheet(name)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateSheet_RejectsDuplicateNames()
    {
        var workbook = new ExcelWorkbook();
        workbook.CreateSheet("Report");
        workbook.Invoking(w => w.CreateSheet("report")).Should().Throw<ArgumentException>();
    }
}
