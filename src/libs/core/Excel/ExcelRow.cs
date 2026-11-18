namespace TNO.Core.Excel;

/// <summary>
/// ExcelRow class, a row in an <see cref="ExcelSheet"/>.
/// </summary>
public sealed class ExcelRow
{
    #region Variables
    private readonly SortedDictionary<int, ExcelCell> _cells = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The zero-based row index.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// get/set - A custom row height in points, or null for the default height.
    /// </summary>
    public double? Height { get; set; }

    /// <summary>
    /// get/set - A style applied to the whole row, or null for none.
    /// </summary>
    public ExcelCellStyle? Style { get; set; }

    /// <summary>
    /// get - The cells in this row, ordered by column.
    /// </summary>
    public IEnumerable<ExcelCell> Cells => _cells.Values;
    #endregion

    #region Constructors
    internal ExcelRow(int index)
    {
        this.Index = index;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a cell at the specified zero-based column, replacing any cell already there.
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <param name="style"></param>
    /// <returns></returns>
    public ExcelCell CreateCell(int columnIndex, ExcelCellStyle? style = null)
    {
        if (columnIndex < 0) throw new ArgumentOutOfRangeException(nameof(columnIndex));
        var cell = new ExcelCell(this.Index, columnIndex) { Style = style };
        _cells[columnIndex] = cell;
        return cell;
    }
    #endregion
}
