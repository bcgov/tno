namespace TNO.DAL.Models;

public class SortParam
{
    #region Properties
    public string? Table { get; set; }

    public string Column { get; set; } = "";

    public SortDirection Direction { get; set; }
    #endregion

    #region Constructors
    public SortParam() { }

    public SortParam(string column, SortDirection direction = SortDirection.Ascending)
    {
        this.Column = column;
        this.Direction = direction;
    }

    public SortParam(string table, string column, SortDirection direction = SortDirection.Ascending) : this(column, direction)
    {
        this.Table = table;
    }
    #endregion
}
