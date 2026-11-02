namespace TNO.Core.Excel;

/// <summary>
/// ExcelHorizontalAlignment enum, the horizontal alignment of a cell's content.
/// </summary>
public enum ExcelHorizontalAlignment
{
    /// <summary>
    /// Text left, numbers right (the spreadsheet default).
    /// </summary>
    General = 0,

    /// <summary>
    /// Align to the left edge of the cell.
    /// </summary>
    Left = 1,

    /// <summary>
    /// Centre within the cell.
    /// </summary>
    Center = 2,

    /// <summary>
    /// Align to the right edge of the cell.
    /// </summary>
    Right = 3,
}
