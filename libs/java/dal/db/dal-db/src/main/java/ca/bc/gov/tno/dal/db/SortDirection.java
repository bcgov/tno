package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for sorting.
 */
public enum SortDirection implements IEnumValue<String> {
  /** Sort ascending */
  Ascending("asc"),
  /** Sort descending */
  Descending("desc");

  private final String value;

  /**
   * Creates a new instance of an SortDirection enum value, initializes with
   * specified value.
   */
  SortDirection(final String newValue) {
    value = newValue;
  }

  /**
   * Get the current eum value.
   */
  public String getValue() {
    return value;
  }

  /**
   * Get all the sorting direction enum values.
   */
  public static final EnumSet<SortDirection> All = EnumSet.allOf(SortDirection.class);
}
