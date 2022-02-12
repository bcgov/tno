package ca.bc.gov.tno.dal.db;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides value type to control the type of value that can be entered.
 */
public enum ValueType implements IEnumValue<Integer> {
  /**
   * Boolean values.
   */
  Boolean(0),
  /**
   * String values.
   */
  String(1),
  /**
   * Text values.
   */
  Text(2),
  /**
   * Numeric values.
   */
  Numeric(3);

  private final int value;

  /**
   * Creates a new instance of an ValueType enum value, initializes with
   * specified value.
   */
  ValueType(final int newValue) {
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
   * Get all the kafka message status enum values.
   */
  public static final EnumSet<ValueType> All = EnumSet.allOf(ValueType.class);
}
