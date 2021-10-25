package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for different syndication types.
 */
public enum SyndicationTypes implements IEnumValue {
  RSS(0), ATOM(1);

  private final int value;

  SyndicationTypes(final int newValue) {
    value = newValue;
  }

  public int getValue() {
    return value;
  }

  public static final EnumSet<SyndicationTypes> All = EnumSet.allOf(SyndicationTypes.class);
}
