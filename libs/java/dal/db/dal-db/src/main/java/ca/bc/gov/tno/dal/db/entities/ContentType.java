package ca.bc.gov.tno.dal.db.entities;

import java.util.ArrayList;
import java.util.List;

import javax.persistence.Column;
import javax.persistence.Entity;
import javax.persistence.FetchType;
import javax.persistence.GeneratedValue;
import javax.persistence.GenerationType;
import javax.persistence.Id;
import javax.persistence.OneToMany;
import javax.persistence.SequenceGenerator;
import javax.persistence.Table;

import com.fasterxml.jackson.annotation.JsonBackReference;

import ca.bc.gov.tno.dal.db.AuditColumns;

/**
 * ContentType class, provides a way to identify the different content
 * types.
 */
@Entity
@Table(name = "\"ContentType\"")
public class ContentType extends AuditColumns {
  /**
   * Primary key to identify the content type.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_ContentType")
  @SequenceGenerator(name = "seq_ContentType", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the content type.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the content type.
   */
  @Column(name = "\"description\"")
  private String description;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled;

  /**
   * Sort order of records.
   */
  @Column(name = "\"sortOrder\"", nullable = false)
  private int sortOrder;

  /**
   * A collection of content of this type.
   */
  @JsonBackReference("contents")
  @OneToMany(mappedBy = "contentType", fetch = FetchType.LAZY)
  private List<Content> contents = new ArrayList<>();

  /**
   * Creates a new instance of a ContentType object.
   */
  public ContentType() {

  }

  /**
   * Creates a new instance of a ContentType object, initializes with specified
   * parameters.
   * 
   * @param id   Primary key
   * @param name Unique name
   */
  public ContentType(int id, String name) {
    this.id = id;
    this.name = name;
  }

  /**
   * @return int return the id
   */
  public int getId() {
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
   * @return boolean return the enabled
   */
  public boolean isEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
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
   * @return List{Content} return the contents
   */
  public List<Content> getContents() {
    return contents;
  }

  /**
   * @param contents the contents to set
   */
  public void setContents(List<Content> contents) {
    this.contents = contents;
  }

}
