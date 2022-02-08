package ca.bc.gov.tno.dal.db.models;

import java.text.SimpleDateFormat;
import java.util.Date;

/**
 * FilterParam class, provides a way to filter on a property and value.
 */
public class FilterParam<T> {
  private final Class<T> type;
  private final String table;
  private final String column;
  private final LogicalOperators logicalOperator;
  private final T value;

  /**
   * Creates a new instance of a FilterParam, initializes with specified
   * parameters.
   * 
   * @param type            The value type.
   * @param column          The column name of the property to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value to filter on.
   */
  public FilterParam(Class<T> type, final String column, final LogicalOperators logicalOperator,
      final T value) {
    this(type, "", column, logicalOperator, value);
  }

  /**
   * Creates a new instance of a FilterParam, initializes with specified
   * parameters.
   * 
   * @param type            The value type.
   * @param table           Then table name to filter on.
   * @param column          The column name of the property to filter on.
   * @param logicalOperator The logical operator to compare the value.
   * @param value           The value to filter on.
   */
  @SuppressWarnings("unchecked")
  public FilterParam(Class<T> type, final String table, final String column, final LogicalOperators logicalOperator,
      final T value) {
    if (table == null)
      throw new NullPointerException("Parameter 'table' cannot be null.");
    if (column == null)
      throw new NullPointerException("Parameter 'column' cannot be null.");
    if (column.length() == 0)
      throw new IllegalArgumentException("Parameter 'column' cannot be empty.");

    this.type = type;
    this.table = table.replaceAll("\\s+", "").replaceAll("'", "");
    this.column = column.replaceAll("\\s+", "").replaceAll("'", "");
    this.logicalOperator = logicalOperator;

    if (value instanceof String) {
      this.value = (T) ((String) value).replaceAll("'", "''");
    } else {
      this.value = value;
    }
  }

  /**
   * @return The filter value type.
   */
  public Class<T> getMyType() {
    return this.type;
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
   * @return LogicalOperators return the logicalOperator
   */
  public LogicalOperators getLogicalOperator() {
    return logicalOperator;
  }

  /**
   * @return String return the value
   */
  public T getValue() {
    return value;
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
   * @return Return the filter condition as a string.
   */
  public String toString() {
    return this.toString(null);
  }

  /**
   * @param defaultTable The default table name.
   * @return Return the filter condition as a string.
   */
  public String toString(String defaultTable) {
    if (this.type == String.class) {
      if (this.logicalOperator == LogicalOperators.Contains)
        return String.format("%s%s LIKE '%%%s%%'", getTableName(defaultTable), this.column, this.value);
      else if (this.logicalOperator == LogicalOperators.NotEqual)
        return String.format("%s%s!='%s'", getTableName(defaultTable), this.column, this.value);
      else if (this.logicalOperator == LogicalOperators.GreaterThan)
        return String.format("%s%s>'%s'", getTableName(defaultTable), this.column, this.value);
      else if (this.logicalOperator == LogicalOperators.GreaterThanOrEqual)
        return String.format("%s%s>='%s'", getTableName(defaultTable), this.column, this.value);
      else if (this.logicalOperator == LogicalOperators.LessThan)
        return String.format("%s%s<'%s'", getTableName(defaultTable), this.column, this.value);
      else if (this.logicalOperator == LogicalOperators.LessThanOrEqual)
        return String.format("%s%s<='%s'", getTableName(defaultTable), this.column, this.value);
      else
        return String.format("%s%s='%s'", getTableName(defaultTable), this.column, this.value);
    }

    if (this.type == Date.class) {
      var dFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss Z");
      var date = dFormat.format((Date) this.value);
      if (this.logicalOperator == LogicalOperators.NotEqual)
        return String.format("%s%s!='%s'", getTableName(defaultTable), this.column, date);
      else if (this.logicalOperator == LogicalOperators.GreaterThan)
        return String.format("%s%s>'%s'", getTableName(defaultTable), this.column, date);
      else if (this.logicalOperator == LogicalOperators.GreaterThanOrEqual)
        return String.format("%s%s>='%s'", getTableName(defaultTable), this.column, date);
      else if (this.logicalOperator == LogicalOperators.LessThan)
        return String.format("%s%s<'%s'", getTableName(defaultTable), this.column, date);
      else if (this.logicalOperator == LogicalOperators.LessThanOrEqual)
        return String.format("%s%s<='%s'", getTableName(defaultTable), this.column, date);
      else
        return String.format("%s%s='%s'", getTableName(defaultTable), this.column, date);
    }

    if (this.logicalOperator == LogicalOperators.NotEqual)
      return String.format("%s%s!=%s", getTableName(defaultTable), this.column, this.value);
    else if (this.logicalOperator == LogicalOperators.GreaterThan)
      return String.format("%s%s>%s", getTableName(defaultTable), this.column, this.value);
    else if (this.logicalOperator == LogicalOperators.GreaterThanOrEqual)
      return String.format("%s%s>=%s", getTableName(defaultTable), this.column, this.value);
    else if (this.logicalOperator == LogicalOperators.LessThan)
      return String.format("%s%s<%s", getTableName(defaultTable), this.column, this.value);
    else if (this.logicalOperator == LogicalOperators.LessThanOrEqual)
      return String.format("%s%s<=%s", getTableName(defaultTable), this.column, this.value);
    else
      return String.format("%s%s=%s", getTableName(defaultTable), this.column, this.value);
  }

}
