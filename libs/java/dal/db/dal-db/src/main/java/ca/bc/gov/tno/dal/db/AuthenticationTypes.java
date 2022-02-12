package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for different types of authentication.
 */
public enum AuthenticationTypes implements IEnumValue<Integer> {
  /**
   * Token required.
   */
  Token(0),
  /**
   * User account required.
   */
  User(1);

  private final int value;

  /**
   * Creates a new instance of an AuthenticationTypes enum value, initializes with
   * specified value.
   */
  AuthenticationTypes(final int newValue) {
    value = newValue;
  }

  /**
   * Get the value of the current enum.
   */
  @JsonValue
  public Integer getValue() {
    return value;
  }

  /**
   * Return all values of the enum.
   */
  public static final EnumSet<AuthenticationTypes> All = EnumSet.allOf(AuthenticationTypes.class);
}
