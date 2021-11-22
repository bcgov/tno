package ca.bc.gov.tno.dal.elastic.models;

import org.springframework.data.elasticsearch.annotations.Field;
import org.springframework.data.elasticsearch.annotations.FieldType;

/**
 * Tag class, provides a model to capture tagged keywords/phrases that make the
 * content searchable.
 */
public class Tag {
  /**
   * Identifies the type of tag.
   * 
   * i.e. This enables the ability to search for a "place" named "John", as
   * opposed to simply searching for "John".
   */
  @Field(type = FieldType.Keyword)
  private String key;

  /**
   * The tag value.
   */
  @Field(type = FieldType.Text)
  private String value;

  /**
   * Creates a new instance of a Tag object.
   */
  public Tag() {
  }

  /**
   * Creates a new instance of a Tag object, initializes with specified parameters
   * 
   * @param key   Key value to identify the type of tag.
   * @param value Value of the tag.
   */
  public Tag(final String key, final String value) {
    this.key = key;
    this.value = value;
  }

  /**
   * @return String return the key
   */
  public String getKey() {
    return key;
  }

  /**
   * @param key the key to set
   */
  public void setKey(String key) {
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