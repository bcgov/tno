package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for different types of authentication.
 */
public enum AuthenticationTypes implements IEnumValue {
  Token(0), User(1);

  private final int value;

  AuthenticationTypes(final int newValue) {
    value = newValue;
  }

  public int getValue() {
    return value;
  }

  public static final EnumSet<SyndicationTypes> All = EnumSet.allOf(SyndicationTypes.class);
}
