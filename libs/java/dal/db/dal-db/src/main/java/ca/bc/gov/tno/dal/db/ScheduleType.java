package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for which day of the week. This is a flag enum which allows
 * for more than one or more selected values.
 */
public enum ScheduleType implements IEnumValue<Integer> {
  /** Repeating will run a continuously repeating schedule */
  Repeating(0),
  /** Managed will run between a start and stop time */
  Managed(1);

  private final int value;

  /**
   * Creates a new instance of an ScheduleType enum value, initializes with
   * specified
   * value.
   */
  ScheduleType(final int newValue) {
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
  public static final EnumSet<ScheduleType> All = EnumSet.allOf(ScheduleType.class);
}
