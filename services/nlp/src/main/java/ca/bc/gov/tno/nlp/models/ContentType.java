package ca.bc.gov.tno.nlp.models;

import java.util.EnumSet;

import ca.bc.gov.tno.IEnumValue;

/**
 * ContentType enum, provides the content type options.
 * 
 * text | audio | video
 */
public enum ContentType implements IEnumValue<String> {
  text("text"), audio("audio"), video("video");

  private final String value;

  /**
   * Creates a new instance of a TagKey enum, initializes with specified
   * parameter.
   * 
   * @param newValue
   */
  ContentType(final String newValue) {
    value = newValue;
  }

  /**
   * Get the tag value name.
   */
  public String getValue() {
    return value;
  }

  public static final EnumSet<ContentType> All = EnumSet.allOf(ContentType.class);
}
