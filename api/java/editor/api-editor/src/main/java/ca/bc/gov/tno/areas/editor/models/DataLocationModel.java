package ca.bc.gov.tno.areas.editor.models;

import ca.bc.gov.tno.dal.db.entities.DataLocation;
import ca.bc.gov.tno.models.AuditColumnModel;

public class DataLocationModel extends AuditColumnModel {

  /**
   * Primary key to identify the data location.
   */
  private int id;

  /**
   * A unique name to identify the data location.
   */
  private String name;

  /**
   * A description of the data location.
   */
  private String description = "";

  /**
   * Whether this record is enabled or disabled.
   */
  private boolean enabled = true;

  /**
   * A collection of data sources of this type.
   */
  // private List<DataSource> data_sources = new ArrayList<>();

  public DataLocationModel() {
  }

  public DataLocationModel(DataLocation entity) {
    super(entity);

    if (entity != null) {
      this.id = entity.getId();
      this.name = entity.getName();
      this.description = entity.getDescription();
      this.enabled = entity.getIsEnabled();
    }
  }

  /**
   * @return int return the id
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
  public boolean getIsEnabled() {
    return enabled;
  }

  /**
   * @param enabled the enabled to set
   */
  public void setIsEnabled(boolean enabled) {
    this.enabled = enabled;
  }

}
