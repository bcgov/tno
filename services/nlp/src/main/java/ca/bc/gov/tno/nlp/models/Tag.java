package ca.bc.gov.tno.nlp.models;

/**
 * Tag class, provides a way to capture identifying information that is
 * searchable.
 */
public class Tag {
  /**
   * Identifies the type of tag.
   * 
   * i.e. This enables the ability to search for a "place" named "John", as
   * opposed to simply searching for "John".
   */
  private TagKey key;

  /**
   * The tag value.
   */
  private String value;

  /**
   * Creates a new instance of a Tag object.
   */
  public Tag() {
  }

  /**
   * Creates a new instance of a Tag object, initializes with the specified
   * parameters.
   * 
   * @param key
   * @param value
   */
  public Tag(TagKey key, String value) {
    if (key == null)
      throw new IllegalArgumentException("Parameter 'key' is required.");
    if (value == null || value.length() == 0)
      throw new IllegalArgumentException("Parameter 'value' is required, and cannot be empty.");

    this.key = key;
    this.value = value;
  }

  /**
   * @return TagKey return the key
   */
  public TagKey getKey() {
    return key;
  }

  /**
   * @param key the key to set
   */
  public void setKey(TagKey key) {
    this.key = key;
  }

  /**
   * @return String return the value
   */
  public String getValue() {
    return value;
  }

  /**
   * @param value the value to set
   */
  public void setValue(String value) {
    this.value = value;
  }

}
