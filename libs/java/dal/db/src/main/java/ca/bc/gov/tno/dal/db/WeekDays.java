package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.dal.db.entities.IEnumValue;

/**
 * Provides options for which day of the week.
 */
public enum WeekDays implements IEnumValue {
  NA(0), Sunday(1), Monday(2), Tuesday(4), Wednesday(8), Thursday(16), Friday(32), Saturday(64);

  private final int value;

  WeekDays(final int newValue) {
    value = newValue;
  }

  public int getValue() {
    return value;
  }

  public static final EnumSet<WeekDays> All = EnumSet.allOf(WeekDays.class);
}
