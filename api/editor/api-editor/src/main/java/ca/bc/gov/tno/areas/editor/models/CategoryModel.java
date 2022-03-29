package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.Category;
import ca.bc.gov.tno.models.AuditColumnModel;

public class CategoryModel extends AuditColumnModel {
  /**
   * Primary key to identify the category.
   */
  private int id;

  /**
   * A unique name to identify the category.
   */
  private String name;

  /**
   * A description of the category.
   */
  private String description = "";

  /**
   * The order to display.
   */
  private int sortOrder;

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * The score given to the category.
   */
  public int score;

  /**
   * A collection of role categories that belong to this category.
   */
  // private List<ContentCategory> contentCategories = new ArrayList<>();

  public CategoryModel() {
  }

  public CategoryModel(Category entity) {
    this(entity, 0, entity.getVersion());
  }

  public CategoryModel(Category entity, int score, long version) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.isEnabled();
      this.sortOrder = entity.getSortOrder();
    }
    this.score = score;
    this.setVersion(version);
  }

  /**
   * @return int the id
   */
  public int getId() {
    return id;
  }

  /**
   * @param id the id to set
   */
  public void setId(int id) {
    this.id = id;
  }

  /**
   * @return String the name
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
   * @return String the description
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
   * @return boolean the enabled
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
   * @return int the sortOrder
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
   * @return int the score
   */
  public int getScore() {
    return score;
  }

  /**
   * @param score the score to set
   */
  public void setScore(int score) {
    this.score = score;
  }

}
