package ca.bc.gov.tno.dal.db.models;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for different logical operators.
 */
public enum LogicalOperators implements IEnumValue<Integer> {
  /** Contains */
  Contains(0),
  /** Equal */
  Equals(1),
  /** Not equal */
  NotEqual(2),
  /** Greater than */
  GreaterThan(3),
  /** Greater than or equal */
  GreaterThanOrEqual(4),
  /** Less than */
  LessThan(5),
  /** Less than or equal */
  LessThanOrEqual(6);

  private final int value;

  /**
   * Creates a new instance of an LogicalOperators enum value, initializes with
   * specified value.
   */
  LogicalOperators(final int newValue) {
    value = newValue;
  }

  /**
   * Get the current eum value.
   */
  public Integer getValue() {
    return value;
  }

  /**
   * Get all the logical operators enum values.
   */
  public static final EnumSet<LogicalOperators> All = EnumSet.allOf(LogicalOperators.class);
}
