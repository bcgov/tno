package ca.bc.gov.tno;

import java.util.EnumSet;

/**
 * ContentType enum, provides the content type options.
 * 
 * text | audio | video
 */
public enum ContentType implements IEnumValue<String> {
  /**
   * Text content type.
   */
  text("text"),
  /**
   * Audio content type.
   */
  audio("audio"),
  /**
   * Video content type.
   */
  video("video");

  private final String value;

  /**
   * Creates a new instance of a TagKey enum, initializes with specified
   * parameter.
   * 
   * @param newValue The value of the current enum.
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

  /**
   * Get all of the content type enum values.
   */
  public static final EnumSet<ContentType> All = EnumSet.allOf(ContentType.class);
}
