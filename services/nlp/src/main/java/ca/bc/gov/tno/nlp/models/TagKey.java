package ca.bc.gov.tno.nlp.models;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * TagKey enum, provides the tag key options.
 * 
 * keyword | category | person | organization | location | activity | thing |
 * date
 */
public enum TagKey implements IEnumValue<String> {
  keyword("keyword"), category("category"), person("person"), organization("organization"), location("location"),
  activity("activity"), thing("thing"), date("date");

  private final String value;

  /**
   * Creates a new instance of a TagKey enum, initializes with specified
   * parameter.
   * 
   * @param newValue
   */
  TagKey(final String newValue) {
    value = newValue;
  }

  /**
   * Get the tag value name.
   */
  public String getValue() {
    return value;
  }

  public static final EnumSet<TagKey> All = EnumSet.allOf(TagKey.class);
}
