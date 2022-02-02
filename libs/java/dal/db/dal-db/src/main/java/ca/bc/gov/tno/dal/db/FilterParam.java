package ca.bc.gov.tno.dal.db;

public class FilterParam<T> {
  private String name;
  private LogicalOperators logicalOperator;
  private T value;

  public FilterParam(final String name, final LogicalOperators logicalOperator, final T value) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.name = name.replaceAll("\\s+", "");
    this.logicalOperator = logicalOperator;
    this.value = value;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
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

}
