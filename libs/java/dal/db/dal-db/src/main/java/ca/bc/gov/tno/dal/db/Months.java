package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides an option for each month. This is a flag enum which allows for more
 * than one or more selected values.
 */
public enum Months implements IEnumValue<Integer> {
  /** Not Applicable */
  NA(0),
  /** January */
  January(1),
  /** February */
  February(2),
  /** March */
  March(4),
  /** April */
  April(8),
  /** May */
  May(16),
  /** June */
  June(32),
  /** July */
  July(64),
  /** August */
  August(128),
  /** September */
  September(256),
  /** October */
  October(512),
  /** November */
  November(1024),
  /** December */
  December(2048);

  private final int value;

  /**
   * Creates a new instance of an Months enum value, initializes with specified
   * value.
   */
  Months(final int newValue) {
    value = newValue;
  }

  /**
   * Get the current enum value.
   */
  public Integer getValue() {
    return value;
  }

  /**
   * Get all the month enum values.
   */
  public static final EnumSet<Months> All = EnumSet.allOf(Months.class);
}
