package ca.bc.gov.tno.nlp;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * TagKey enum, provides the tag key options.
 */
public enum NlpStatus implements IEnumValue<String> {
  running("running"), pause("pause"), paused("paused"), resume("resume"), sleeping("sleeping");

  private final String value;

  /**
   * Creates a new instance of a TagKey enum, initializes with specified
   * parameter.
   * 
   * @param newValue
   */
  NlpStatus(final String newValue) {
    value = newValue;
  }

  /**
   * Get the tag value name.
   */
  public String getValue() {
    return value;
  }

  public static final EnumSet<NlpStatus> All = EnumSet.allOf(NlpStatus.class);
}
