package ca.bc.gov.tno;

import java.util.EnumSet;

/**
 * TagKey enum, provides the tag key options.
 * 
 * keyword | category | person | organization | location | activity | thing |
 * date
 */
public enum TagKey implements IEnumValue<String> {
  /**
   * Keyword tag
   */
  keyword("keyword"),
  /**
   * Category tag
   */
  category("category"),
  /**
   * Person tag
   */
  person("person"),
  /**
   * Organization tag
   */
  organization("organization"),
  /**
   * Location tag
   */
  location("location"),
  /**
   * Activity tag
   */
  activity("activity"),
  /**
   * Entity tag
   */
  thing("entity"),
  /**
   * Date tag
   */
  date("date");

  private final String value;

  /**
   * Creates a new instance of a TagKey enum, initializes with specified
   * parameter.
   * 
   * @param newValue The value of the current enum.
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

  /**
   * Get all of the tag key enum values.
   */
  public static final EnumSet<TagKey> All = EnumSet.allOf(TagKey.class);
}
