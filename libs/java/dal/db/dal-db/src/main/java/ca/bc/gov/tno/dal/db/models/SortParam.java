package ca.bc.gov.tno.dal.db.models;

import ca.bc.gov.tno.dal.db.SortDirection;

/**
 * SortParm class, provides a way to control the sort of a column.
 */
public final class SortParam {
  private String column;

  private SortDirection direction;

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
   * @param column    The column name.
   * @param direction The sorting order.
   */
  public SortParam(final String column, final SortDirection direction) {
    if (column == null)
      throw new NullPointerException("Parameter 'column' cannot be null.");
    if (column.length() == 0)
      throw new IllegalArgumentException("Parameter 'column' cannot be empty.");
    if (direction == null)
      throw new NullPointerException("Parameter 'direction' cannot be null.");

    // If a column name has spaces remove them to ensure no SQL injection.
    this.column = column.replaceAll("\\s+", "");
    this.direction = direction;
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
   * Returns the sort order as a string.
   */
  public String toString() {
    return this.column + " " + this.direction.getValue();
  }
}
