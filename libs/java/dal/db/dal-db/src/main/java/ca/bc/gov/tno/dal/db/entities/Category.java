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
 * Category class, provides a way to manage categories.
 */
@Entity
@Table(name = "\"Category\"")
public class Category extends AuditColumns {
  /**
   * Primary key to identify the category.
   */
  @Id
  @GeneratedValue(strategy = GenerationType.SEQUENCE, generator = "seq_Category")
  @SequenceGenerator(name = "seq_Category", allocationSize = 1)
  @Column(name = "\"id\"", nullable = false)
  private int id;

  /**
   * A unique name to identify the category.
   */
  @Column(name = "\"name\"", nullable = false)
  private String name;

  /**
   * A description of the category.
   */
  @Column(name = "\"description\"")
  private String description = "";

  /**
   * The order to display.
   */
  @Column(name = "\"sortOrder\"", nullable = false)
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  @Column(name = "\"isEnabled\"", nullable = false)
  private boolean enabled = true;

  /**
   * A collection of role categories that belong to this category.
   */
  @JsonBackReference("contentCategories")
  @OneToMany(mappedBy = "category", fetch = FetchType.LAZY)
  private List<ContentCategory> contentCategories = new ArrayList<>();

  /**
   * Creates a new instance of a Category object.
   */
  public Category() {

  }

  /**
   * Creates a new instance of a Category object, initializes with specified
   * parameters.
   * 
   * @param id   Primary key
   * @param name Unique name
   */
  public Category(int id, String name) {
    if (name == null)
      throw new NullPointerException("Parameter 'name' cannot be null.");
    if (name.length() == 0)
      throw new IllegalArgumentException("Parameter 'name' cannot be empty.");

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
   * @param enabled the enabled to set
   */
  public void setEnabled(boolean enabled) {
    this.enabled = enabled;
  }

  /**
   * @return List{ContentCategory} return the contentCategories
   */
  public List<ContentCategory> getContentCategories() {
    return contentCategories;
  }

  /**
   * @param contentCategories the contentCategories to set
   */
  public void setContentCategories(List<ContentCategory> contentCategories) {
    this.contentCategories = contentCategories;
  }

}
