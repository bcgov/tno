package ca.bc.gov.tno.dal.db.models;

import ca.bc.gov.tno.dal.db.SortDirection;

/**
 * SortParm class, provides a way to control the sort of a column.
 */
public final class SortParam {
  private final String table;
  private final String column;
  private final SortDirection direction;

  /**
   * Creates new instance of a SortParam object, initializes with specified
   * parameters.
   * 
   * @param column The column name.
   */
  public SortParam(final String column) {
    this(column, SortDirection.Ascending);
  }

  /**
   * Creates new instance of a SortParam object, initializes with specified
   * parameters.
   * 
   * @param table  The table name.
   * @param column The column name.
   */
  public SortParam(final String table, final String column) {
    this(table, column, SortDirection.Ascending);
  }

  /**
   * Creates new instance of a SortParam object, initializes with specified
   * parameters.
   * 
   * @param column    The column name.
   * @param direction The sorting order.
   */
  public SortParam(final String column, final SortDirection direction) {
    this("", column, direction);
  }

  /**
   * Creates new instance of a SortParam object, initializes with specified
   * parameters.
   * 
   * @param table     The table name.
   * @param column    The column name.
   * @param direction The sorting order.
   */
  public SortParam(final String table, final String column, final String direction) {
    if (table == null)
      throw new NullPointerException("Parameter 'table' cannot be null.");
    if (column == null)
      throw new NullPointerException("Parameter 'column' cannot be null.");
    if (column.length() == 0)
      throw new IllegalArgumentException("Parameter 'column' cannot be empty.");

    var dir = direction == null ? "" : direction;

    this.table = table.replaceAll("\\s+", "");
    this.column = column.replaceAll("\\s+", "");

    switch (dir.toLowerCase()) {
      case "d":
      case "desc":
      case "descend":
      case "descending":
        this.direction = SortDirection.Descending;
        break;
      case "a":
      case "asc":
      case "ascend":
      case "ascending":
      default:
        this.direction = SortDirection.Ascending;
        break;
    }
  }

  /**
   * Creates new instance of a SortParam object, initializes with specified
   * parameters.
   * 
   * @param table     The table name.
   * @param column    The column name.
   * @param direction The sorting order.
   */
  public SortParam(final String table, final String column, final SortDirection direction) {
    if (table == null)
      throw new NullPointerException("Parameter 'table' cannot be null.");
    if (column == null)
      throw new NullPointerException("Parameter 'column' cannot be null.");
    if (column.length() == 0)
      throw new IllegalArgumentException("Parameter 'column' cannot be empty.");
    if (direction == null)
      throw new NullPointerException("Parameter 'direction' cannot be null.");

    // If a column name has spaces remove them to ensure no SQL injection.
    this.table = table.replaceAll("\\s+", "");
    this.column = column.replaceAll("\\s+", "");
    this.direction = direction;
  }

  /**
   * @return String return the table
   */
  public String getTable() {
    return table;
  }

  /**
   * @return String return the column
   */
  public String getColumn() {
    return column;
  }

  /**
   * @return SortDirection return the direction
   */
  public SortDirection getDirection() {
    return direction;
  }

  /**
   * @return Generate the table name append value.
   */
  private String getTableName(String defaultTable) {
    if (this.table != null && this.table.length() != 0)
      return this.table + ".";
    if (defaultTable != null && defaultTable.length() != 0)
      return defaultTable + ".";
    return "";
  }

  /**
   * @return Returns the sort order as a string.
   */
  public String toString() {
    return this.toString(null);
  }

  /**
   * @param defaultTable The default table name.
   * @return Returns the sort order as a string.
   */
  public String toString(String defaultTable) {
    return String.format("%s%s %s", getTableName(defaultTable), this.column, this.direction.getValue());
  }
}
