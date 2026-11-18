namespace TNO.Core.Excel;

/// <summary>
/// ExcelCellStyle record, describes how a cell or row is formatted.
/// Styles are value objects: two equal styles share one entry in the workbook's style sheet.
/// </summary>
public sealed record ExcelCellStyle
{
    #region Properties
    /// <summary>
    /// get - The font family name.
    /// </summary>
    public string FontName { get; init; } = "Calibri";

    /// <summary>
    /// get - The font size in points.
    /// </summary>
    public double FontSize { get; init; } = 11;

    /// <summary>
    /// get - Whether the font is bold.
    /// </summary>
    public bool Bold { get; init; }

    /// <summary>
    /// get - The font colour as an RGB hex string (e.g. "000000").
    /// </summary>
    public string FontColor { get; init; } = "000000";

    /// <summary>
    /// get - The solid background fill colour as an RGB hex string, or null for no fill.
    /// </summary>
    public string? FillColor { get; init; }

    /// <summary>
    /// get - The horizontal alignment.
    /// </summary>
    public ExcelHorizontalAlignment HorizontalAlignment { get; init; }

    /// <summary>
    /// get - The vertical alignment.
    /// </summary>
    public ExcelVerticalAlignment VerticalAlignment { get; init; }

    /// <summary>
    /// get - Whether text wraps within the cell.
    /// </summary>
    public bool WrapText { get; init; }

    /// <summary>
    /// get - The number format code (e.g. "#,##0", "0.00%"), or null for General.
    /// </summary>
    public string? NumberFormat { get; init; }
    #endregion
}
