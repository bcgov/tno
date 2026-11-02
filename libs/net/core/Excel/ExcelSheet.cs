namespace TNO.Core.Excel;

/// <summary>
/// ExcelSheet class, a worksheet in an <see cref="ExcelWorkbook"/>.
/// </summary>
public sealed class ExcelSheet
{
    #region Variables
    private readonly SortedDictionary<int, ExcelRow> _rows = new();
    private readonly SortedDictionary<int, double> _columnWidths = new();
    private readonly List<string> _mergedRegions = new();
    #endregion

    #region Properties
    /// <summary>
    /// get - The sheet name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// get - The rows in this sheet, ordered by index.
    /// </summary>
    public IEnumerable<ExcelRow> Rows => _rows.Values;

    /// <summary>
    /// get - Custom column widths in characters, keyed by zero-based column index.
    /// </summary>
    public IReadOnlyDictionary<int, double> ColumnWidths => _columnWidths;

    /// <summary>
    /// get - The merged cell ranges in A1 notation (e.g. "A1:E1").
    /// </summary>
    public IReadOnlyList<string> MergedRegions => _mergedRegions;
    #endregion

    #region Constructors
    internal ExcelSheet(string name)
    {
        this.Name = name;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Create a row at the specified zero-based index, replacing any row already there.
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <returns></returns>
    public ExcelRow CreateRow(int rowIndex)
    {
        if (rowIndex < 0) throw new ArgumentOutOfRangeException(nameof(rowIndex));
        var row = new ExcelRow(rowIndex);
        _rows[rowIndex] = row;
        return row;
    }

    /// <summary>
    /// Set the width of a zero-based column, in characters of the default font.
    /// </summary>
    /// <param name="columnIndex"></param>
    /// <param name="width"></param>
    public void SetColumnWidth(int columnIndex, double width)
    {
        if (columnIndex < 0) throw new ArgumentOutOfRangeException(nameof(columnIndex));
        if (width < 0) throw new ArgumentOutOfRangeException(nameof(width));
        _columnWidths[columnIndex] = width;
    }

    /// <summary>
    /// Merge a range of cells, given in A1 notation (e.g. "A1:E1").
    /// </summary>
    /// <param name="range"></param>
    public void AddMergedRegion(string range)
    {
        if (String.IsNullOrWhiteSpace(range)) throw new ArgumentException("A cell range is required.", nameof(range));
        _mergedRegions.Add(range);
    }
    #endregion
}
