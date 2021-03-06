package ca.bc.gov.tno.services.models;

import java.util.EnumSet;

import com.fasterxml.jackson.annotation.JsonValue;

import ca.bc.gov.tno.IEnumValue;

/**
 * Provides options for different syndication types.
 */
public enum SyndicationTypes implements IEnumValue<Integer> {
  /** RSS Syndication feed */
  RSS(0),
  /** ATOM Syndication feed */
  ATOM(1),
  /** TV */
  TV(2),
  /** Radio */
  Radio(3);

  private final int value;

  /**
   * Creates a new instance of an SyndicationTypes enum value, initializes with
   * specified value.
   */
  SyndicationTypes(final int newValue) {
    value = newValue;
  }

  /**
   * Get the current eum value.
   */
  @JsonValue
  public Integer getValue() {
    return value;
  }

  /**
   * Get all the syndication type enum values.
   */
  public static final EnumSet<SyndicationTypes> All = EnumSet.allOf(SyndicationTypes.class);
}
