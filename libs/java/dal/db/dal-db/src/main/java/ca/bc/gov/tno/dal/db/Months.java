package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.dal.db.entities.IEnumValue;

/**
 * Provides options for which day of the week.
 */
public enum Months implements IEnumValue {
  NA(0), January(1), February(2), March(4), April(8), May(16), June(32), July(64), August(128), September(256),
  October(512), November(1024), December(2048);

  private final int value;

  Months(final int newValue) {
    value = newValue;
  }

  public int getValue() {
    return value;
  }

  public static final EnumSet<Months> All = EnumSet.allOf(Months.class);
}
