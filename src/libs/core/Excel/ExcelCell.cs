namespace TNO.Core.Excel;

/// <summary>
/// ExcelCell class, a single cell in an <see cref="ExcelRow"/>.
/// </summary>
public sealed class ExcelCell
{
    #region Properties
    /// <summary>
    /// get - The zero-based column index.
    /// </summary>
    public int ColumnIndex { get; }

    /// <summary>
    /// get - The zero-based row index.
    /// </summary>
    public int RowIndex { get; }

    /// <summary>
    /// get - The cell reference in A1 notation (e.g. "B4").
    /// </summary>
    public string Address => ExcelWorkbook.GetCellReference(this.RowIndex, this.ColumnIndex);

    /// <summary>
    /// get/set - The cell style, or null for the workbook default.
    /// </summary>
    public ExcelCellStyle? Style { get; set; }

    /// <summary>
    /// get - The text value when the cell holds a string.
    /// </summary>
    public string? Text { get; private set; }

    /// <summary>
    /// get - The numeric value when the cell holds a number.
    /// </summary>
    public double? Number { get; private set; }

    /// <summary>
    /// get - The formula (without a leading '=') when the cell holds a formula.
    /// </summary>
    public string? Formula { get; private set; }
    #endregion

    #region Constructors
    internal ExcelCell(int rowIndex, int columnIndex)
    {
        this.RowIndex = rowIndex;
        this.ColumnIndex = columnIndex;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Set the cell to a string value. A null value leaves the cell blank (its style is kept).
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ExcelCell SetValue(string? value)
    {
        this.Text = value;
        this.Number = null;
        this.Formula = null;
        return this;
    }

    /// <summary>
    /// Set the cell to a numeric value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public ExcelCell SetValue(double value)
    {
        this.Text = null;
        this.Number = value;
        this.Formula = null;
        return this;
    }

    /// <summary>
    /// Set the cell to a formula. The spreadsheet application calculates it when the file is opened.
    /// </summary>
    /// <param name="formula">The formula without a leading '=' (e.g. "SUM(B4:D4)").</param>
    /// <returns></returns>
    public ExcelCell SetFormula(string formula)
    {
        this.Text = null;
        this.Number = null;
        this.Formula = formula;
        return this;
    }
    #endregion
}
