package ca.bc.gov.tno.services.models;

import java.util.ArrayList;
import java.util.List;

/**
 * Tag class, provides a way to manage tags.
 */
public class Tag extends AuditColumns {
  /**
   * Primary key to identify the tag.
   */
  private String id;

  /**
   * A unique name to identify the tag.
   */
  private String name;

  /**
   * A description of the tag.
   */
  private String description = "";

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean isEnabled = true;

  /**
   * A collection of content tags that belong to this tag.
   */
  private List<ContentTag> contentTags = new ArrayList<>();

  /**
   * Creates a new instance of a Tag object.
   */
  public Tag() {

  }

  /**
   * Creates a new instance of a Tag object, initializes with specified
   * parameters.
   *
   * @param id   Primary key
   * @param name Unique name
   */
  public Tag(String id, String name) {
    if (id == null)
      throw new NullPointerException("Parameter 'id' cannot be null.");
    if (id.length() == 0)
      throw new IllegalArgumentException("Parameter 'id' cannot be empty.");
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

    this.id = id;
    this.name = name;
  }

  /**
   * Creates a new instance of a Tag object, initializes with specified
   * parameters.
   *
   * @param id      Primary key
   * @param name    Unique name
   * @param version Row version value
   */
  public Tag(String id, String name, long version) {
    this(id, name);
    this.setVersion(version);
  }

  /**
   * @return String return the id
   */
  public String getId() {
    return id;
  }

  /**
   * @return String return the name
   */
  public String getName() {
    return name;
  }

  /**
   * @param name the name to set
   */
  public void setName(String name) {
    this.name = name;
  }

  /**
   * @return String return the description
   */
  public String getDescription() {
    return description;
  }

  /**
   * @param description the description to set
   */
  public void setDescription(String description) {
    this.description = description;
  }

  /**
   * @return int return the sortOrder
   */
  public int getSortOrder() {
    return sortOrder;
  }

  /**
   * @param sortOrder the sortOrder to set
   */
  public void setSortOrder(int sortOrder) {
    this.sortOrder = sortOrder;
  }

  /**
   * @return boolean return the enabled
   */
  public boolean getIsEnabled() {
    return isEnabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.isEnabled = enabled;
  }

  /**
   * @return List{ContentTag} return the contentTags
   */
  public List<ContentTag> getContentTags() {
    return contentTags;
  }

  /**
   * @param contentTags the contentTags to set
   */
  public void setContentTags(List<ContentTag> contentTags) {
    this.contentTags = contentTags;
  }

}
