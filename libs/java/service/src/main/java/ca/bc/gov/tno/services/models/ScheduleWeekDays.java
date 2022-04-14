package ca.bc.gov.tno.services.models;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for which day of the week. This is a flag enum which allows
 * for more than one or more selected values.
 */
public enum ScheduleWeekDays implements IEnumValue<Integer> {
  /** Not Applicable */
  NA(0),
  /** Sunday */
  Sunday(1),
  /** Monday */
  Monday(2),
  /** Tuesday */
  Tuesday(4),
  /** Wednesday */
  Wednesday(8),
  /** Thursday */
  Thursday(16),
  /** Friday */
  Friday(32),
  /** Saturday */
  Saturday(64);

  private final int value;

  /**
   * Creates a new instance of an WeekDays enum value, initializes with specified
   * value.
   */
  ScheduleWeekDays(final int newValue) {
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
   * Get all the week day enum values.
   */
  public static final EnumSet<ScheduleWeekDays> All = EnumSet.allOf(ScheduleWeekDays.class);
}
