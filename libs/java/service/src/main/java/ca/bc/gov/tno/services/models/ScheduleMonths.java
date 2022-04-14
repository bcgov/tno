package ca.bc.gov.tno.services.models;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides an option for each month. This is a flag enum which allows for more
 * than one or more selected values.
 */
public enum ScheduleMonths implements IEnumValue<Integer> {
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
  ScheduleMonths(final int newValue) {
    value = newValue;
  }

  /**
   * Get the current enum value.
   */
  @JsonValue
  public Integer getValue() {
    return value;
  }

  /**
   * Get all the month enum values.
   */
  public static final EnumSet<ScheduleMonths> All = EnumSet.allOf(ScheduleMonths.class);
}
